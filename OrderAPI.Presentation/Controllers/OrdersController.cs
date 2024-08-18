using Azure;
using ecommerceSharedLibrary.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.Application.DTOs;
using OrderAPI.Application.DTOs.Conversions;
using OrderAPI.Application.Interfaces;

namespace OrderAPI.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrder _orderInterface;
        private readonly IOrderService _orderService;

        public OrdersController(IOrder orderInterface, IOrderService orderService)
        {
            _orderInterface = orderInterface;
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrders()
        {
            var orders = await _orderInterface.GetAllAsync();
            if (!orders.Any()) return NotFound("No order detected in the database");

            var (_, list) = OrderConversion.FromEntity(null, orders);

            return !list!.Any() ? NotFound() : Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDTO>> GetOrder(int id)
        {
            var order = await _orderInterface.FindByIdAsync(id);
            if (order == null) return NotFound(null);
            var (_order, _) = OrderConversion.FromEntity(order, null);
            return Ok(_order);
        }

        [HttpGet("client/{clientId:int}")]
        public async Task<ActionResult<OrderDTO>> GetClientOrders(int clientId)
        {
            if (clientId <= 0) return BadRequest("Invalid Data Provided!");

            var orders = await _orderService.GetOrdersByClientId(clientId);
            if (!orders.Any()) return NotFound(null);
            return Ok(orders);
        }

        [HttpGet("details/{orderId:int}")]
        public async Task<ActionResult<OrderDetailsDTO>> GetOrderDetails(int orderId)
        {
            if (orderId <= 0) return BadRequest("Invalid data provided!");
            var orderDetail = await _orderService.GetOrderDetails(orderId);
            return orderDetail.OrderId > 0 ? Ok(orderDetail) : NotFound("No Order Found!");
        }


        [HttpPost] 
        public async Task<ActionResult<Response>> CreateOrder(OrderDTO orderDTO)
        {
            // Check Model State if all data annotations are passed
            if(!ModelState.IsValid) return BadRequest("Incompleted data submitted!");

            // convert to entity
            var getEntity = OrderConversion.ToEntity(orderDTO);
            var response = await _orderInterface.CreateAsync(getEntity);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpPut]
        public async Task<ActionResult<Response>> UpdateOrder(OrderDTO orderDTO)
        {
            // convert from DTO to entity
            var order = OrderConversion.ToEntity(orderDTO);
            var response = await _orderInterface.UpdateAsync(order);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpDelete]
        public async Task<ActionResult<Response>> DeleteOrder(OrderDTO orderDTO)
        {
            // Convert from DTO to Entity
            var order = OrderConversion.ToEntity(orderDTO);
            var response = await _orderInterface.DeleteAsync(order);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

    }
}
