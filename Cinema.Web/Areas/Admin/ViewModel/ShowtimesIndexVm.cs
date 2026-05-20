using Cinema.Domain.Entities;

namespace Cinema.Web.Areas.Admin.ViewModels;

public class ShowtimesIndexVm
{
    public List<Showtime> Items { get; set; } = new();
    public Movie? Movie { get; set; }
    public bool IsAllShowtimes { get; set; }
    public ShowtimeFormVm QuickCreateForm { get; set; } = new();
}
