using Cinema.Web.ViewModels;
using Cinema.Application.Tickets;
using Cinema.Application.Profiles;
using Cinema.Application.Badges;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cinema.Web.Controllers;

[Authorize]
// Giris yapan kullanicinin Profilim ve Biletlerim ekranlarini yonetir.
public class ProfileController : Controller
{
    private readonly ITicketService _ticketService;
    private readonly IProfileService _profileService;
    private readonly IBadgeService _badgeService;

    public ProfileController(
        ITicketService ticketService,
        IProfileService profileService,
        IBadgeService badgeService)
    {
        _ticketService = ticketService;
        _profileService = profileService;
        _badgeService = badgeService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var profile = await _profileService.GetProfileAsync(userId);
        if (profile == null)
            return Challenge();

        var vm = new ProfileEditVm
        {
            UserId = profile.UserId,
            Email = profile.Email,
            PhoneNumber = profile.PhoneNumber,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            City = profile.City,
            Address = profile.Address,
            ProfilePhotoPath = profile.ProfilePhotoPath
        };

        var badges = await _badgeService.GetUserBadgesAsync(userId);
        vm.EarnedBadges = badges.Earned.Select(b => new BadgeItemVm
        {
            Title = b.Title,
            Description = b.Description,
            RequirementText = b.RequirementText,
            EarnedAtUtc = b.EarnedAtUtc
        }).ToList();
        vm.LockedBadges = badges.Locked.Select(b => new BadgeItemVm
        {
            Title = b.Title,
            Description = b.Description,
            RequirementText = b.RequirementText,
            EarnedAtUtc = b.EarnedAtUtc
        }).ToList();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileEditVm vm, IFormFile? profilePhoto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        if (!ModelState.IsValid)
            return View(vm);

        ProfilePhotoUploadDto? photoDto = null;
        if (profilePhoto is not null && profilePhoto.Length > 0)
        {
            photoDto = new ProfilePhotoUploadDto
            {
                FileName = profilePhoto.FileName,
                Content = profilePhoto.OpenReadStream(),
                Length = profilePhoto.Length
            };
        }

        var result = await _profileService.UpdateProfileAsync(new ProfileUpdateDto
        {
            UserId = userId,
            Email = vm.Email,
            PhoneNumber = vm.PhoneNumber,
            FirstName = vm.FirstName,
            LastName = vm.LastName,
            City = vm.City,
            Address = vm.Address,
            ProfilePhotoPath = vm.ProfilePhotoPath
        }, photoDto);

        if (photoDto != null && photoDto.Content != Stream.Null)
            await photoDto.Content.DisposeAsync();

        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);

            return View(vm);
        }

        TempData["ProfileSaved"] = "Profil bilgileri guncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    // Kullaniciya ait bilet gecmisini listeler.
    public async Task<IActionResult> Tickets()
    {
        // Ticket + Movie join ile kullanicinin satin alma gecmisi okunur.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var tickets = await _ticketService.GetUserTicketsAsync(userId);
        var vm = tickets.Select(t => new TicketHistoryItemVm
        {
            TicketId = t.TicketId,
            MovieTitle = t.MovieTitle,
            SeatNo = t.SeatNo,
            PurchasedAt = t.PurchasedAtUtc,
            SessionAt = t.SessionAtUtc,
            Price = t.Price
        }).ToList();

        return View(vm);
    }

}
