using OrderAPI.Application.DTOs;
using OrderAPI.Application.DTOs.Conversions;
using OrderAPI.Application.Interfaces;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace OrderAPI.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly ResiliencePipelineProvider<string> _resiliencePipelineProvider;
        private readonly IOrder _orderInterface;

        public OrderService(HttpClient httpClient, ResiliencePipelineProvider<string> resiliencePipelineProvider, IOrder order)
        {
            _httpClient = httpClient;
            _resiliencePipelineProvider = resiliencePipelineProvider;  
            _orderInterface = order;
        }

        // GET Product
        public async Task<ProductDTO> GetProduct(int productId)
        {
            // Call Product API using HttpClient
            // Redirect this call to the API Gateway since product API is not response to outsiders.
            var getProduct = await _httpClient.GetAsync($"/api/products/{productId}");
            if (!getProduct.IsSuccessStatusCode)
            {
                return null;
            }
            var product = await getProduct.Content.ReadFromJsonAsync<ProductDTO>();
            return product!;
        }

        // GET User
        public async Task<AppUserDTO> GetUser(int userId)
        {
            // Call Product API using HttpClient
            // Redirect this call to the API Gateway since product API is not response to outsiders.
            var getUser = await _httpClient.GetAsync($"/api/Authentication/{userId}");
            if (!getUser.IsSuccessStatusCode)
            {
                return null;
            }
            var user = await getUser.Content.ReadFromJsonAsync<AppUserDTO>();
            return user!;
        }


        // GET Order Details By Id
        public async Task<OrderDetailsDTO> GetOrderDetails(int orderId)
        {
            // Prepare Order
            var order = await _orderInterface.FindByIdAsync(orderId);
            if(order is null || order!.Id <= 0)
            {
                return null;
            }

            // Get Retry Pipeline
            var retryPipeline = _resiliencePipelineProvider.GetPipeline("my-retry-pipeline");

            // Prepared Product
            var productDTO = await retryPipeline.ExecuteAsync(async token => await GetProduct(order.ProductId));

            // Prepared Client
            var appUserDTO = await retryPipeline.ExecuteAsync(async token => await GetUser(order.ClientId));

            // Populate Order Details
            return new OrderDetailsDTO(
                order.Id,
                productDTO.Id,
                appUserDTO.Id,
                appUserDTO.Name,
                appUserDTO.Email,
                appUserDTO.Address,
                appUserDTO.TelephoneNumber,
                productDTO.Name,
                order.PurchaseQuantity,
                productDTO.Price,
                productDTO.Quantity * order.PurchaseQuantity,
                order.OrderedDate
                );
        }

        // Get Orders By ClientId
        public async Task<IEnumerable<OrderDTO>> GetOrdersByClientId(int clientId)
        {
            // Get all Clients Orders
            var ordersData = await _orderInterface.GetOrdersAsync(o => o.ClientId == clientId);
            if(!ordersData.Any()) return null!;

            // Convert from entity to DTO
            var (_, _orders) = OrderConversion.FromEntity(null, ordersData);

            return _orders!;
        }
    }
}
