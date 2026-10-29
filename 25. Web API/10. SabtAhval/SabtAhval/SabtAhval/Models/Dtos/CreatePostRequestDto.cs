namespace SabtAhval.Models.Dtos
{
    public class CreatePostRequestDto
    {
        public int userId { get; set; }
        public string? title { get; set; }
        public string? body { get; set; }

    }
}
