using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CreateMasterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Gender { get; set; } 
        public double Rating { get; set; }
        public int PricePersent { get; set; }
        public int ProfLevel { get; set; } 
        public int Specialization { get; set; } 
        public string? ImageUrl { get; set; }
    }
}
