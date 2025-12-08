using Domain.Entities;
using Infrastructure.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public interface IMongoDbContext
    {
        public IMongoCollection<Client> Client { get; }
    }
}
