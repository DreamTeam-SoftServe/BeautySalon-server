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
        public IMongoCollection<Master> Master { get; }
        public IMongoCollection<Payment> Payment { get; }
        public IMongoCollection<Product> Product { get; }
        public IMongoCollection<Review> Review { get; }
        public IMongoCollection<Schedule> Schedule { get; }
        public IMongoCollection<Service> Service { get; }
        public IMongoCollection<ServiceAppointment> ServiceAppointment { get; }
        public IMongoCollection<WorkDay> WorkDay { get; }

    }
}
