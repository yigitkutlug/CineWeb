namespace Cinema.Application.Actors;

public class ActorListItemDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string? PhotoPath { get; set; }
    public int? Age { get; set; }
    public string? Nationality { get; set; }
    public DateTime? BirthDate { get; set; }
}

public class ActorDetailDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Nationality { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? PhotoPath { get; set; }
    public string? Biography { get; set; }
}
