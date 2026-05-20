namespace Cinema.Application.Profiles;

public interface IProfileService
{
    Task<ProfileEditDto?> GetProfileAsync(string userId);
    Task<ProfileUpdateResultDto> UpdateProfileAsync(ProfileUpdateDto dto, ProfilePhotoUploadDto? photo);
}
