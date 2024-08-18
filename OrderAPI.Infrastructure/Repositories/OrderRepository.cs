using ecommerceSharedLibrary.Logs;
using ecommerceSharedLibrary.Response;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Application.DTOs;
using OrderAPI.Application.Interfaces;
using OrderAPI.Domain.Entities;
using OrderAPI.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OrderAPI.Infrastructure.Repositories
{
    public class OrderRepository(OrderDbContext context) : IOrder
    {
        public async Task<ResponseModel> CreateAsync(Order entity)
        {
            try
            {
                var order = context.Orders.Add(entity).Entity;
                await context.SaveChangesAsync();
                return order.Id > 0 ? new ResponseModel(true, "Order placed Succesfully") : new ResponseModel(false, "Error ocurred while placing order");

            }catch (Exception ex)
            {
                // Log Original Exception
                LogException.LogExceptions(ex);

                // Display scary-free message to client
                return new ResponseModel(false, "Error ocurred while placing order");
            }
        }

        public async Task<ResponseModel> DeleteAsync(Order entity)
        {
            try
            {
                var order = await FindByIdAsync(entity.Id);
                
                if(order is null) return new ResponseModel(false, "Order not found!");

                context.Orders.Remove(order);
                await context.SaveChangesAsync();
                return new ResponseModel(true, "Order Successfully deleted!");
            }
            catch (Exception ex)
            {
                // Log Original Exception
                LogException.LogExceptions(ex);

                // Display scary-free message to client
                return new ResponseModel(false, "Error ocurred while placing order");
            }
        }

        public async Task<Order> FindByIdAsync(int id)
        {
            try
            {
                var order = await context.Orders!.FindAsync(id);
                return order is not null ? order : null!;
            }
            catch (Exception ex)
            {
                // Log Original Exception
                LogException.LogExceptions(ex);

                // Display scary-free message to client
                throw new Exception("Error ocurred while retrieving order");
            }
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            try
            {
                var orders = await context.Orders.AsNoTracking().ToListAsync();
                return orders is not null ? orders : null!;
            }
            catch (Exception ex)
            {
                // Log Original Exception
                LogException.LogExceptions(ex);

                // Display scary-free message to client
                throw new Exception("Error ocurred while retrieving order");
            }
        }

        public async Task<Order> GetByAsync(Expression<Func<Order, bool>> predicate)
        {
            try
            {
                var order = await context.Orders.Where(predicate).FirstOrDefaultAsync();
                return order is not null ? order : null!;
            }
            catch (Exception ex)
            {
                // Log Original Exception
                LogException.LogExceptions(ex);

                // Display scary-free message to client
                throw new Exception("Error ocurred while retrieving order");
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync(Expression<Func<Order, bool>> predicate)
        {
            try
            {
                var orders = await context.Orders.Where(predicate).ToListAsync();
                return orders is not null ? orders : null!;
            }
            catch (Exception ex)
            {
                // Log Original Exception
                LogException.LogExceptions(ex);

                // Display scary-free message to client
                throw new Exception("Error ocurred while retrieving order");
            }
        }

        public async Task<ResponseModel> UpdateAsync(Order entity)
        {
            try
            {
                // Buscar la orden existente por su Id
                var order = await FindByIdAsync(entity.Id);

                if (order is null) return new ResponseModel(false, "Order not found");

                // Actualizar las propiedades de la orden existente
                order.ProductId = entity.ProductId;
                order.ClientId = entity.ClientId;
                order.PurchaseQuantity = entity.PurchaseQuantity;
                order.OrderedDate = entity.OrderedDate;
                // Actualiza todas las propiedades que necesites...

                // Guardar los cambios
                await context.SaveChangesAsync();

                return new ResponseModel(true, "Order Updated!");

            }
            catch (Exception ex)
            {
                // Log Original Exception
                LogException.LogExceptions(ex);

                // Display scary-free message to client
                return new ResponseModel(false, "Error ocurred while placing order");
            }
        }
    }
}
