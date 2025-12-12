using MongoDB.Driver;
using Domain.Entities;
using Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Data
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _database;
        public MongoDbContext(IOptions<MongoDbSettings> options)
        {
            var settings = options.Value;
            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
        }
        public IMongoCollection<Client> Client => _database.GetCollection<Client>("Clients");
        public IMongoCollection<Master> Master => _database.GetCollection<Master>("Masters");
        public IMongoCollection<Payment> Payment  => _database.GetCollection<Payment>("Payments");
        public IMongoCollection<Product> Product => _database.GetCollection<Product>("Products");
        public IMongoCollection<Review> Review => _database.GetCollection<Review>("Reviews");
        public IMongoCollection<Schedule> Schedule => _database.GetCollection<Schedule>("Schedules");
        public IMongoCollection<Service> Service => _database.GetCollection<Service>("Services");
        public IMongoCollection<ServiceAppointment> ServiceAppointment => _database.GetCollection<ServiceAppointment>("ServiceAppointments");
        public IMongoCollection<WorkDay> WorkDay => _database.GetCollection<WorkDay>("WorkDays");


    }
}
