using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Cinema.Domain.Entities;
using Cinema.Web.ViewModels;
using Cinema.Application.Movies;
using Cinema.Application.Showtimes;

namespace Cinema.Web.Controllers;

// Public (yetkisiz) sayfalarin controller'i.
public class HomeController : Controller
{
    private readonly IMovieQueryService _movieQueryService;
    private readonly IShowtimeQueryService _showtimeQueryService;

    public HomeController(IMovieQueryService movieQueryService, IShowtimeQueryService showtimeQueryService)
    {
        _movieQueryService = movieQueryService;
        _showtimeQueryService = showtimeQueryService;
    }

    // Ana sayfa
    public async Task<IActionResult> Index()
    {
        // Ana sayfada sadece vitrin/one cikan filmler gosterilir (max 3).
        var featuredMovies = await _movieQueryService.GetFeaturedMoviesAsync(3);

        var nowUtc = DateTime.UtcNow;
        var showtimes = await _showtimeQueryService.GetShowtimeListForApiAsync(nowUtc);
        var upcoming = showtimes
            .Where(s => s.IsActive && s.StartsAtUtc > nowUtc)
            .OrderBy(s => s.StartsAtUtc)
            .Take(3)
            .ToList();

        var vm = new HomePageVm
        {
            FeaturedMovies = featuredMovies,
            UpcomingShowtimes = upcoming
        };

        return View(vm);
    }

    // Gizlilik sayfasi
    public IActionResult Privacy()
    {
        return View();
    }

    // Hakkimizda sayfasi
    public IActionResult About()
    {
        return View();
    }

    // Iletisim sayfasi (GET)
    [HttpGet]
    public IActionResult Contact()
    {
        return View(new ContactFormVm());
    }

    // Iletisim formu (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Contact(ContactFormVm model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // TODO: Burada e-posta gonderimi / kayit islemi yapilabilir.
        return View(new ContactFormVm { IsSubmitted = true });
    }



    // Framework hata sayfasi; cache kapatilir ki stale hata donmesin.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
