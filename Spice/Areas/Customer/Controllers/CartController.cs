using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;
using Stripe;

namespace Spice.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext context;

        [BindProperty]
        public OrderDetailCartViewModel orderDetailCartVM { get; set; }

        public CartController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task<IActionResult> Index()
        {
            orderDetailCartVM = new OrderDetailCartViewModel()
            {
                OrderHeader = new Models.OrderHeader()

            };
            orderDetailCartVM.OrderHeader.OrderTotal = 0;

            //Find the user
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //Get the context shopping cart using the user value

            var cart = context.ShoppingCarts.Where(c => c.ApplicationUserId == claim.Value);

            //If cart has value put viewModel === ListShoppingCarts
            if (cart != null)
            {
                orderDetailCartVM.ListShoppingCarts = cart.ToList();
            }

            foreach (var item in orderDetailCartVM.ListShoppingCarts)
            {
                item.MenuItem = await context.MenuItems.Include(m => m.SubCategory).FirstOrDefaultAsync(m => m.Id == item.MenuItemId);

                orderDetailCartVM.OrderHeader.OrderTotal = orderDetailCartVM.OrderHeader.OrderTotal + (item.MenuItem.Price * item.Count);

                item.MenuItem.Description = SD.ConvertToRawHtml(item.MenuItem.Description);
                if (item.MenuItem.Description.Length > 100)
                {
                    item.MenuItem.Description = item.MenuItem.Description.Substring(0, 99) + "...";
                }

            }
            orderDetailCartVM.OrderHeader.OrderTotalOriginal = orderDetailCartVM.OrderHeader.OrderTotal;

            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                orderDetailCartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await context.Coupons.Where(c => c.Name.ToLower() == orderDetailCartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();

                orderDetailCartVM.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, orderDetailCartVM.OrderHeader.OrderTotalOriginal);
            }

            return View(orderDetailCartVM);
        }

        public IActionResult AddCoupon()
        {
            if (orderDetailCartVM.OrderHeader.CouponCode == null)
            {
                orderDetailCartVM.OrderHeader.CouponCode = "";
            }
            HttpContext.Session.SetString(SD.ssCouponCode, orderDetailCartVM.OrderHeader.CouponCode);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveCoupon()
        {
            HttpContext.Session.SetString(SD.ssCouponCode, string.Empty);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await context.ShoppingCarts.FirstOrDefaultAsync(c => c.Id == cartId);

            cart.Count += 1;
            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await context.ShoppingCarts.FirstOrDefaultAsync(c => c.Id == cartId);
            if (cart.Count == 1)
            {
                context.ShoppingCarts.Remove(cart);
                await context.SaveChangesAsync();

                var countapp = context.ShoppingCarts.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;

                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, countapp);
            }
            else
            {
                cart.Count -= 1;
                await context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await context.ShoppingCarts.FirstOrDefaultAsync(c => c.Id == cartId);

            context.ShoppingCarts.Remove(cart);
            await context.SaveChangesAsync();

            var countapp = context.ShoppingCarts.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count;
            HttpContext.Session.SetInt32(SD.ssShoppingCartCount, countapp);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Summary()
        {
            orderDetailCartVM = new OrderDetailCartViewModel()
            {
                OrderHeader = new Models.OrderHeader()

            };
            orderDetailCartVM.OrderHeader.OrderTotal = 0;

            //Find the user
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ApplicationUser applicationUser = await context.ApplicationUsers.Where(c => c.Id == claim.Value).FirstOrDefaultAsync();

            var cart = context.ShoppingCarts.Where(c => c.ApplicationUserId == claim.Value);
            //Get the context shopping cart using the user value
            //If cart has value put viewModel === ListShoppingCarts
            if (cart != null)
            {
                orderDetailCartVM.ListShoppingCarts = cart.ToList();
            }

            foreach (var item in orderDetailCartVM.ListShoppingCarts)
            {
                item.MenuItem = await context.MenuItems.FirstOrDefaultAsync(m => m.Id == item.MenuItemId);
                orderDetailCartVM.OrderHeader.OrderTotal = orderDetailCartVM.OrderHeader.OrderTotal + (item.MenuItem.Price * item.Count);
            }
            orderDetailCartVM.OrderHeader.OrderTotalOriginal = orderDetailCartVM.OrderHeader.OrderTotal;
            orderDetailCartVM.OrderHeader.PickupName = applicationUser.Name;
            orderDetailCartVM.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
            orderDetailCartVM.OrderHeader.PickUpTime = DateTime.Now;

            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                orderDetailCartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromdb = await context.Coupons.Where(c => c.Name.ToLower() == orderDetailCartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                orderDetailCartVM.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromdb, orderDetailCartVM.OrderHeader.OrderTotalOriginal);

            }
            return View(orderDetailCartVM);

        }

        [HttpPost, ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPost(string stripeEmail, string stripeToken)
        {
            //get user identity 
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //Get List of shopping cart using the claim user identity
            orderDetailCartVM.ListShoppingCarts = await context.ShoppingCarts.Where(c => c.ApplicationUserId == claim.Value).ToListAsync();


            //Set order headers
            orderDetailCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            orderDetailCartVM.OrderHeader.OrderDate = DateTime.Now;
            orderDetailCartVM.OrderHeader.UserId = claim.Value;
            orderDetailCartVM.OrderHeader.Status = SD.PaymentStatusPending;
            orderDetailCartVM.OrderHeader.PickUpTime = Convert.ToDateTime(orderDetailCartVM.OrderHeader.PickUpDate.ToShortDateString() + " " + orderDetailCartVM.OrderHeader.PickUpTime.ToShortTimeString());

            //Save Changes
            context.OrderHeaders.Add(orderDetailCartVM.OrderHeader);
            await context.SaveChangesAsync();

            //Set new list order details obj
            List<OrderDetails> orderDetailsList = new List<OrderDetails>();//More research
            orderDetailCartVM.OrderHeader.OrderTotalOriginal = 0;

            foreach (var item in orderDetailCartVM.ListShoppingCarts)
            {
                //Get the Menu Items
                item.MenuItem = await context.MenuItems.FirstOrDefaultAsync(m => m.Id == item.MenuItemId);

                //set the orderdetails
                OrderDetails orderDetails = new OrderDetails
                {
                    MenuItemId = item.MenuItemId,
                    OrderId = orderDetailCartVM.OrderHeader.Id,
                    Description = item.MenuItem.Description,
                    Name = item.MenuItem.Name,
                    Price = item.MenuItem.Price,
                    Count = item.Count
                };

                orderDetailCartVM.OrderHeader.OrderTotalOriginal += orderDetails.Count * orderDetails.Price;
                context.OrderDetails.Add(orderDetails);
            }
            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                orderDetailCartVM.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await context.Coupons.Where(c => c.Name.ToLower() == orderDetailCartVM.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                orderDetailCartVM.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, orderDetailCartVM.OrderHeader.OrderTotalOriginal);
            }
            else
            {
                orderDetailCartVM.OrderHeader.OrderTotal = orderDetailCartVM.OrderHeader.OrderTotalOriginal;
            }
            orderDetailCartVM.OrderHeader.CouponCodeDiscount = orderDetailCartVM.OrderHeader.OrderTotalOriginal - orderDetailCartVM.OrderHeader.OrderTotal;

            context.ShoppingCarts.RemoveRange(orderDetailCartVM.ListShoppingCarts);
            HttpContext.Session.SetInt32(SD.ssShoppingCartCount, 0);
            await context.SaveChangesAsync();


            //stripe logic
            if (stripeToken != null)
            {

                var customers = new CustomerService();
                var charges = new ChargeService();

                var customer = customers.Create(new CustomerCreateOptions
                {
                    Email = stripeEmail,
                    Source = stripeToken
                });

                var charge = charges.Create(new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(orderDetailCartVM.OrderHeader.OrderTotal * 100),
                    Description = "Order Id" + orderDetailCartVM.OrderHeader.Id,
                    Currency = "usd",
                    CustomerId = customer.Id
                });

                orderDetailCartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
                if (charge.Status.ToLower()=="succeeded")
                {
                    //email for successful order
                    orderDetailCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    orderDetailCartVM.OrderHeader.Status = SD.StatusSubmitted;
                }
                else
                {
                    orderDetailCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }

                await context.SaveChangesAsync();

            }
            else
            {
                orderDetailCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }
            return RedirectToAction("Confirm", "Order", new { id = orderDetailCartVM.OrderHeader.Id });

            //return RedirectToAction("Index", "Home");
        }

        //public async Task<IActionResult> OrderHistory() {
        //    var claimsIdentity = (ClaimsIdentity)User.Identity;
        //    var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
        //    List<OrderDetailsViewModel> orderList = new List<OrderDetailsViewModel>();
        //    List<OrderHeader> orderHeadersList = 
        //}

    }

}
