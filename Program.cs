using Infrastructure.Configuration;
using Infrastructure.Data;
using Domain.Entities;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var settings = new MongoDbSettings
{
    ConnectionString = "mongodb+srv://appUser:dCbSxM-GCjG96%25_@cluster0.ezakgjl.mongodb.net/?appName=Cluster0",
    DatabaseName = "BeautySalon"
};

//var context = new MongoDbContext(settings);

//var client = new Client
//{
//    Id = Guid.NewGuid(),
//    Name = "Test3",
//    Email = "Test3"
//};


//await context.Client.InsertOneAsync(client);
//Console.WriteLine("Client inserted successfully.");

//var clients = await context.Client.Find(_ => true).ToListAsync();
//foreach (var c in clients)
//{
//    Console.WriteLine($"Client ID: {c.Id}, Name: {c.Name}, Email: {c.Email}");
//}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
