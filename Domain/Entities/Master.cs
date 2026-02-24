using Domain.Enum;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Master : BaseEntity
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public GenderType Gender { get; set; }
        public double Rating { get; set; }
        public int PricePersent { get; set; }
        public ProficiencyType ProfLevel { get; set; }
        public string? Experience { get; set; }
        public ServiceType Specialization { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<ServiceType, int> prices = new Dictionary<ServiceType, int>();
        public Schedule? Sсhedule { get; set; }
        public Review? Reviews { get; set; }
        public string? ImageUrl { get; set; }

    }
}
