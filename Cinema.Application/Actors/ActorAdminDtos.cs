using Cinema.Domain.Entities;

namespace Cinema.Application.Actors;

public class ActorCreateDto
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Nationality { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Biography { get; set; }
}

public class ActorUpdateDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Nationality { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? PhotoPath { get; set; }
    public string? Biography { get; set; }
}

public class ActorPhotoUploadDto
{
    public string FileName { get; set; } = "";
    public Stream Content { get; set; } = Stream.Null;
    public long Length { get; set; }
}

public class ActorCreateResultDto
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
    public Actor? Actor { get; set; }
}

public class ActorUpdateResultDto
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
    public Actor? Actor { get; set; }
}
