using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MainItemController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IHostingEnvironment hostingEnvironment;

        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }

        public MainItemController(ApplicationDbContext context, IHostingEnvironment hostingEnvironment)
        {
            this.context = context;
            this.hostingEnvironment = hostingEnvironment;
            MenuItemVM = new MenuItemViewModel()
            {
                MenuItem = new Models.MenuItem(),
                Categories = context.Categories
            };
        }

        //GET Index
        public async Task<IActionResult> Index()
        {
            var returnMenuItemResut = await context.MenuItems.Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();

            return View(returnMenuItemResut);
        }

        //GET CREATE
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }

        //POST CREATE
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostCreate()
        {
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());//From the name of the Div

            if (!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }

            context.MenuItems.Add(MenuItemVM.MenuItem);
            await context.SaveChangesAsync();


            //Working on the image saving section
            //Root Extracted
            string webRootPath = hostingEnvironment.WebRootPath;
            //Files name extracted
            var files = HttpContext.Request.Form.Files;

            //Find Id
            var menuItemFromDb = await context.MenuItems.FindAsync(MenuItemVM.MenuItem.Id);

            if (files.Count() > 0)
            {
                //Files has been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);

                using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }

                //This will include in the database
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension;

            }
            else
            {
                //No files was uploaded, So use Default
                var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
                //Source and Destination
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemVM.MenuItem.Id + ".png");
                //This will be displayed in the SQL
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + ".png";
            }
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //GET EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem = await context.MenuItems.Include(x => x.Category).Include(x => x.SubCategory).FirstOrDefaultAsync(x => x.Id == id);
            MenuItemVM.SubCategories = await context.SubCategories.Where(x => x.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);

        }
        ////POST EDIT
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }

            //Working on the image saving section
            //Root Extracted
            string webRootPath = hostingEnvironment.WebRootPath;
            //Files name extracted
            var files = HttpContext.Request.Form.Files;
            //Find Id
            var menuItemFromDb = await context.MenuItems.FindAsync(MenuItemVM.MenuItem.Id);

            if (files.Count() > 0)
            {
                //Files has been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension_new = Path.GetExtension(files[0].FileName);

                //Delete the original files
                var imagePath = Path.Combine(webRootPath, menuItemFromDb.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                //we will upload the new file

                using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension_new), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }

                //This will include in the database
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension_new;
            }

            //else

            menuItemFromDb.Name = MenuItemVM.MenuItem.Name;
            menuItemFromDb.Description = MenuItemVM.MenuItem.Description;
            menuItemFromDb.Price = MenuItemVM.MenuItem.Price;
            menuItemFromDb.Spicyness = MenuItemVM.MenuItem.Spicyness;
            menuItemFromDb.CategoryId = MenuItemVM.MenuItem.CategoryId;
            menuItemFromDb.SubCategoryId = MenuItemVM.MenuItem.SubCategoryId;

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //GET DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await context.MenuItems.Include(x => x.Category).Include(x => x.SubCategory).FirstOrDefaultAsync(x => x.Id == id);
            MenuItemVM.SubCategories = await context.SubCategories.Where(x => x.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

        //GET DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem = await context.MenuItems.Include(x => x.Category).Include(x => x.SubCategory).FirstOrDefaultAsync(x => x.Id == id);
            MenuItemVM.SubCategories = await context.SubCategories.Where(x => x.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

        //POST DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostDelete(int id)
        {
            string webRootPath = hostingEnvironment.WebRootPath;
            MenuItem menuItem = await context.MenuItems.FindAsync(id);
            if (menuItem != null)
            {

                var files = HttpContext.Request.Form.Files;

                if (files.Count > 0)
                {
                    var imagePath = Path.Combine(webRootPath, menuItem.Image.TrimStart('\\'));

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                context.MenuItems.Remove(menuItem);
                await context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}