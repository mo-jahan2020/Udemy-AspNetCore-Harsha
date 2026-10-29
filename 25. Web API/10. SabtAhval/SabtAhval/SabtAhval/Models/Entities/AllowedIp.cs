using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SabtAhval.Models.Entities
{
    public class AllowedIp
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "IpAddress coulde not Be Null")]
        [StringLength(20)]
        public string IpAddress { get; set; }

        public bool IsActive { get; set; }

        public int ApiClientId {  get; set; }
        [ForeignKey(nameof(ApiClientId))]
        ApiClient? ApiClient { get; set; }   
    }
}
