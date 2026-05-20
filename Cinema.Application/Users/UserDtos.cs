namespace Cinema.Application.Users;

public class UserListItemDto
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public string? FullName { get; set; }
    public string? City { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool IsAdmin { get; set; }
    public bool IsSuperAdmin { get; set; }
}

public class UserDetailsDto
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? ProfilePhotoPath { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool IsAdmin { get; set; }
    public bool IsSuperAdmin { get; set; }
}

public class UserUpdateDto
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
}

public class UserUpdateResultDto
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}
