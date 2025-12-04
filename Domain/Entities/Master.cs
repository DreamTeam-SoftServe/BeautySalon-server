using Domain.Entities;
using Domain.Entities.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Master
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public GenderType Gender { get; set; }
        public double Rating { get; set; }
        public int PricePersent { get; set; }
        public ProficiencyType ProfLevel { get; set; }
        public ServiceType Specialization { get; set; }

        public Dictionary<ServiceType, int> prices = new Dictionary<ServiceType, int>();
        public Schedule Sсhedule { get; set; }
        public Review Reviews { get; set; }


    }
}
