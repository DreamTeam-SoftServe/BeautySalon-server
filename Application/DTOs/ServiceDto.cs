using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ServiceDto
    {
        public Guid? Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ServicePrice { get; set; }
        public int ServiceType { get; set; }
        public int Duration { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsTraining { get; set; }
    }
}
