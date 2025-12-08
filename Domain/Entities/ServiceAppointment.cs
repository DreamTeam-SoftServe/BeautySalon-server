using Domain.Entities;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ServiceAppointment : BaseEntitiy
    {
        public int TotalPrice { get; set; }
        public DateTime Start_date { get; set; }
        public Guid ServiceId { get; set; }
        public Service Service { get; set; }
        public Guid ClientId { get; set; }
        public Client Client { get; set; }
        public Guid MasterId { get; set; } 
        public Master Master { get; set; }
        public AppointmentStatus Status { get; set; }
        public Payment Payment { get; set; }
    }
}
