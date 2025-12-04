using Domain.Entities;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class ServiceAppointment
    {
        public int id { get; set; }
        public int totalPrice { get; set; }
        public DateTime start_date { get; set; }
        public int serviceId { get; set; }
        public Service service { get; set; }
        public int clientId { get; set; }
        public Client client { get; set; }
        public int masterId { get; set; } 
        public Master master { get; set; }
        public AppointmentStatus status { get; set; }
        public Payment Payment { get; set; }
    }
}
