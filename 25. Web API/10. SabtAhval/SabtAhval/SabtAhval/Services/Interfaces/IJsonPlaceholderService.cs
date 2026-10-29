using SabtAhval.Models.Dtos;

namespace SabtAhval.Services.Interfaces
{
    public interface IJsonPlaceholderService
    {
        Task<List<PostDto>> GetPostsAsync();
        Task<PostDto?> GetPostByIdAsync(int id);
        Task<PostDto?> CreatePostAsync(CreatePostRequestDto request);

    }
}
