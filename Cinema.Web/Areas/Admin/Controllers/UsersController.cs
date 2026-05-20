using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Cinema.Web.Areas.Admin.ViewModels;
using Cinema.Domain.Entities;
using Cinema.Application.Tickets;
using Cinema.Application.Users;
using Cinema.Application.Reviews;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Web.Areas.Admin.Controllers;

[Area("Admin")]
// Kullanici/rol yonetimi. Admin goruntuler, sadece SuperAdmin duzenler.
[Authorize(Roles = "Admin,SuperAdmin")]
public class UsersController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITicketService _ticketService;
    private readonly IUserAdminService _userAdminService;
    private readonly IReviewService _reviewService;

    // Identity kullanici islemleri icin servis.
    public UsersController(
        UserManager<IdentityUser> userManager,
        ITicketService ticketService,
        IUserAdminService userAdminService,
        IReviewService reviewService)
    {
        _userManager = userManager;
        _ticketService = ticketService;
        _userAdminService = userAdminService;
        _reviewService = reviewService;
    }

    // Tum kullanicilari listeler.
    public async Task<IActionResult> Index()
    {
        var users = await _userAdminService.GetUsersAsync();
        var items = users.Select(u => new UserListItemVm
        {
            Id = u.Id,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            FullName = u.FullName,
            City = u.City,
            Roles = u.Roles,
            IsAdmin = u.IsAdmin,
            IsSuperAdmin = u.IsSuperAdmin
        }).ToList();

        return View(new UsersIndexVm { Items = items });
    }

    [HttpGet]
    // Kullanici detaylari (salt okunur).
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        var user = await _userAdminService.GetUserDetailsAsync(id);
        if (user == null)
            return NotFound();

        var tickets = await _ticketService.GetUserTicketsForAdminAsync(user.Id);
        var ticketItems = tickets.Select(ticket => new UserTicketItemVm
        {
            TicketId = ticket.TicketId,
            MovieTitle = ticket.MovieTitle,
            StartsAtUtc = ticket.StartsAtUtc,
            HallName = ticket.HallName,
            SeatLabel = ticket.SeatLabel,
            Price = ticket.Price,
            PurchasedAtUtc = ticket.PurchasedAtUtc
        }).ToList();

        var reviews = await _reviewService.GetForUserAsync(user.Id);
        var reviewItems = reviews.Select(r => new UserReviewItemVm
        {
            ReviewId = r.Id,
            MovieId = r.MovieId,
            MovieTitle = r.MovieTitle,
            Rating = r.Rating,
            Comment = r.Comment,
            IsApproved = r.IsApproved,
            CreatedAtUtc = r.CreatedAtUtc
        }).ToList();

        var vm = new UserDetailsVm
        {
            Id = user.Id,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FirstName = user.FirstName,
            LastName = user.LastName,
            City = user.City,
            Address = user.Address,
            ProfilePhotoPath = user.ProfilePhotoPath,
            IsAdmin = user.IsAdmin,
            IsSuperAdmin = user.IsSuperAdmin,
            Tickets = ticketItems,
            Reviews = reviewItems
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CancelTicket(int id, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return NotFound();

        var result = await _ticketService.CancelTicketAsync(id, userId, DateTime.UtcNow);
        if (!result.Success)
        {
            if (result.ErrorMessage == "Bilet bulunamadi.")
                return NotFound();

            return Forbid();
        }

        return RedirectToAction(nameof(Details), new { id = userId });
    }

    [HttpGet]
    // Edit formunu mevcut kullanici verisi ve rol checkbox durumlariyla doldurur.
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        var user = await _userAdminService.GetUserDetailsAsync(id);
        if (user == null)
            return NotFound();

        var vm = new UserEditVm
        {
            Id = user.Id,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FirstName = user.FirstName,
            LastName = user.LastName,
            City = user.City,
            Address = user.Address,
            ProfilePhotoPath = user.ProfilePhotoPath,
            IsAdmin = user.IsAdmin,
            IsSuperAdmin = user.IsSuperAdmin
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Formdan gelen email + rol secimlerini kaydeder.
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Edit(UserEditVm vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var result = await _userAdminService.UpdateUserAsync(new UserUpdateDto
        {
            Id = vm.Id,
            Email = vm.Email,
            PhoneNumber = vm.PhoneNumber,
            FirstName = vm.FirstName,
            LastName = vm.LastName,
            City = vm.City,
            Address = vm.Address,
            ProfilePhotoPath = vm.ProfilePhotoPath,
            IsAdmin = vm.IsAdmin,
            IsSuperAdmin = vm.IsSuperAdmin
        });

        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);

            return View(vm);
        }

        return RedirectToAction(nameof(Index));
    }
}
