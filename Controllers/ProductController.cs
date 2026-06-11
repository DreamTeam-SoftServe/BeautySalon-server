using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IRepository<Product> _productRepository;

        public ProductController(IRepository<Product> productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productRepository.GetAllAsync();
            return Ok(products);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Brand = dto.Brand,
                category = dto.Category, // Make sure to capitalize 'Category' in your Domain Entity if you want to follow C# conventions
                Price = dto.Price,
                Stock = dto.Stock,
                Description = dto.Description,
                Volume = dto.Volume,
                ImgUrl = dto.ImgUrl
            };

            await _productRepository.CreateAsync(product);
            return Ok(product);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id) // <--- Змінено на Guid
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id) // <--- Змінено на Guid
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound("Product not found");

            await _productRepository.DeleteAsync(id);
            return Ok(new { message = "Product deleted successfully" });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateProductDto dto) // <--- Змінено на Guid
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound("Product not found");

            product.Name = dto.Name;
            product.Brand = dto.Brand;
            product.category = dto.Category;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.Description = dto.Description;
            product.Volume = dto.Volume;

            if (!string.IsNullOrEmpty(dto.ImgUrl))
            {
                product.ImgUrl = dto.ImgUrl;
            }

            await _productRepository.UpdateAsync(id, product);
            return Ok(product);
        }
    }
}