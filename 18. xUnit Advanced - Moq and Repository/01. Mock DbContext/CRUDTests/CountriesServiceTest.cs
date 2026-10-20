using System;
using System.Collections.Generic;
using Entities;
using ServiceContracts.DTO;
using ServiceContracts;
using Services;
using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using EntityFrameworkCoreMock;
using Moq;
/*
 * سرویس‌های شما (CountriesService و PersonsService) برای کار کردن به یک دیتابیس واقعی (ApplicationDbContext) وابسته‌اند. اگر بخواهیم مستقیماً روی SQL Server تست بنویسیم، با این مشکلات روبرو می‌شویم:

تست‌ها کند می‌شوند (اتصال شبکه، I/O دیسک).
داده‌های واقعی ممکن است خراب شوند.
تست‌ها به وضعیت دیتابیس وابسته می‌شوند و قابل تکرار نیستند.
راه‌اندازی محیط تست سخت می‌شود.
راه‌حل: به‌جای دیتابیس واقعی، یک «دیتابیس جعلی» (Mock) در حافظه می‌سازیم که رفتار دیتابیس واقعی را شبیه‌سازی می‌کند. اینجاست که Moq وارد می‌شود.
Moq یک کتابخانه Mocking برای دات‌نت است که اجازه می‌دهد برای هر interface یا کلاس virtual، یک شیء جعلی بسازید و رفتارش را کنترل کنید. الگوی اصلی Moq این است:

var mock = new Mock<ISomeDependency>();      // ۱. ساخت mock
mock.Setup(x => x.SomeMethod()).Returns(true); // ۲. تعریف رفتار
ISomeDependency instance = mock.Object;       // ۳. گرفتن نمونه قابل استفاده-
در این پروژه، چون وابستگی اصلی ApplicationDbContext (یک کلاس EF Core) است، از کتابخانه کمکی EntityFrameworkCoreMock استفاده شده که روی Moq سوار شده و کار ساخت Mock برای DbContext و DbSet<T> را خیلی ساده می‌کند.


 */
namespace CRUDTests
{
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countriesService;

        //constructor
        public CountriesServiceTest()
        {
            // ۱. داده‌های اولیه خالی برای Countries
            var countriesInitialData = new List<Country>() { };

            // ۲. ساخت یک Mock از ApplicationDbContext
            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(
              new DbContextOptionsBuilder<ApplicationDbContext>().Options
             );

            // ۳. گرفتن شیء واقعی-شبیه‌سازی‌شده از داخل Mock
            ApplicationDbContext dbContext = dbContextMock.Object;

            // ۴. ساخت Mock برای DbSet<Country> و اتصال آن به لیست initial data
            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);

            // ۵. تزریق DbContext جعلی به سرویس واقعی
            _countriesService = new CountriesService(dbContext);
            // همین کار را برای پرسون تست هم انجام میدهیم:
            //پس یک DbContext جعلی واحد، به دو سرویس تزریق می‌شود؛ به‌همین دلیل تست‌ها می‌توانند ابتدا یک کشور بسازند و بعد شخصی با همان CountryID وارد کنند.
        }

        #region AddCountry
        //When CountryAddRequest is null, it should throw ArgumentNullException
        [Fact]
        public async Task AddCountry_NullCountry()
        {
            //Arrange
            CountryAddRequest? request = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                //Act
                await _countriesService.AddCountry(request);
            });
        }

        //When the CountryName is null, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_CountryNameIsNull()
        {
            //Arrange
            CountryAddRequest? request = new CountryAddRequest() { CountryName = null };

            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                //Act
                await _countriesService.AddCountry(request);
            });
        }


        //When the CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName()
        {
            //Arrange
            CountryAddRequest? request1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest? request2 = new CountryAddRequest() { CountryName = "USA" };

            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                //Act
                await _countriesService.AddCountry(request1);
                await _countriesService.AddCountry(request2);
            });
        }


        //When you supply proper country name, it should insert (add) the country to the existing list of countries
        [Fact]
        public async Task AddCountry_ProperCountryDetails()
        {
            //Arrange
            CountryAddRequest? request = new CountryAddRequest() { CountryName = "Japan" };

            //Act
            CountryResponse response = await _countriesService.AddCountry(request);
            List<CountryResponse> countries_from_GetAllCountries = await _countriesService.GetAllCountries();

            //Assert
            Assert.True(response.CountryID != Guid.Empty);
            Assert.Contains(response, countries_from_GetAllCountries);
        }

        #endregion


        #region GetAllCountries

        [Fact]
        //The list of countries should be empty by default (before adding any countries)
        public async Task GetAllCountries_EmptyList()
        {
            //Act
            List<CountryResponse> actual_country_response_list = await _countriesService.GetAllCountries();

            //Assert
            Assert.Empty(actual_country_response_list);
        }


        [Fact]
        public async Task GetAllCountries_AddFewCountries()
        {
            //Arrange
            List<CountryAddRequest> country_request_list = new List<CountryAddRequest>() {
        new CountryAddRequest() { CountryName = "USA" },
        new CountryAddRequest() { CountryName = "UK" }
      };

            //Act
            List<CountryResponse> countries_list_from_add_country = new List<CountryResponse>();

            foreach (CountryAddRequest country_request in country_request_list)
            {
                countries_list_from_add_country.Add(await _countriesService.AddCountry(country_request));
            }

            List<CountryResponse> actualCountryResponseList = await _countriesService.GetAllCountries();

            //read each element from countries_list_from_add_country
            foreach (CountryResponse expected_country in countries_list_from_add_country)
            {
                Assert.Contains(expected_country, actualCountryResponseList);
            }
        }
        #endregion


        #region GetCountryByCountryID

        [Fact]
        //If we supply null as CountryID, it should return null as CountryResponse
        public async Task GetCountryByCountryID_NullCountryID()
        {
            //Arrange
            Guid? countrID = null;

            //Act
            CountryResponse? country_response_from_get_method = await _countriesService.GetCountryByCountryID(countrID);


            //Assert
            Assert.Null(country_response_from_get_method);
        }


        [Fact]
        //If we supply a valid country id, it should return the matching country details as CountryResponse object
        public async Task GetCountryByCountryID_ValidCountryID()
        {
            //Arrange
            CountryAddRequest? country_add_request = new CountryAddRequest() { CountryName = "China" };
            CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);

            //Act
            CountryResponse? country_response_from_get = await _countriesService.GetCountryByCountryID(country_response_from_add.CountryID);

            //Assert
            Assert.Equal(country_response_from_add, country_response_from_get);
        }
        #endregion
    }
}
