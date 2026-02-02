using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Configuration;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

//Env.Load();
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings")
);

builder.Services.AddSingleton<IMongoDbContext, MongoDbContext>();

var context = builder.Services.BuildServiceProvider().GetRequiredService<IMongoDbContext>();

builder.Services.AddScoped<IRepository<Client>>(sp => new MongoRepository<Client>(context.Client));
builder.Services.AddScoped<IRepository<Master>>(sp => new MongoRepository<Master>(context.Master));
builder.Services.AddScoped<IRepository<Payment>>(sp => new MongoRepository<Payment>(context.Payment));
builder.Services.AddScoped<IRepository<Product>>(sp => new MongoRepository<Product>(context.Product));
builder.Services.AddScoped<IRepository<Review>>(sp => new MongoRepository<Review>(context.Review));
builder.Services.AddScoped<IRepository<Schedule>>(sp => new MongoRepository<Schedule>(context.Schedule));
builder.Services.AddScoped<IRepository<Service>>(sp => new MongoRepository<Service>(context.Service));
builder.Services.AddScoped<IRepository<ServiceAppointment>>(sp => new MongoRepository<ServiceAppointment>(context.ServiceAppointment));
builder.Services.AddScoped<IRepository<WorkDay>>(sp => new MongoRepository<WorkDay>(context.WorkDay));

var app = builder.Build();

// Confidgure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
