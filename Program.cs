using API.BackgroundJobs;
using API.Services;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Configuration;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Text;

//Env.Load();
var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);



BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
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
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173");
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
    });

});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

var accessKey = builder.Configuration["AWS:AccessKey"];
var secretKey = builder.Configuration["AWS:SecretKey"];

 if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
{
    var awsOptions = builder.Configuration.GetAWSOptions();
    awsOptions.Credentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey);
    builder.Services.AddDefaultAWSOptions(awsOptions);

    builder.Services.AddAWSService<Amazon.S3.IAmazonS3>();
}
else
{
    Console.WriteLine("AWS keys did not load from the .env file!");
}

builder.Services.AddScoped<IImageService, S3ImageService>();

builder.Services.AddScoped<IRepository<Client>>(sp =>
    new GenericRepository<Client>(
        sp.GetRequiredService<IMongoDbContext>().Client));

builder.Services.AddScoped<IRepository<Client>>(sp =>
    new GenericRepository<Client>(sp.GetRequiredService<IMongoDbContext>().Client));
builder.Services.AddScoped<IRepository<Master>>(sp =>
    new GenericRepository<Master>(sp.GetRequiredService<IMongoDbContext>().Master));
builder.Services.AddScoped<IRepository<Payment>>(sp =>
    new GenericRepository<Payment>(sp.GetRequiredService<IMongoDbContext>().Payment));
builder.Services.AddScoped<IRepository<Product>>(sp =>
    new GenericRepository<Product>(sp.GetRequiredService<IMongoDbContext>().Product));
builder.Services.AddScoped<IRepository<Review>>(sp =>
    new GenericRepository<Review>(sp.GetRequiredService<IMongoDbContext>().Review));
builder.Services.AddScoped<IRepository<Schedule>>(sp =>
    new GenericRepository<Schedule>(sp.GetRequiredService<IMongoDbContext>().Schedule));
builder.Services.AddScoped<IRepository<Service>>(sp =>
    new GenericRepository<Service>(sp.GetRequiredService<IMongoDbContext>().Service));
builder.Services.AddScoped<IRepository<ServiceAppointment>>(sp =>
    new GenericRepository<ServiceAppointment>(sp.GetRequiredService<IMongoDbContext>().ServiceAppointment));
builder.Services.AddScoped<IRepository<WorkDay>>(sp =>
    new GenericRepository<WorkDay>(sp.GetRequiredService<IMongoDbContext>().WorkDay));

builder.Services.AddHostedService<AutoCloseBookingsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}   

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
