namespace Cinema.Web.Areas.Admin.ViewModels;

public class ActorFormVm
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? Nationality { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? PhotoPath { get; set; }
    public string? Biography { get; set; }
}
