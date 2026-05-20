using Cinema.Application.Showtimes;
using Cinema.Domain.Entities;

namespace Cinema.Web.ViewModels;

public class HomePageVm
{
    public List<Movie> FeaturedMovies { get; set; } = new();
    public List<ShowtimeListDto> UpcomingShowtimes { get; set; } = new();
}
