namespace Cinema.Web.Areas.Admin.ViewModels;

public class UsersIndexVm
{
    public List<UserListItemVm> Items { get; set; } = new();
}

public class UserListItemVm
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
