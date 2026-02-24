using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class UpdateRoleDto
    {
        public string Role { get; set; }
        public Guid? MasterProfileId { get; set; }
    }
}
