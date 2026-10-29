using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SabtAhval.Models.Entities
{
    public class ApiRequestLog
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "User Name coulde not Be Null")]
        [StringLength(20)]
        public string? Username { get; set; }
        public string? ClientIp { get; set; }
        [Required(ErrorMessage = "HTTP Method coulde not Be Null")]
        [StringLength(10)]
        public string? HttpMethod {  get; set; }
        [Required(ErrorMessage = "Request Path coulde not Be Null")]
        [StringLength(200)]
        public string? RequestPath {  get; set; }
        [Required(ErrorMessage = "Query String coulde not Be Null")]
        [StringLength(50)]
        public string? QueryString { get; set; }
        public string? RequestBody { get; set; }
        Int16 StatusCode { get; set; }
        [Required(ErrorMessage = "Response Body coulde not Be Null")]
        [StringLength(250)]
        public string? ResponseBody {  get; set; }
        public int ApiClientId {  get; set; }
        [ForeignKey("ApiClientId")]
        ApiClient? ApiClient { get; set; }  


    }
}
