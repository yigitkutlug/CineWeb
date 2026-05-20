namespace Cinema.Application.Showtimes;

public class ShowtimeListDto
{
    public int Id { get; set; }
    public string MovieTitle { get; set; } = "";
    public string HallName { get; set; } = "";
    public DateTime StartsAtUtc { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}

public class ShowtimeDetailDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = "";
    public string HallName { get; set; } = "";
    public DateTime StartsAtUtc { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}
