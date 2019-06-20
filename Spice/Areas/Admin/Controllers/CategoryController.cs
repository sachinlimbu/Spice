using System;
using System.Collections.Generic;
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
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext context;

        public CategoryController(ApplicationDbContext context)
        {
            this.context = context;
        }
        [BindProperty]
        public Category Category { get; set; }
        //GET INDEX
        public async Task<IActionResult> Index()
        {
            var RetrieveCategory = await context.Categories.ToListAsync();
            return View(RetrieveCategory);
        }

        //GET CREATE
        public IActionResult Create()
        {
            return View();
        }

        //POST CREATE
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostCreate()
        {
            if (ModelState.IsValid)
            {
                context.Categories.Add(Category);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        //GET EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var getData = await context.Categories.FindAsync(id);
            if (getData == null)
            {
                return NotFound();
            }
            return View(getData);
        }
        //POST EDIT
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostEdit()
        {
            if (ModelState.IsValid)
            {
                context.Categories.Update(Category);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(Category);
        }

        //GET DETAILS
        public async Task<IActionResult> Details(int?id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var returnResult = await context.Categories.FindAsync(id);
            if (returnResult == null)
            {
                return NotFound();
            }
            return View(returnResult);
        }

        //GET DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var returnResult = await context.Categories.FindAsync(id);
            if (returnResult == null)
            {
                return NotFound();
            }
            return View(returnResult);
        }

        //POST DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostDelete()
        {
            if (ModelState.IsValid)
            {
                context.Categories.Remove(Category);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(Category);
        }

    }
}