using Cinema.Application.Tickets;
using Cinema.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

namespace Cinema.Infrastructure.Tickets;

public class EmailTicketNotificationService : ITicketNotificationService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailTicketNotificationService> _logger;

    public EmailTicketNotificationService(
        UserManager<IdentityUser> userManager,
        IEmailSender emailSender,
        ILogger<EmailTicketNotificationService> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task SendTicketPurchaseConfirmationAsync(string userId, Showtime showtime, List<string> seatNumbers, decimal totalPrice)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || string.IsNullOrWhiteSpace(user.Email) || showtime.Movie == null || showtime.Hall == null)
            return;

        var sessionLocal = showtime.StartsAt.ToLocalTime();
        var seatsText = string.Join(", ", seatNumbers);
        var subject = $"Biletiniz hazir: {showtime.Movie.Title}";
        var body = $"""
            <h2>Bilet Satin Alma Onayi</h2>
            <p><strong>Film:</strong> {showtime.Movie.Title}</p>
            <p><strong>Salon:</strong> {showtime.Hall.Name}</p>
            <p><strong>Koltuklar:</strong> {seatsText}</p>
            <p><strong>Seans:</strong> {sessionLocal:dd.MM.yyyy HH:mm}</p>
            <p><strong>Tutar:</strong> {totalPrice:0.00} TL</p>
            <p>Iyi seyirler dileriz.</p>
            """;

        try
        {
            await _emailSender.SendEmailAsync(user.Email, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Bilet kaydi olustu ancak onay maili gonderilemedi. UserId: {UserId}", userId);
        }
    }
}
