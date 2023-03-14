using AttaBoyGameStore.Data;
using AttaBoyGameStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AttaBoyGameStore.Controllers
{
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET Shop
        public IActionResult Index()
        {
            var categories = _context.Categories
                .OrderBy(c => c.Name)
                .ToList();

            return View(categories);
        }

        // GET Shop/Category/5
        public IActionResult Category(int Id)
        {
            var category = _context.Categories
                .Include(c => c.Products)
                .FirstOrDefault(c => c.Id == Id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        public IActionResult Cart()
        {
            var customerId = GetCustomerId();

            var cartLines = _context.CartLines
                .Where(cl => cl.CustomerId == customerId)
                .Include(cl => cl.Product)
                .OrderByDescending(cl => cl.Id)
                .ToList();

            ViewData["TotalPrice"] = cartLines
                .Sum(cl => cl.Price)
                .ToString("C");

            /*
            decimal totalPrice = 0;

            for (int i = 0; i < cartLines.Count(); i++)
            {
                CartLine cartLine = cartLines[i];

                totalPrice += cartLine.Price;
            }

            ViewData["TotalPrice"] = totalPrice.ToString("C");
            */

            return View(cartLines);
        }

        // POST Ship/AddToCart
        [HttpPost]
        public IActionResult AddToCart([FromForm] int ProductId, [FromForm] int Quantity)
        {
            if (Quantity <= 0)
            {
                return BadRequest();
            }

            var product = _context.Products.Find(ProductId);
            if (product == null)
            {
                return BadRequest();
            }

            var price = product.Price * Quantity;

            var customerId = GetCustomerId();

            var cartLine = _context.CartLines
                .Where(cl => cl.ProductId == ProductId && cl.CustomerId == customerId)
                .FirstOrDefault();

            if (cartLine == null)
            {
                cartLine = new CartLine()
                {
                    ProductId = ProductId,
                    Quantity = Quantity,
                    Price = price,
                    CustomerId = customerId,
                };

                _context.CartLines.Add(cartLine);
            }
            else
            {
                cartLine.Quantity += Quantity;
                cartLine.Price += product.Price * Quantity;

                _context.CartLines.Update(cartLine);
            }

            _context.SaveChanges();

            return Redirect("Cart");
        }

        // POST Shop/UpdateCart
        [HttpPost]
        public IActionResult UpdateCart([FromForm] int CartLineId, [FromForm] int Quantity)
        {
            if (Quantity <= 0)
            {
                return BadRequest();
            }

            var cartLine = _context.CartLines.Find(CartLineId);
            if (cartLine == null)
            {
                return BadRequest();
            }

            var product = _context.Products.Find(cartLine.ProductId);

            var discount = Math.Max((product.Price * cartLine.Quantity) - cartLine.Price, 0);

            cartLine.Price = (product.Price * Quantity) - discount;
            cartLine.Quantity = Quantity;

            _context.CartLines.Update(cartLine);
            _context.SaveChanges();

            return Redirect("Cart");
        }

        // POST Shop/RemoveFromCart
        [HttpPost]
        public IActionResult RemoveFromCart([FromForm] int CartLineId)
        {
            var cartLine = _context.CartLines.Find(CartLineId);
            if (cartLine == null)
            {
                return BadRequest();
            }

            _context.CartLines.Remove(cartLine);
            _context.SaveChanges();

            return Redirect("Cart");
        }

        // GetCustomerId gets the customer id from the session
        // This might be a GUID, or the logged in user's email
        // If it isn't in the session, it stores it
        private String GetCustomerId()
        {
            var customerId = HttpContext.Session.GetString("CustomerId");

            if (!String.IsNullOrWhiteSpace(customerId))
            {
                return customerId;
            }

            // Didn't find customer id, need to generate

            var isLoggedIn = User?.Identity?.IsAuthenticated ?? false;

            if (isLoggedIn)
            {
                customerId = User.Identity.Name; // Email address
            }
            else
            {
                customerId = Guid.NewGuid().ToString(); // Generate new Id
            }

            HttpContext.Session.SetString("CustomerId", customerId);

            return customerId;
        }
    }
}
