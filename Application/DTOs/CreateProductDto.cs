using Domain.Enum;

namespace Application.DTOs
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string Brand { get; set; }
        public ProductCategory Category { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }
        public int Volume { get; set; }
        public string ImgUrl { get; set; } // Just a string now!
    }
}