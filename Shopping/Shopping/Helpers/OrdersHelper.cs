using Microsoft.EntityFrameworkCore;
using Shopping.Common;
using Shopping.Data;
using Shopping.Data.Entities;
using Shopping.Enums;
using Shopping.Models;

namespace Shopping.Helpers
{
    public class OrdersHelper : IOrdersHelper
    {
        private readonly DataContext _context;

        public OrdersHelper(DataContext context)
        {
            _context = context;
        }

        public async Task<Response> ProcessOrderAsync(ShowCartViewModel model)
        {
            Response response = await CheckInventoryAsync(model);
            if (!response.IsSuccess)
            {
                return response;
            }

            Sale sale = new()
            {
                Date = DateTime.UtcNow,
                User = model.User,
                Remarks = model.Remarks,
                SaleDetails = new List<SaleDetail>(),
                OrderStatus = OrderStatus.Nuevo
            };

            foreach(TemporalSale temporalSale in model.TemporalSales)
            {
                sale.SaleDetails.Add(new SaleDetail
                {
                    Product = temporalSale.Product,
                    Quantity = temporalSale.Quantity,
                    Remarks = temporalSale.Remarks
                });

                Product product = await _context.Products.FindAsync(temporalSale.Product.Id);
                if(product != null)
                {
                    product.Stock -= temporalSale.Quantity;
                    _context.Products.Update(product);
                }

                _context.TemporalSales.Remove(temporalSale);
            }
            
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
            return response;
        }

        private async Task<Response> CheckInventoryAsync(ShowCartViewModel model)
        {
            Response response = new()
            {
                IsSuccess = true,
            };

            foreach (TemporalSale temporalSale in model.TemporalSales)
            {
                Product product = await _context.Products.FindAsync(temporalSale.Product.Id);
                if (product == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"El producto {temporalSale.Product.Name} no está disponible";
                    return response;
                }

                if (product.Stock < temporalSale.Quantity)
                {
                    response.IsSuccess = false;
                    response.Message = $"Lo sentimos, no tenemos existencias suficientes para el producto {temporalSale.Product.Name}," +
                        $" Por favor disminuir la cantidad o sustituirlo por otro.";
                    return response;
                }
            }
            return response;
        }

        public async Task<Response> CancelOrderAsync(int id)
        {
            Sale sale = await _context.Sales
                .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
                .FirstOrDefaultAsync(s => s.Id == id);

            foreach (SaleDetail saleDetail in sale.SaleDetails)
            {
                Product product = await _context.Products.FindAsync(saleDetail.Product.Id);
                if (product != null)
                {
                    product.Stock += saleDetail.Quantity;
                }
            }

            sale.OrderStatus = OrderStatus.Cancelado;
            await _context.SaveChangesAsync();
            return new Response { IsSuccess = true };
        }
    }
}
