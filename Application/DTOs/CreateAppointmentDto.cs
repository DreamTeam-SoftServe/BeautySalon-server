using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CreateAppointmentDto
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public Guid ServiceId { get; set; }
        public Guid ClientId { get; set; }
        public Guid MasterId { get; set; }
        public DateTime Start_date { get; set; }
    }
}
