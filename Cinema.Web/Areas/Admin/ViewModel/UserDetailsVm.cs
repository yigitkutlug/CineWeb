namespace Cinema.Web.Areas.Admin.ViewModels;

public class UserDetailsVm
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? ProfilePhotoPath { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsSuperAdmin { get; set; }
    public List<UserTicketItemVm> Tickets { get; set; } = new();
    public List<UserReviewItemVm> Reviews { get; set; } = new();
}

public class UserTicketItemVm
{
    public int TicketId { get; set; }
    public string MovieTitle { get; set; } = "";
    public DateTime? StartsAtUtc { get; set; }
    public string? HallName { get; set; }
    public string? SeatLabel { get; set; }
    public decimal Price { get; set; }
    public DateTime PurchasedAtUtc { get; set; }
}

public class UserReviewItemVm
{
    public int ReviewId { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = "";
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
