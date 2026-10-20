using Entities;
namespace ServiceContracts.DTO
{
    public class CountryResponse
    {
        public Guid CountryID { get; set; }
        public string? CountryName { get; set; }
        public override bool Equals(object? obj)
        {
            if(obj ==null || obj.GetType() != typeof(CountryResponse))
                return false;
            
            CountryResponse countryResponse = (CountryResponse)obj;
            if(countryResponse == null)
                return false;
            return this.CountryID == countryResponse.CountryID && this.CountryName == countryResponse.CountryName;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    public static class CountryExtensions
    {
        public static CountryResponse ToCountryResponse(this Country country)
        {
            return new CountryResponse
            {
                CountryID = country.CountryID,
                CountryName = country.CountryName
            };
        }
    }   
}
