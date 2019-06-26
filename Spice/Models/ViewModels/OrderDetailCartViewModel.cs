using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spice.Models.ViewModels
{
    public class OrderDetailCartViewModel
    {
        public List<ShoppingCart> ListShoppingCarts { get; set; }
        public OrderHeader OrderHeader { get; set; }

    }
}
