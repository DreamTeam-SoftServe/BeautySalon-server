using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Entities
{
    public class Client : BaseEntity
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "Client";
        public Guid? MasterProfileId { get; set; }
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public List<ServiceAppointment>? ServicesHistory { get; set; }
    }
}
