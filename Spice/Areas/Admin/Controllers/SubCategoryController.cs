using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]

    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext context;
        [TempData]
        public string StatusMessage { get; set; }


        public SubCategoryController(ApplicationDbContext context)
        {
            this.context = context;
        }

        //GET INDEX LIST
        public async Task<IActionResult> Index()
        {
            var getResultList = await context.SubCategories.Include(x => x.Category).ToListAsync();
            return View(getResultList);
        }

        //GET CREATE
        public async Task<IActionResult> Create()
        {
            SubCategoryAndCategoryViewModel categoryAndCategoryVM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await context.Categories.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                SubCategoryList = await context.SubCategories.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };
            return View(categoryAndCategoryVM);
        }


        //POST CREATE
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostCreate(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {

                var returnResultVm = context.SubCategories.Include(x => x.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);
                if (returnResultVm.Count() > 0)
                {
                    //Error Message
                    StatusMessage = "Error : Sub Category exists under " + returnResultVm.First().Name + " category. Please use another name";
                }
                else
                {
                    context.SubCategories.Add(model.SubCategory);
                    await context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryViewModel modelVm = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await context.Categories.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await context.SubCategories.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync(),
                Message = StatusMessage
            };
            return View(modelVm);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var getData = await context.SubCategories.SingleOrDefaultAsync(m => m.Id == id);

            if (getData == null)
            {
                return NotFound();
            }

            SubCategoryAndCategoryViewModel categoryAndCategoryVM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await context.Categories.ToListAsync(),
                SubCategory = getData,
                SubCategoryList = await context.SubCategories.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };
            return View(categoryAndCategoryVM);
        }

        //POST Edit
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostEdit(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = context.SubCategories.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);

                if (doesSubCategoryExists.Count() > 0)
                {
                    //Error
                    StatusMessage = "Error : Sub Category exists under " + doesSubCategoryExists.First().Category.Name + " category. Please use another name.";
                }
                else
                {
                    var subCatFromDb = await context.SubCategories.FindAsync(model.SubCategory.Id);
                    subCatFromDb.Name = model.SubCategory.Name;

                    await context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await context.Categories.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await context.SubCategories.OrderBy(p => p.Name).Select(p => p.Name).ToListAsync(),
                Message = StatusMessage
            };
            //modelVM.SubCategory.Id = id;
            return View(modelVM);
        }


        //GET Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var returnSubCategory = await context.SubCategories.Include(x => x.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (returnSubCategory == null)
            {
                return NotFound();
            }
            return View(returnSubCategory);
        }

        //GET Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var returnSubCategory = await context.SubCategories.Include(x => x.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (returnSubCategory == null)
            {
                return NotFound();
            }


            return View(returnSubCategory);

        }

        //POST Delete

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostDelete(int? id)
        {
            //var returnSubCategory = context.SubCategories.FirstOrDefaultAsync(m => m.Id == id);
            if (id == null)
            {
                return NotFound();
            }
            var returnSubCategory = await context.SubCategories.FirstOrDefaultAsync(m => m.Id == id);
            if (returnSubCategory == null)
            {
                return NotFound();
            }
            else
            {
                context.Remove(returnSubCategory);
                await context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
        }

        //GET GetSubCategory

        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subCategories = new List<SubCategory>();

            subCategories = await (from item in context.SubCategories
                                   where item.CategoryId == id
                                   select item).ToListAsync();
            return Json(new SelectList(subCategories, "Id", "Name"));
        }
    }
}