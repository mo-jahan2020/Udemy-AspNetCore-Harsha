using System.ComponentModel.DataAnnotations;
using Entities;

namespace ServiceContracts.DTO
{
    public class CountryAddRequest
    {
        [Required(ErrorMessage = "Country name is required")]
        public string? CountryName { get; set; }
        public Country ToCountry()
        {
            return new Country
            {
                CountryName = this.CountryName
            };
        }
    }
}