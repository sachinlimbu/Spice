using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Spice.Data;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MainItemController : Controller
    {
        private readonly ApplicationDbContext context;

        public MainItemController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {


            return View();
        }
    }
}