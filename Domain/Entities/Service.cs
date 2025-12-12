using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enum;

namespace Domain.Entities
{
    public class Service : BaseEntity
    {
        public string Title { get; set; }   
        public int Duration { get; set; }
        public int ServicePrice { get; set; }
        public ServiceType ServiceType { get; set; }
    }
}
