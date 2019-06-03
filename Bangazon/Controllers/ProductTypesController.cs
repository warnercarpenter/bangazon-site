using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bangazon.Data;
using Bangazon.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;


namespace Bangazon.Controllers
{
    public class ProductTypesController : Controller
    {

        private readonly IConfiguration _config;
        private string _connectionString;
        private readonly ApplicationDbContext _context;

        public ProductTypesController(IConfiguration config,
            ApplicationDbContext context)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _context = context;
        }

        public SqlConnection Connection => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        // GET: ProductTypes
        public async Task<IActionResult> Index()
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"WITH MyRowSet
                                        AS
                                        (
                                        SELECT p.Title, p.ProductId, pt.Label, pt.ProductTypeId, p.Price, p.ImagePath,
                                        ROW_NUMBER() OVER (PARTITION BY pt.ProductTypeId ORDER BY p.DateCreated DESC) AS RowNum 
                                        from Product p
                                        join ProductType pt on p.ProductTypeId = pt.ProductTypeId
                                        )
                                        SELECT * FROM MyRowSet WHERE RowNum <= 3";
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Product> products = new List<Product>();
                    while (reader.Read())
                    {
                        Product product = new Product
                        {
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),

                            ProductType = new ProductType
                            {
                                Label = reader.GetString(reader.GetOrdinal("Label")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId"))
                            }
                        };

                        products.Add(product);
                    }

                    ViewBag.Products = products;

                    reader.Close();

                    return View(await _context.ProductType.Include(pt => pt.Products).ToListAsync());
                }
            }
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            //using _context to query the database
            var productType = await _context.ProductType
                //using singleordefaultasync allows us to get one category type
                .SingleOrDefaultAsync(p => p.ProductTypeId == id);

            var productList = _context.Product
                //the where is getting all of the products related to one category
                .Where(p => p.ProductType.ProductTypeId == productType.ProductTypeId)
                .OrderByDescending(p => p.DateCreated);
            //ViewData is used to access our product list
            ViewData["productList"] = productList;
            return View(productType);
        }



        // POST: ProductTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductTypeId,Label")] ProductType productType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(productType);
        }

        // GET: ProductTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productType = await _context.ProductType.FindAsync(id);
            if (productType == null)
            {
                return NotFound();
            }
            return View(productType);
        }

        // POST: ProductTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductTypeId,Label")] ProductType productType)
        {
            if (id != productType.ProductTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductTypeExists(productType.ProductTypeId))
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
            return View(productType);
        }

        // GET: ProductTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productType = await _context.ProductType
                .FirstOrDefaultAsync(m => m.ProductTypeId == id);
            if (productType == null)
            {
                return NotFound();
            }

            return View(productType);
        }

        // POST: ProductTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productType = await _context.ProductType.FindAsync(id);
            _context.ProductType.Remove(productType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductTypeExists(int id)
        {
            return _context.ProductType.Any(e => e.ProductTypeId == id);
        }
    }
}
