using AttaBoyGameStore.Models;
using Microsoft.AspNetCore.Mvc;

namespace AttaBoyGameStore.Controllers
{
    public class BrandsController : Controller
    {
        public IActionResult Index()
        {
            var brands = new List<Brand>();

            brands.Add(new Brand() { Name = "EA" });
            brands.Add(new Brand() { Name = "Ubisoft" });
            brands.Add(new Brand() { Name = "Unity" });
            brands.Add(new Brand() { Name = "Blizzard" });
            brands.Add(new Brand() { Name = "Rockstar" });

            return View(brands);
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult ShopByBrand(String brand)
        {
            ViewData["Brand"] = brand;

            return View();
        }
    }
}
