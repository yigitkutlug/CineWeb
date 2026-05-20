namespace Cinema.Application.AdminMovies;

public class AdminMovieListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Genre { get; set; }
    public int DurationMinutes { get; set; }
    public int ActorCount { get; set; }
    public bool IsFeatured { get; set; }
}

public class ActorOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class AdminMovieEditDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int DurationMinutes { get; set; }
    public string? Genre { get; set; }
    public string? Description { get; set; }
    public string? PosterImagePath { get; set; }
    public bool IsFeatured { get; set; }
    public List<int> SelectedActorIds { get; set; } = new();
}

public class MovieUpsertDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int DurationMinutes { get; set; }
    public string? Genre { get; set; }
    public string? Description { get; set; }
    public string? PosterImagePath { get; set; }
    public bool IsFeatured { get; set; }
    public List<int> SelectedActorIds { get; set; } = new();
}

public class PosterUploadDto
{
    public string FileName { get; set; } = "";
    public Stream Content { get; set; } = Stream.Null;
    public long Length { get; set; }
}

public class MovieUpsertResultDto
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
    public int MovieId { get; set; }
}
