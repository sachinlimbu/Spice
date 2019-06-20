using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;

namespace Spice.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext context;

        public HomeController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task<IActionResult> Index()
        {

            IndexViewModel indexVM = new IndexViewModel()
            {
                MenuItems = await context.MenuItems.Include(x => x.Category).Include(x => x.SubCategory).ToListAsync(),
                Categories = await context.Categories.ToListAsync(),
                Coupons = await context.Coupons.Where(x => x.IsActive == true).ToListAsync()
            };

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                var countt = context.ShoppingCarts.Where(u => u.ApplicationUserId == claim.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, countt);
            }



            return View(indexVM);
        }

        //GET DETAILS
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var returnDbMenuItem = await context.MenuItems.Include(x => x.Category).Include(x => x.SubCategory).Where(x => x.Id == id).FirstOrDefaultAsync();

            ShoppingCart shoppingCartObj = new ShoppingCart()
            {
                MenuItem = returnDbMenuItem,
                MenuItemId = returnDbMenuItem.Id
            };

            return View(shoppingCartObj);
        }

        //POST DETAILS
        [Authorize]
        [HttpPost, ActionName("Details")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DetailsPost(ShoppingCart shoppingCartobj)
        {
            shoppingCartobj.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                //User ID and Passed to Shopping obj
                shoppingCartobj.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = await context.ShoppingCarts
                    .Where(c => c.ApplicationUserId == shoppingCartobj.ApplicationUserId && c.MenuItemId == shoppingCartobj.MenuItemId)
                    .FirstOrDefaultAsync();

                if (cartFromDb == null)
                {
                    await context.ShoppingCarts.AddAsync(shoppingCartobj);
                }
                else
                {
                    cartFromDb.Count = cartFromDb.Count + shoppingCartobj.Count;
                }
                await context.SaveChangesAsync();

                var count = context.ShoppingCarts.Where(c => c.ApplicationUserId == shoppingCartobj.ApplicationUserId).ToList().Count();

                HttpContext.Session.SetInt32("ssCartCourt", count);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var returnMenuItemFromDb = await context.MenuItems.Include(x => x.Category).Include(x => x.SubCategory).FirstOrDefaultAsync(x => x.Id == shoppingCartobj.MenuItemId);

                ShoppingCart shoppingCart = new ShoppingCart()
                {
                    MenuItem = returnMenuItemFromDb,
                    MenuItemId = returnMenuItemFromDb.Id
                };
                return View(shoppingCart);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
