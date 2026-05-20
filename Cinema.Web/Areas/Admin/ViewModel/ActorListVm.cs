namespace Cinema.Web.Areas.Admin.ViewModels;

public class ActorListItemVm
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string? PhotoPath { get; set; }
    public int? Age { get; set; }
    public string? Nationality { get; set; }
    public DateTime? BirthDate { get; set; }
}

public class ActorListVm
{
    public List<ActorListItemVm> Items { get; set; } = new();
}
