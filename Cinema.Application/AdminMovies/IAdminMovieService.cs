namespace Cinema.Application.AdminMovies;

public interface IAdminMovieService
{
    Task<List<AdminMovieListDto>> GetMoviesAsync();
    Task<List<ActorOptionDto>> GetActorOptionsAsync();
    Task<AdminMovieEditDto?> GetMovieForEditAsync(int id);
    Task<MovieUpsertResultDto> CreateMovieAsync(MovieUpsertDto dto, PosterUploadDto? poster);
    Task<MovieUpsertResultDto> UpdateMovieAsync(MovieUpsertDto dto, PosterUploadDto? poster);
    Task<bool> DeleteMovieAsync(int id);
}
