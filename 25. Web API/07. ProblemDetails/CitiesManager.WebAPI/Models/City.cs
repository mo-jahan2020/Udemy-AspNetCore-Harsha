using System.ComponentModel.DataAnnotations;

namespace CitiesManager.WebAPI.Models
{
 public class City
 {
  [Key]
  public Guid CityID { get; set; }
        // این تغییر را ایچاد کرده و دستور زیر را زدیم:
        // Add-Migration RequiredCityName
        //update-databse
  [Required(ErrorMessage = "City Name can't be blank")]
  public string? CityName { get; set; }
 }
}
