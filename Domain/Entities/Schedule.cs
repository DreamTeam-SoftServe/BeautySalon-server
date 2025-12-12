using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Schedule : BaseEntity
    {
        public Guid MasterId { get; set; }
        public Master Master { get; set; }
        public List<WorkDay> WorkDays { get; set; }
        public List<ServiceAppointment> Appointments { get; set; }
    }
}