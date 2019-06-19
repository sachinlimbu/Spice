using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spice.Models.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<MenuItem> MenuItems { get; set; }
        public IEnumerable<Coupon> Coupons { get; set; }
        
    }
}
