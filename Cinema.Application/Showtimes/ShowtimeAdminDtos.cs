namespace Cinema.Application.Showtimes;

public class ShowtimeAdminUpsertDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public int HallId { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ShowtimeAdminItemDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public int HallId { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}
