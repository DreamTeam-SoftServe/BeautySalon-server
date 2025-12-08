using MongoDB.Driver;
using Domain.Entities;
using Infrastructure.Configuration;

namespace Infrastructure.Data
{
    public class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _database;
        public MongoDbContext(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
        }
        public IMongoCollection<Client> Client => _database.GetCollection<Client>("Clients");
    }
}
