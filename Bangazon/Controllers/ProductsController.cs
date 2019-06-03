using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bangazon.Data;
using Bangazon.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Bangazon.Models.ProductViewModels;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Hosting.Internal;
using Microsoft.AspNetCore.Hosting;

namespace Bangazon.Controllers
{
    public class ProductsController : Controller

    {
        private readonly IHostingEnvironment _hostingEnvironment;

        private readonly UserManager<ApplicationUser> _userManager;
        public ProductsController(ApplicationDbContext ctx,
                          UserManager<ApplicationUser> userManager,
                          IHostingEnvironment hostingEnvironment)
        {
            _userManager = userManager;
            _context = ctx;
            _hostingEnvironment = hostingEnvironment;
        }
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        private readonly ApplicationDbContext _context;

        //public ProductsController(ApplicationDbContext context)
        //{
        //    _context = context;
        //}

        // GET: Products
        [Authorize]
        public async Task<IActionResult> Index(string SearchProduct)
        {
            var currentUser = await GetCurrentUserAsync();
            ViewBag.UserId = currentUser.Id;
            // if searchBox is not empty then filter product list
            if (SearchProduct != null)
            {
                var applicationDbContext1 = _context.Product.Include(p => p.ProductType)
                   .Include(p => p.User)
                   .Where(p => p.Title.Contains(SearchProduct) || p.City.ToLower() == SearchProduct.ToLower())
                   .OrderByDescending(p => p.DateCreated);

                return View(await applicationDbContext1.ToListAsync());
            }
            //if not show all products
            var applicationDbContext = _context.Product.Include(p => p.ProductType).Include(p => p.User).OrderByDescending(p => p.DateCreated).Take(20);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductType)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            UploadImageViewModel viewproduct = new UploadImageViewModel();
            viewproduct.product = new Product();
            ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label");
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id");
            return View(viewproduct);
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,DateCreated,Description,Title,Price,Quantity,UserId,City,ImagePath,ProductTypeId, ImageFile")] UploadImageViewModel viewproduct )
        {
            viewproduct.product = new Product();
            // addind current dateTime
            viewproduct.product.DateCreated = DateTime.Now;
            ModelState.Remove("UserId");
            //if product type is 0, give the error message
            viewproduct.product.Description = "Description";
            viewproduct.product.Title = "Title";
            viewproduct.product.Price = 1;
            viewproduct.product.Quantity = 1;
            viewproduct.product.City = "City";
            viewproduct.product.ProductTypeId = 1;
            if (viewproduct.product.ProductTypeId == 0)
            {
                ViewBag.Message = string.Format("Please select the Category");

                ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label");
                ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id");
                return View();
            }
            if (ModelState.IsValid)
            {
                // adding current userId
                var user = await GetCurrentUserAsync();
                viewproduct.product.UserId = user.Id;

                if (viewproduct.ImageFile.Length > 0)
                {
                    // don't rely on or trust the FileName property without validation
                    //**Warning**: The following code uses `GetTempFileName`, which throws
                    // an `IOException` if more than 65535 files are created without 
                    // deleting previous temporary files. A real app should either delete
                    // temporary files or use `GetTempPath` and `GetRandomFileName` 
                    // to create temporary file names.
                    var fileName = Path.GetFileName(viewproduct.ImageFile.FileName);
                    Path.GetTempFileName();
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);
                    //var filePath = Path.GetTempFileName();
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await viewproduct.ImageFile.CopyToAsync(stream);
                        // validate file, then move to CDN or public folder
                    }

                    viewproduct.product.ImagePath = viewproduct.ImageFile.FileName;
                }
                

              _context.Add(viewproduct.product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label", viewproduct.product.ProductTypeId);
            //ViewData["ProductTypeId"].Add
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", viewproduct.product.UserId);
            return View(viewproduct);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label", product.ProductTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", product.UserId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,DateCreated,Description,Title,Price,Quantity,UserId,City,ImagePath,ProductTypeId")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductTypeId"] = new SelectList(_context.ProductType, "ProductTypeId", "Label", product.ProductTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", product.UserId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductType)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            _context.Product.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
    }
}