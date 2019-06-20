using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]

    public class CouponController : Controller
    {
        private readonly ApplicationDbContext context;

        public CouponController(ApplicationDbContext context)
        {
            this.context = context;
        }
        //GET INDEX
        public async Task<IActionResult> Index()
        {
            var returnCouponResults = await context.Coupons.ToListAsync();

            return View(returnCouponResults);
        }

        //GET CREATE
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostCreate(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;

                if (files.Count() > 0)
                {
                    //Upload the image
                    byte[] p1 = null;
                    using (var fs1 = files[0].OpenReadStream())
                    {
                        using (var ms1 = new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }
                    coupon.Picture = p1;
                }
                context.Add(coupon);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }
        //GET EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var retunDbContext = await context.Coupons.SingleOrDefaultAsync(x => x.Id == id);
            if (retunDbContext == null)
            {
                return NotFound();
            }


            return View(retunDbContext);
        }

        //POST EDIT
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostEdit(int? id, Coupon coupon)
        {
            if (id == null)
            {
                return NotFound();
            }
            var couponFromDb = await context.Coupons.Where(x => x.Id == coupon.Id).FirstOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count()>0)
                {
                    //Upload the image
                    byte[] p1 = null;
                    using (var fs1 = files[0].OpenReadStream())
                    {
                        using (var ms1 = new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }
                    couponFromDb.Picture = p1;
                }
                couponFromDb.MinimumAmount = coupon.MinimumAmount;
                couponFromDb.Name = coupon.Name;
                couponFromDb.Discount = coupon.Discount;
                couponFromDb.CouponType = coupon.CouponType;
                couponFromDb.IsActive = coupon.IsActive;

                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        //GET DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var returnDetailsContext = await context.Coupons.FirstOrDefaultAsync(x => x.Id == id);
            if (returnDetailsContext == null)
            {
                return NotFound();
            }
            return View(returnDetailsContext);
        }

        //GET DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var returnDatabase = await context.Coupons.FirstOrDefaultAsync(x => x.Id == id);
            if (returnDatabase == null)
            {
                return NotFound();
            }
            return View(returnDatabase);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, Coupon coupon)
        {
            var coupons = await context.Coupons.SingleOrDefaultAsync(x => x.Id == id);
            context.Remove(coupons);
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}