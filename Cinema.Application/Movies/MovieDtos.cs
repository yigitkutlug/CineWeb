using Cinema.Domain.Entities;

namespace Cinema.Application.Movies;

public class MovieDetailsDto
{
    public Movie Movie { get; set; } = default!;
    public List<Showtime> Showtimes { get; set; } = new();
}

public class SeatSelectionDto
{
    public Showtime Showtime { get; set; } = default!;
    public Movie Movie { get; set; } = default!;
    public Hall Hall { get; set; } = default!;
    public List<Seat> Seats { get; set; } = new();
    public HashSet<int> ReservedSeatIds { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public List<int> SelectedSeatIds { get; set; } = new();
}

public class SeatSelectionResultDto
{
    public int MovieId { get; set; }
    public SeatSelectionDto? Data { get; set; }
}

public class TicketPurchaseSuccessDto
{
    public string MovieTitle { get; set; } = "";
    public string HallName { get; set; } = "";
    public DateTime SessionAt { get; set; }
    public List<string> SeatNumbers { get; set; } = new();
    public decimal TotalPrice { get; set; }
    public int TicketCount => SeatNumbers.Count;
}

public class PurchaseResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public TicketPurchaseSuccessDto? SuccessPayload { get; set; }
}

public class MovieListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Genre { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsFeatured { get; set; }
    public string? PosterImagePath { get; set; }
}

public class MovieDetailsApiDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Genre { get; set; }
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
    public bool IsFeatured { get; set; }
    public string? PosterImagePath { get; set; }
    public List<ShowtimeApiDto> Showtimes { get; set; } = new();
}

public class ShowtimeApiDto
{
    public int Id { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public string HallName { get; set; } = "";
    public decimal Price { get; set; }
}
