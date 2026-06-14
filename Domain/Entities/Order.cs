using Domain.Enum;
using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Order : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string DeliveryType { get; set; }

        public string? DeliveryCity { get; set; }
        public string? DeliveryAddress { get; set; }

        public OrderStatus Status { get; set; }
        public decimal TotalPrice { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}