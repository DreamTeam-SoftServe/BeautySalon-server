using Application.DTOs;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IRepository<Order> _orderRepository;

        public OrderController(IRepository<Order> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            // Calculate total price based on items
            decimal total = dto.Items.Sum(item => item.Price * item.Quantity);

            var order = new Order
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                DeliveryType = dto.DeliveryType,
                Status = OrderStatus.PENDING,
                DeliveryCity = dto.DeliveryCity, 
                DeliveryAddress = dto.DeliveryAddress, 
                TotalPrice = total,
                Items = dto.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList(),
                CreatedAt = DateTime.UtcNow
            };

            await _orderRepository.CreateAsync(order);
            return Ok(new { success = true, orderId = order.Id });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderRepository.GetAllAsync();
            return Ok(orders);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return NotFound("Order not found");

            // Приведення числа (0, 1, 2...) до вашого Enum OrderStatus
            order.Status = (OrderStatus)dto.NewStatus;

            await _orderRepository.UpdateAsync(id, order);
            return Ok(new { message = "Status updated successfully" });
        }
    }
}