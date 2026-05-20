using Cinema.Web.Areas.Admin.ViewModels;
using Cinema.Domain.Entities;
using Cinema.Application.Showtimes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Cinema.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
// Film seanslarini admin tarafinda yoneten controller.
// Bu controller da klasik CRUD pattern'i ile calisir:
// - Index: seans listesi (READ)
// - Create GET/POST: yeni seans (CREATE)
// - Edit GET/POST: seans guncelleme (UPDATE)
// - Delete POST: seans silme (DELETE)
public class ShowtimesController : Controller
{
    private readonly IShowtimeAdminService _showtimeAdminService;

    public ShowtimesController(IShowtimeAdminService showtimeAdminService)
    {
        _showtimeAdminService = showtimeAdminService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? movieId = null)
    {
        var vm = await BuildIndexVmAsync(movieId);
        if (vm == null)
            return NotFound();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int movieId)
    {
        // Yeni seans formu acarken film basligi ve salon secenekleri yuklenir.
        var movie = await _showtimeAdminService.GetMovieAsync(movieId);
        if (movie == null)
            return NotFound();

        var vm = new ShowtimeFormVm
        {
            MovieId = movie.Id,
            MovieTitle = movie.Title,
            HallOptions = await GetHallOptionsAsync()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ShowtimeFormVm vm)
    {
        return await CreateInternalAsync(vm, useIndexViewOnError: false);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromIndex([Bind(Prefix = "QuickCreateForm")] ShowtimeFormVm vm)
    {
        return await CreateInternalAsync(vm, useIndexViewOnError: true);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        // Edit ekraninda UTC seans saati local saate cevrilir ki input kolay olsun.
        var showtime = await _showtimeAdminService.GetShowtimeWithMovieAsync(id);
        if (showtime == null)
            return NotFound();

        var vm = new ShowtimeFormVm
        {
            Id = showtime.Id,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie?.Title ?? "",
            HallId = showtime.HallId,
            StartsAt = DateTime.SpecifyKind(showtime.StartsAt, DateTimeKind.Utc).ToLocalTime(),
            Price = showtime.Price,
            IsActive = showtime.IsActive,
            HallOptions = await GetHallOptionsAsync()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ShowtimeFormVm vm)
    {
        var startsAtUtc = DateTime.SpecifyKind(vm.StartsAt, DateTimeKind.Local).ToUniversalTime();

        if (startsAtUtc <= DateTime.UtcNow)
            ModelState.AddModelError(nameof(vm.StartsAt), "Gecmis tarihli seans kaydedilemez.");

        if (await IsVipHallAsync(vm.HallId))
            vm.Price = 600m;

        var movie = await _showtimeAdminService.GetMovieAsync(vm.MovieId);
        if (movie == null)
            return NotFound();

        if (await _showtimeAdminService.HasHallConflictAsync(vm.HallId, startsAtUtc, movie.DurationMinutes, vm.Id))
            ModelState.AddModelError(nameof(vm.StartsAt), "Bu salonda bu saat araliginda baska bir seans var.");

        // Validation hatasinda ayni form tekrar render edilir.
        if (!ModelState.IsValid)
        {
            vm.MovieTitle = movie.Title;
            vm.HallOptions = await GetHallOptionsAsync();
            return View(vm);
        }

        // Guncellenecek kayit DB'den cekilir, sonra alanlar tek tek update edilir.
        var showtime = await _showtimeAdminService.GetShowtimeAsync(vm.Id);
        if (showtime == null)
            return NotFound();

        showtime.HallId = vm.HallId;
        showtime.StartsAt = startsAtUtc;
        showtime.Price = vm.Price;
        showtime.IsActive = vm.IsActive;

        await _showtimeAdminService.UpdateShowtimeAsync(showtime);
        return RedirectToAction(nameof(Index), new { movieId = showtime.MovieId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var showtime = await _showtimeAdminService.GetShowtimeAsync(id);
        if (showtime == null)
            return NotFound();

        var movieId = showtime.MovieId;
        await _showtimeAdminService.DeleteShowtimeAsync(id);

        return RedirectToAction(nameof(Index), new { movieId });
    }

    private async Task<List<SelectListItem>> GetHallOptionsAsync()
    {
        // Tum salonlar dropdown icin tek noktadan hazirlaniyor.
        var halls = await _showtimeAdminService.GetHallsAsync();
        return halls
            .Select(h => new SelectListItem
            {
                Value = h.Id.ToString(),
                Text = h.Name
            })
            .ToList();
    }

    private async Task<List<SelectListItem>> GetMovieOptionsAsync()
    {
        var movies = await _showtimeAdminService.GetMoviesAsync();
        return movies
            .Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Title
            })
            .ToList();
    }

    private async Task<ShowtimesIndexVm?> BuildIndexVmAsync(int? movieId, ShowtimeFormVm? formOverride = null)
    {
        Movie? movie = null;
        if (movieId.HasValue)
        {
            movie = await _showtimeAdminService.GetMovieAsync(movieId.Value);
            if (movie == null)
                return null;
        }

        var items = await _showtimeAdminService.GetShowtimesAsync(movieId);

        var form = formOverride ?? new ShowtimeFormVm
        {
            MovieId = movie?.Id ?? 0,
            MovieTitle = movie?.Title ?? "",
            ReturnToAllShowtimes = !movieId.HasValue
        };

        form.MovieOptions = await GetMovieOptionsAsync();
        form.HallOptions = await GetHallOptionsAsync();

        return new ShowtimesIndexVm
        {
            Items = items,
            Movie = movie,
            IsAllShowtimes = !movieId.HasValue,
            QuickCreateForm = form
        };
    }

    private async Task<IActionResult> CreateInternalAsync(ShowtimeFormVm vm, bool useIndexViewOnError)
    {
        var startsAtUtc = DateTime.SpecifyKind(vm.StartsAt, DateTimeKind.Local).ToUniversalTime();

        if (startsAtUtc <= DateTime.UtcNow)
            ModelState.AddModelError(nameof(vm.StartsAt), "Gecmis tarihli seans eklenemez.");

        if (await IsVipHallAsync(vm.HallId))
            vm.Price = 600m;

        var movie = await _showtimeAdminService.GetMovieAsync(vm.MovieId);
        if (movie == null)
            return NotFound();

        if (await _showtimeAdminService.HasHallConflictAsync(vm.HallId, startsAtUtc, movie.DurationMinutes))
            ModelState.AddModelError(nameof(vm.StartsAt), "Bu salonda bu saat araliginda baska bir seans var.");

        if (!ModelState.IsValid)
        {
            vm.MovieTitle = movie.Title;
            vm.MovieOptions = await GetMovieOptionsAsync();
            vm.HallOptions = await GetHallOptionsAsync();

            if (!useIndexViewOnError)
                return View("Create", vm);

            var indexVm = await BuildIndexVmAsync(vm.ReturnToAllShowtimes ? null : vm.MovieId, vm);
            if (indexVm == null)
                return NotFound();

            return View("Index", indexVm);
        }

        var showtime = new Showtime
        {
            MovieId = vm.MovieId,
            HallId = vm.HallId,
            StartsAt = startsAtUtc,
            Price = vm.Price,
            IsActive = vm.IsActive
        };

        await _showtimeAdminService.CreateShowtimeAsync(showtime);

        return vm.ReturnToAllShowtimes
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Index), new { movieId = vm.MovieId });
    }

    private async Task<bool> IsVipHallAsync(int hallId)
    {
        var halls = await _showtimeAdminService.GetHallsAsync();
        var hall = halls.FirstOrDefault(h => h.Id == hallId);
        return hall != null && hall.Name.Contains("VIP", StringComparison.OrdinalIgnoreCase);
    }
}
