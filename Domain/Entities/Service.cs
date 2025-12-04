using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Service
    {
        public int Id { get; set; }
        public string Title { get; set; }   
        public int Duration { get; set; }
        public int ServicePrice { get; set; }
        public ServiceType ServiceType { get; set; }
    }
}
