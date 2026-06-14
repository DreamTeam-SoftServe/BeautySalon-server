using System.Collections.Generic;

namespace Application.DTOs
{
    public class CreateOrderItemDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class CreateOrderDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string DeliveryType { get; set; }

        // НОВІ ПОЛЯ
        public string? DeliveryCity { get; set; }
        public string? DeliveryAddress { get; set; }

        public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();
    }
}