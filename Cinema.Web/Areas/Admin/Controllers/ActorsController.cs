using Cinema.Application.Actors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cinema.Web.Areas.Admin.ViewModels;

namespace Cinema.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
// Admin oyuncu CRUD controller'i:
// - Oyuncu ekleme / listeleme / duzenleme / silme
// - Foto dosya upload islemi
// - Ad + Soyad -> FullName normalize etme
// - PostgreSQL timestamptz uyumu icin BirthDate UTC normalize etme
public class ActorsController : Controller
{
    private readonly IActorAdminService _actorService;

    public ActorsController(IActorAdminService actorService)
    {
        _actorService = actorService;
    }

    public async Task<IActionResult> Index()
    {
        var actors = await _actorService.GetActorsAsync();
        var vm = new ActorListVm
        {
            Items = actors.Select(actor => new ActorListItemVm
            {
                Id = actor.Id,
                FullName = actor.FullName,
                PhotoPath = actor.PhotoPath,
                Age = actor.Age,
                Nationality = actor.Nationality,
                BirthDate = actor.BirthDate
            }).ToList()
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult Create()
    {
        // CRUD / CREATE (FORM)
        return View(new ActorFormVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ActorFormVm model, IFormFile? photoFile)
    {
        if (!ModelState.IsValid)
            return View(model);

        ActorPhotoUploadDto? photoDto = null;
        if (photoFile != null && photoFile.Length > 0)
        {
            photoDto = new ActorPhotoUploadDto
            {
                FileName = photoFile.FileName,
                Content = photoFile.OpenReadStream(),
                Length = photoFile.Length
            };
        }

        var result = await _actorService.CreateActorAsync(new ActorCreateDto
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Nationality = model.Nationality,
            BirthDate = model.BirthDate,
            Biography = model.Biography
        }, photoDto);

        if (photoDto != null && photoDto.Content != Stream.Null)
            await photoDto.Content.DisposeAsync();

        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);

            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var actor = await _actorService.GetActorAsync(id);
        if (actor == null)
            return NotFound();

        var vm = new ActorFormVm
        {
            Id = actor.Id,
            FirstName = actor.FirstName,
            LastName = actor.LastName,
            Nationality = actor.Nationality,
            BirthDate = actor.BirthDate,
            PhotoPath = actor.PhotoPath,
            Biography = actor.Biography
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ActorFormVm model, IFormFile? photoFile)
    {
        if (!ModelState.IsValid)
            return View(model);

        ActorPhotoUploadDto? photoDto = null;
        if (photoFile != null && photoFile.Length > 0)
        {
            photoDto = new ActorPhotoUploadDto
            {
                FileName = photoFile.FileName,
                Content = photoFile.OpenReadStream(),
                Length = photoFile.Length
            };
        }

        var result = await _actorService.UpdateActorAsync(new ActorUpdateDto
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Nationality = model.Nationality,
            BirthDate = model.BirthDate,
            PhotoPath = model.PhotoPath,
            Biography = model.Biography
        }, photoDto);

        if (photoDto != null && photoDto.Content != Stream.Null)
            await photoDto.Content.DisposeAsync();

        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);

            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _actorService.DeleteActorAsync(id);
        if (!deleted)
            return NotFound();

        return RedirectToAction(nameof(Index));
    }
}
