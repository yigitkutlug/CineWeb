using Cinema.Web.Areas.Admin.ViewModels;
using Cinema.Application.AdminMovies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Cinema.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
// Admin film CRUD controller'i:
// - Create: yeni film + film-aktor iliskisi olusturur
// - Read: listeleme / edit formu icin veri okur
// - Update: film bilgisi + film-aktor iliskilerini senkronize eder
// - Delete: filmi siler
public class MoviesController : Controller
{
    private readonly IAdminMovieService _movieService;

    public MoviesController(IAdminMovieService movieService)
    {
        _movieService = movieService;
    }

    public async Task<IActionResult> Index()
    {
        var movies = await _movieService.GetMoviesAsync();
        return View(movies);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new MovieFormVm
        {
            ActorOptions = await GetActorOptionsAsync()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MovieFormVm vm, IFormFile? posterFile)
    {
        if (!ModelState.IsValid)
        {
            vm.ActorOptions = await GetActorOptionsAsync();
            return View(vm);
        }

        PosterUploadDto? posterDto = null;
        if (posterFile is not null && posterFile.Length > 0)
        {
            posterDto = new PosterUploadDto
            {
                FileName = posterFile.FileName,
                Content = posterFile.OpenReadStream(),
                Length = posterFile.Length
            };
        }

        var result = await _movieService.CreateMovieAsync(new MovieUpsertDto
        {
            Title = vm.Title,
            DurationMinutes = vm.DurationMinutes,
            Genre = vm.Genre,
            Description = vm.Description,
            IsFeatured = vm.IsFeatured,
            SelectedActorIds = vm.SelectedActorIds
        }, posterDto);

        if (posterDto != null && posterDto.Content != Stream.Null)
            await posterDto.Content.DisposeAsync();

        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);

            vm.ActorOptions = await GetActorOptionsAsync();
            return View(vm);
        }

        return RedirectToAction(nameof(Edit), new { id = result.MovieId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var movie = await _movieService.GetMovieForEditAsync(id);
        if (movie == null)
            return NotFound();

        var vm = new MovieFormVm
        {
            Id = movie.Id,
            Title = movie.Title,
            DurationMinutes = movie.DurationMinutes,
            Genre = movie.Genre ?? "",
            Description = movie.Description,
            PosterImagePath = movie.PosterImagePath,
            IsFeatured = movie.IsFeatured,
            SelectedActorIds = movie.SelectedActorIds,
            ActorOptions = await GetActorOptionsAsync()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MovieFormVm vm, IFormFile? posterFile)
    {
        if (!ModelState.IsValid)
        {
            vm.ActorOptions = await GetActorOptionsAsync();
            return View(vm);
        }

        PosterUploadDto? posterDto = null;
        if (posterFile is not null && posterFile.Length > 0)
        {
            posterDto = new PosterUploadDto
            {
                FileName = posterFile.FileName,
                Content = posterFile.OpenReadStream(),
                Length = posterFile.Length
            };
        }

        var result = await _movieService.UpdateMovieAsync(new MovieUpsertDto
        {
            Id = vm.Id,
            Title = vm.Title,
            DurationMinutes = vm.DurationMinutes,
            Genre = vm.Genre,
            Description = vm.Description,
            PosterImagePath = vm.PosterImagePath,
            IsFeatured = vm.IsFeatured,
            SelectedActorIds = vm.SelectedActorIds
        }, posterDto);

        if (posterDto != null && posterDto.Content != Stream.Null)
            await posterDto.Content.DisposeAsync();

        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);

            vm.ActorOptions = await GetActorOptionsAsync();
            return View(vm);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _movieService.DeleteMovieAsync(id);
        if (!deleted)
            return NotFound();

        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> GetActorOptionsAsync()
    {
        var actors = await _movieService.GetActorOptionsAsync();
        return actors.Select(a => new SelectListItem
        {
            Value = a.Id.ToString(),
            Text = a.Name
        }).ToList();
    }
}
