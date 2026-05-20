using Cinema.Domain.Entities;

namespace Cinema.Application.Actors;

public interface IActorAdminService
{
    Task<List<Actor>> GetActorsAsync();
    Task<Actor?> GetActorAsync(int id);
    Task<ActorCreateResultDto> CreateActorAsync(ActorCreateDto dto, ActorPhotoUploadDto? photo);
    Task<ActorUpdateResultDto> UpdateActorAsync(ActorUpdateDto dto, ActorPhotoUploadDto? photo);
    Task<bool> DeleteActorAsync(int id);
}
