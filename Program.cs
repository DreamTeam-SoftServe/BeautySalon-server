using Domain.Entities;
using Infrastructure.Configuration;
using Infrastructure.Data;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings")
);

builder.Services.AddSingleton<IMongoDbContext, MongoDbContext>();

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var context = scope.ServiceProvider.GetRequiredService<IMongoDbContext>();

//    var client = new Client
//    {
//        Id = Guid.NewGuid(),
//        Name = "Test4",
//        Email = "test4@example.com"
//    };

//    try
//    {
//        await context.Client.InsertOneAsync(client);
//        Console.WriteLine($"[Success] Client '{client.Name}' inserted.");

//        var clients = await context.Client.Find(_ => true).ToListAsync();
//        Console.WriteLine($"Total clients in DB: {clients.Count}");
//        foreach (var c in clients)
//        {
//            Console.WriteLine($" - {c.Name} ({c.Email})");
//        }
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"[Error] MongoDB issue: {ex.Message}");
//    }
//}

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
