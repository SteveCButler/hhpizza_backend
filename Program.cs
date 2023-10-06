using hhpizza_backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
//ADD CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:3000",
                                "http://localhost:7129")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
        });
});


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core
builder.Services.AddNpgsql<HHPizzaDbContext>(builder.Configuration["HHPizzaDbConnectionString"]);

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();
//Add for Cors
app.UseCors(MyAllowSpecificOrigins);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//   ENDPOINTS

//CHECK USER EXISTS
app.MapGet("/api/checkuser/{uid}", (HHPizzaDbContext db, string uid) =>
{
    var userExists = db.Users.Where(x => x.Uid == uid).FirstOrDefault();
    if (userExists == null)
    {
        return Results.StatusCode(204);
    }
    return Results.Ok(userExists);
});

// GET ALL Orders
app.MapGet("/api/order", (HHPizzaDbContext db) =>
{
    return db.Orders.ToList();
});

// POST Order
app.MapPost("/api/order", (HHPizzaDbContext db, Order order) =>
{
    db.Orders.Add(order);
    db.SaveChanges();
    return Results.Created($"/api/order/{order.Id}", order);
});

// DELETE Order
app.MapDelete("/api/order/{id}", (HHPizzaDbContext db, int id) =>
{
    var orderToDelete = db.Orders.Where(x =>x.Id == id).FirstOrDefault();
    if(orderToDelete == null)
    {
        return Results.NotFound();
    }
    db.Orders.Remove(orderToDelete);
    db.SaveChanges();
    return Results.NoContent();

});





app.Run();

