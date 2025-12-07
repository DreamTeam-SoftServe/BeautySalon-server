using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enum;

namespace Domain.Entities
{
    public class Master : BaseEntitie
    {
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
