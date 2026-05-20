namespace Cinema.Application.Profiles;

public class ProfileEditDto
{
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? ProfilePhotoPath { get; set; }
}

public class ProfileUpdateDto
{
    public string UserId { get; set; } = "";
    [System.ComponentModel.DataAnnotations.Required]
    public string Email { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? ProfilePhotoPath { get; set; }
}

public class ProfilePhotoUploadDto
{
    public string FileName { get; set; } = "";
    public Stream Content { get; set; } = Stream.Null;
    public long Length { get; set; }
}

public class ProfileUpdateResultDto
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}
