
using Microsoft.AspNetCore.Mvc;
using MinimalAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MinimalAPI.RouteGroups
{
    public static class ProductsMapGroup
    {
        private static List<Product> products = new List<Product>() {
   new Product() { Id = 1, ProductName = "Smart Phone" },
   new Product() { Id = 2, ProductName = "Smart TV" }
  };

        public static RouteGroupBuilder ProductsAPI(this RouteGroupBuilder group)
        {

            //GET /products
            group.MapGet("/", async (HttpContext context) =>
            {
                //Sample Response:
                //[{ "Id": 1, "ProductName": "Smart Phone" }, { "Id": 2, "ProductName": "Smart TV" }]

                return Results.Ok(products);
            });


            //GET /products/{id}
            group.MapGet("/{id:int}", async (HttpContext context, int id) =>
            {
                Product? product = products.FirstOrDefault(temp => temp.Id == id);
                if (product == null)
                {
                    return Results.BadRequest(new { error = "Incorrect Product ID" });
                }

                //Sample Response:
                //{ "Id": 1, "ProductName": "Smart Phone" }

                return Results.Ok(product);
            });


            //POST /products
            group.MapPost("/", (HttpContext context, Product product) =>
            {
                // 1- قبلار ورود به این خط 
                // 4- حالا این خط اجرا میشود
                products.Add(product);

                //Sample Response:
                //{ "message": "Product Added" }

                return Results.Ok(new { message = "Product Added" });
                // 5- و از اینجا به after میرود
            })
             .AddEndpointFilter(async (EndpointFilterInvocationContext context, EndpointFilterDelegate next) =>
             {
                 // endpoint Filter ها مانند action Filter ها قبل و بعد از minimal Api ها اجرا میشوند
                 // قبل از منطق و بعد از منطق اجرای یک Minimal API
                 // قبل از اجرا بیستر روی validation متمرکز است
                 // بعد از اجرا به کاری که میخواهید بعد اجرای یک Minimal API انجام دهید متمرکز است

                 //Beforr logic here
                 // 2- وارد اینجا میشود
                 var product = context.Arguments.OfType<Product>().FirstOrDefault();

                 if (product == null)
                 {
                     return Results.BadRequest("Product details are not found in the request");
                 }

                 var validationContext = new ValidationContext(product);
                 List<ValidationResult> errors = new List<ValidationResult>();
                 bool isValid = Validator.TryValidateObject(product, validationContext, errors, true);

                 if (!isValid)
                 {
                     return Results.BadRequest(errors.FirstOrDefault()?.ErrorMessage);
                 }
                 Console.WriteLine("Befor Runing minimal API");
                 var result = await next(context); //invokes the subsequent endpoint filter or endpoint's request delegate
                 // 3- همه چیز درست بود از اینجا برمیگردد به اجری Minimal API


                 //After logic here
                 //6- بعد از اجرا به اینجا منتقل میشود
                 Console.WriteLine("After Runing minimal API");

                 return result;
                 // 7 - اجرای minimal API پایان یافت
             })
             ;


            //PUT /products/{id}
            group.MapPut("/{id}", async (HttpContext context, int id, [FromBody] Product product) =>
            {
                Product? productFromCollection = products.FirstOrDefault(temp => temp.Id == id);
                if (productFromCollection == null)
                {
                    //Sample Response:
                    //{ "error": "Invalid Product ID" }

                    return Results.BadRequest(new { error = "Incorrect Product ID" });
                }

                productFromCollection.ProductName = product.ProductName;

                //Sample Response:
                //{ "message": "Product Updated" }

                return Results.Ok(new { message = "Product Updated" });
            });


            //DELETE /products/{id}
            group.MapDelete("/{id}", async (HttpContext context, int id) =>
            {
                Product? productFromCollection = products.FirstOrDefault(temp => temp.Id == id);
                if (productFromCollection == null)
                {
                    /* 
                    Sample Response:
                    {
                     "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                     "title": "One or more validation errors occurred.",
                     "status": 400,
                     "errors": {
                         "id": [
                             "Incorrect Product ID"
                         ]
                      }
                    }
                    */

                    return Results.ValidationProblem(
                     new Dictionary<string, string[]> {
       {  "id", new  string[] { "Incorrect Product ID" } }
                     });
                }

                products.Remove(productFromCollection);

                //Sample Response:
                //{ "message": "Product Deleted" }

                return Results.Ok(new { message = "Product Deleted" });
            });


            return group;
        }
    }
}
