using ContactsManager.Core.Domain.IdentityEntities;
using CRUDExample.Filters.ActionFilters;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;

namespace CRUDExample
{
 public static class ConfigureServicesExtension
 {
  public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
  {
   services.AddTransient<ResponseHeaderActionFilter>();

   //it adds controllers and views as services
   services.AddControllersWithViews(options => {
    //options.Filters.Add<ResponseHeaderActionFilter>(5);

    var logger = services.BuildServiceProvider().GetRequiredService<ILogger<ResponseHeaderActionFilter>>();

    options.Filters.Add(new ResponseHeaderActionFilter(logger)
    {
     Key = "My-Key-From-Global",
     Value = "My-Value-From-Global",
     Order = 2
    });
   });

   //add services into IoC container
   services.AddScoped<ICountriesRepository, CountriesRepository>();
   services.AddScoped<IPersonsRepository, PersonsRepository>();

   services.AddScoped<ICountriesGetterService, CountriesGetterService>();
   services.AddScoped<ICountriesAdderService, CountriesAdderService>();
   services.AddScoped<ICountriesUploaderService, CountriesUploaderService>();

   services.AddScoped<IPersonsGetterService, PersonsGetterServiceWithFewExcelFields>();
   services.AddScoped<PersonsGetterService, PersonsGetterService>();

   services.AddScoped<IPersonsAdderService, PersonsAdderService>();
   services.AddScoped<IPersonsDeleterService, PersonsDeleterService>();
   services.AddScoped<IPersonsUpdaterService, PersonsUpdaterService>();
   services.AddScoped<IPersonsSorterService, PersonsSorterService>();

   services.AddDbContext<ApplicationDbContext>(options =>
   {
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
   });

   services.AddTransient<PersonsListActionFilter>();


            // سرویس اضافه میشود تا برنامه در ابتدای اجرا از آن استفاده کند
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddDefaultTokenProviders()// افزودن توکن پرووایدر که در تغییر کلمه عبور و ایمیل و تلفن استفاده میشود
                .AddEntityFrameworkStores<ApplicationDbContext>()// این خط برای مشخ شدن محل نگهداری اطلاعات Indentiry میباشد این در لایه اپلیکیشن اننجام میشود
                .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()// این ذخیره کاربران در لایه ریپازیتوری انجام میشود
                .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>()// این ذخیره نقش ها در لایه ریپازیتوری انجام میشود
                ;



            services.AddHttpLogging(options =>
   {
    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
   });

   return services;
  }
 }
}
