using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bangazon.Data;
using Bangazon.Models;
using Bangazon.Models.ViewModel;

namespace Bangazon.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            return View();
        }

        public IActionResult IncompleteOrders()
        {
            //an instance of viewModel
            MultipleOrder usersView = new MultipleOrder();
            usersView.Users = new List<ApplicationUser>();

            // fetch all data of all orders and their details 
              usersView.Users = _context.ApplicationUsers
                .Include(au => au.Orders)
                    .ThenInclude(o => o.OrderProducts)
                        .ThenInclude(op => op.Product)
                            .ThenInclude(p => p.ProductType)

                .Where(au => au.Orders.Any(o => o.DateCompleted == null)).ToList();

            //persons.Where(p => p.Locations.Any(l => searchIds.Any(id => l.Id == id)));

            if (usersView.Users == null)
            {
                return NotFound();
            }

            return View(usersView);
        }

        public IActionResult MultipleOrders()
        {

            //Get users and include Orders
            var users = _context.ApplicationUsers
                .Include(au => au.Orders);

            //Only include active orders
            foreach (var user in users)
            {
                user.Orders = user.Orders.Where(o => o.DateCompleted == null).ToList();
            }

            if (users == null)
            {
                return NotFound();
            }

            return View(users);
        }

        public IActionResult AbandonedProductTypes()
        {

            //Get Al Product Types and Inclide Products, OrderProducts, and Orders
            List<ProductType> productTypes = _context.ProductType
                .Include(pt => pt.Products)
                .ThenInclude(p => p.OrderProducts)
                .ThenInclude(op => op.Order)
                .ToList();

            //Loop through each Product Type
            foreach (ProductType pt in productTypes)
            {
                //Loop through each Product in each Product Type
                foreach (Product product in pt.Products)
                {
                    //Limit OrderProducts to only contain unfulfilled orders
                    product.OrderProducts = product.OrderProducts.Where(op => op.Order.DateCompleted == null).ToList();
                    //Set Quantity of each product to the Count of the new OrderProductList
                    product.Quantity = product.OrderProducts.Count();
                }

                //Set the Quantity of the Product Type to the sum of the quantities of each Product
                pt.Quantity = pt.Products.Sum(p => p.Quantity);
            }

            //Create a new list which is the same as the original PT list but ordered by quanitity. Only take the top five.
            List<ProductType> newProductTypes = productTypes.OrderByDescending(pt => pt.Quantity).Take(5).ToList();

            if (newProductTypes == null)
            {
                return NotFound();
            }

            return View(newProductTypes);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber");
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,DateCreated,DateCompleted,UserId,PaymentTypeId")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber", order.PaymentTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", order.UserId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber", order.PaymentTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", order.UserId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,DateCreated,DateCompleted,UserId,PaymentTypeId")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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
            ViewData["PaymentTypeId"] = new SelectList(_context.PaymentType, "PaymentTypeId", "AccountNumber", order.PaymentTypeId);
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", order.UserId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.PaymentType)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Order.FindAsync(id);
            _context.Order.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.OrderId == id);
        }

        public async Task<IActionResult> DeleteOrderProduct(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderProduct = await _context.OrderProduct.Include(op => op.Product)
                .FirstOrDefaultAsync(m => m.OrderProductId == id);
            if (orderProduct == null)
            {
                return NotFound();
            }

            return View(orderProduct);
        }

        [HttpPost, ActionName("DeleteOrderProduct")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrderProductConfirmed(int id)
        {
            var orderProduct = await _context.OrderProduct.FindAsync(id);
            _context.OrderProduct.Remove(orderProduct);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
