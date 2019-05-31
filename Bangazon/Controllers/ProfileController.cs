using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bangazon.Data;
using Bangazon.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bangazon.Controllers
{
    public class ProfileController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        public ProfileController(ApplicationDbContext ctx,
                          UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = ctx;
        }
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        private readonly ApplicationDbContext _context;

        [Authorize]
        // Only Users logged in can view a profile
        // GET: Profile
        public ActionResult Index()
        {
            return View();
        }

        // GET: Profile/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Profile/CreatePaymentType
        public IActionResult CreatePaymentType()
        {
            
            return View();
        }

        // POST: Profile/CreatePaymentType
        // Allows users to add a new payment type

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePaymentType([Bind("PaymentTypeId,DateCreated,Description,UserId,AccountNumber")] PaymentType paymenttype)
        {
            //adds current date to payment type
            paymenttype.DateCreated = DateTime.Now;
            //removes userId from  ModelState so that it will be valid
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                paymenttype.UserId = user.Id;

                _context.Add(paymenttype);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", paymenttype.UserId);
            return View(paymenttype);
        }

        // GET: Profile/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Profile/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Profile/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Profile/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}