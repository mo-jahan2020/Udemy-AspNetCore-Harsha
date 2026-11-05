using MinimalAPI.Models;
using MinimalAPI.RouteGroups;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

//Swagger
builder.Services.AddEndpointsApiExplorer(); //Generates description for all endpoints
builder.Services.AddSwaggerGen();


var app = builder.Build();

var mapGroup = app.MapGroup("/products").ProductsAPI();

app.UseSwagger(); //creates endpoint for swagger.json
app.UseSwaggerUI();


app.Run();
