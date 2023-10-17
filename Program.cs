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
app.MapGet("/api/validateUser/{uid}", (HHPizzaDbContext db, string uid) =>
{
    var userExists = db.Users.Where(x => x.Uid == uid).FirstOrDefault();
    if (userExists == null)
    {
        return Results.StatusCode(204);
    }
    return Results.Ok(userExists);
});

//Create a User
app.MapPost("/api/user", (HHPizzaDbContext db, User user) =>
{
    db.Users.Add(user);
    db.SaveChanges();
    return Results.Created($"/api/user/{user.Id}", user);
});

//Get user by id
app.MapGet("/api/user/{id}", (HHPizzaDbContext db, int id) =>
{
    var user = db.Users.Single(u => u.Id == id);
    return user;
});

// GET ALL Orders
app.MapGet("/api/order", (HHPizzaDbContext db) =>
{
    return db.Orders.ToList();
});

//Get Closed Orders
// GET ALL Orders
app.MapGet("/api/closed-orders", (HHPizzaDbContext db) =>
{
    return db.Orders.Where(x => x.Status == "closed").ToList();
});

// GET Order by Id
app.MapGet("/api/orderDetails/{id}", (HHPizzaDbContext db, int id) =>
{
    var order = db.Orders.Where(x =>x.Id == id).Include(x => x.Items).FirstOrDefault();
    if(order == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(order);
});

// POST Order
app.MapPost("/api/order", (HHPizzaDbContext db, Order order) =>
{
    db.Orders.Add(order);
    db.SaveChanges();
    return Results.Created($"/api/order/{order.Id}", order);
});

// PUT (update) Order
app.MapPut("/api/order/{id}", (HHPizzaDbContext db, int id, Order order) =>
{
    Order orderToUpdate = db.Orders.Where(x =>x.Id == id).FirstOrDefault();

    if(orderToUpdate == null)
    {
        return Results.NotFound();
    }

    orderToUpdate.Name = order.Name;
    orderToUpdate.CustomerPhone = order.CustomerPhone;
    orderToUpdate.CustomerEmail = order.CustomerEmail;
    orderToUpdate.OrderType = order.OrderType;

    db.SaveChanges();
    return Results.NoContent();

});

//Close Order
app.MapPut("/api/close-order/{id}", (HHPizzaDbContext db, int id, Order order) =>
{
    Order orderToUpdate = db.Orders.Where(x => x.Id == id).FirstOrDefault();

    if (orderToUpdate == null)
    {
        return Results.NotFound();
    }

    orderToUpdate.PaymentType = order.PaymentType;
    orderToUpdate.Tip = order.Tip;
    orderToUpdate.Status = order.Status;
    orderToUpdate.OrderTotal = order.OrderTotal;

    db.SaveChanges();
    return Results.NoContent();

});



// DELETE Order
app.MapDelete("/api/order/{id}", (HHPizzaDbContext db, int id) =>
{
    var order = db.Orders.Where(x => x.Id == id).Include(x => x.Items).FirstOrDefault();
    if (order == null)
    {
        return Results.NotFound();
    }
    foreach (var item in order.Items)
    {
        db.Items.Remove(item);
    }
    db.Orders.Remove(order);
    db.SaveChanges();
    return Results.NoContent();

});

//Items

//GET Item by id
app.MapGet("/api/item/{id}", (HHPizzaDbContext db, int id) =>
{
    var item = db.Items.Where(x => x.Id == id).FirstOrDefault();
    if (item == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(item);
});

//Delete an Item
app.MapDelete("/api/item/{id}", (HHPizzaDbContext db, int id) =>
{
    Item item = db.Items.SingleOrDefault(p => p.Id == id);
    if (item == null)
    {
        return Results.NotFound();
    }
    db.Items.Remove(item);
    db.SaveChanges();
    return Results.NoContent();

});
//Update an Item
app.MapPut("/api/item/{id}", (HHPizzaDbContext db, int id, Item item) =>
{
    Item itemToUpdate = db.Items.SingleOrDefault(product => product.Id == id);
    if (itemToUpdate == null)
    {
        return Results.NotFound();
    }
    itemToUpdate.Name = item.Name;
    itemToUpdate.Price = item.Price;

    db.SaveChanges();
    return Results.NoContent();
});

//Add an item
app.MapPost("/api/item", (HHPizzaDbContext db, Item item) =>
{
    db.Items.Add(item);
    db.SaveChanges();
    return Results.Created($"/api/products/{item.Id}", item);
});



app.MapPost("/api/item/{orderId}", (HHPizzaDbContext db, Item newItem, int orderId) =>
{
    
        var existingOrder = db.Orders
            .Include(o => o.Items)
            .FirstOrDefault(o => o.Id == orderId);

        if (existingOrder == null)
        {
            
            return Results.NotFound();
        }

        // Set the order ID for the new item
        newItem.Orders = new List<Order> { existingOrder };

        db.Items.Add(newItem);
        db.SaveChanges();

    return Results.Created($"/api/item/{orderId}", newItem);

     

});



app.Run();

