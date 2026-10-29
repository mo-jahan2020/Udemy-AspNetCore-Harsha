using System.Net.Http.Json;
using SabtAhval.Models.Dtos;

namespace SabtAhval.Services
{
    public class JsonPlaceholderService
    {
        private readonly HttpClient _httpClient;
        public JsonPlaceholderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<List<PostDto>> GetPostsAsync()
        {
            // GetFromJsonAsync هم درخواست GET می‌فرستد و هم JSON را مستقیماً به لیست تبدیل می‌کند
            var posts = await _httpClient.GetFromJsonAsync<List<PostDto>>("posts");
            return posts ?? new List<PostDto>();
        }
        public async Task<PostDto?> GetPostByIdAsync(int id)
        {
            // اگر API 404 برگرداند، GetFromJsonAsync استثنا پرتاب می‌کند، بنابراین باید آن را بگیریم
            try
            {
                return await _httpClient.GetFromJsonAsync<PostDto>($"posts/{id}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null; // پیدا نشد، null برگردان
            }
        }
        public async Task<PostDto?> CreatePostAsync(CreatePostRequestDto request)
        { // PostAsJsonAsync به طور خودکار request را به JSON تبدیل کرده و با Content-Type: application/json ارسال می‌کند
            using var response = await _httpClient.PostAsJsonAsync("posts", request);

            // اگر وضعیت موفق نباشد (4xx/5xx) استثنا پرتاب کن
            response.EnsureSuccessStatusCode();

            // پاسخ برگشتی (پست ایجاد شده) را به PostDto تبدیل کن
            return await response.Content.ReadFromJsonAsync<PostDto>();
        }
    }
}
