var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//Routing is automatically enabled.
//No need for app.UseRouting() anymore

// Eg: files/sample.txt
app.Map("files/{filename}.{extension}", async context =>
{
 string? fileName = Convert.ToString(context.Request.RouteValues["filename"]);
 string? extension = Convert.ToString(context.Request.RouteValues["extension"]);

 await context.Response.WriteAsync($"In files - {fileName} - {extension}");
});

//مقدار پیش فرض
//Eg: employee/profile/john
app.Map("employee/profile/{EmployeeName=scott}", async context =>
{
 string? employeeName = Convert.ToString(context.Request.RouteValues["employeename"]);
 await context.Response.WriteAsync($"In Employee profile - {employeeName}");
});


//عنصر آی دی میتواند پاس نشود و نال باشد
//Eg: products/details/1
//Eg: products/details  ///Product details - id is not supplied
app.Map("products/details/{id?}", async context =>
{
    //این خط باید اضافه شود
 if (context.Request.RouteValues.ContainsKey("id"))
 {
  int id = Convert.ToInt32(context.Request.RouteValues["id"]);
  await context.Response.WriteAsync($"Product details - {id}");
 }
 else
 {
  await context.Response.WriteAsync($"Product details - id is not supplied");
 }
});


//Fallback for any other requests
app.MapFallback(async (context) =>
{
 await context.Response.WriteAsync($"Request received at {context.Request.Path}");
});


app.Run();
