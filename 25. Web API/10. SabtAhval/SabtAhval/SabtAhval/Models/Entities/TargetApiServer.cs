using System.ComponentModel.DataAnnotations;

namespace SabtAhval.Models.Entities
{
    public class TargetApiServer
    {
        [Key]
        public int Id {  get; set; }

        [Required(ErrorMessage = "Name coulde not Be Null")]
        [StringLength(20)]
        public string? Name { get; set; }
        [Required(ErrorMessage = "BaseURL coulde not Be Null")]
        [StringLength(200)]
        public string? BaseUrl { get; set; }
        public int TimeoutSeconds {  get; set; }
        public bool IsActive {  get; set; }
        [Required(ErrorMessage = "Description coulde not Be Null")]
        [StringLength(200)]
        public string? Description {  get; set; }
        [Required(ErrorMessage = "Create At coulde not Be Null")]
        [StringLength(25)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
