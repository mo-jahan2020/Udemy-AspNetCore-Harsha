using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SabtAhval.Models.Entities
{
    [Index(nameof(Username), IsUnique = true)]
    public class ApiClient
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "User Name coulde not Be Null")]
        [StringLength (20)]
        public string Username { get; set; }

        [Required(ErrorMessage = "ApiKeyHash coulde not Be Null")]
        [StringLength(256)]
        public string ApiKeyHash { get; set; }
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Created At coulde not Be Null")]
        [StringLength(25)]
        public string CreatedAt { get; set; }

        public int TargetApiServerId { get; set; }
        [ForeignKey("TargetApiServerId")]
        public TargetApiServer? TargetApiServer {  get; set; }

    }
}
