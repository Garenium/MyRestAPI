using Casestudy.DAL.DomainClasses;
using Casestudy.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Casestudy.DAL.DAO
{
    public class OrderDAO
    {
        private readonly AppDbContext _db;
        public OrderDAO(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Order>> GetAll(int id)
        {
            return await _db.Orders!.Where(tray => tray.CustomerId == id).ToListAsync<Order>();
        }

        public async Task<List<OrderDetailsHelper>> GetOrderDetails(int tid, string email)
        {
            Customer? customer = _db.Customers!.FirstOrDefault(user => user.Email == email);
            List<OrderDetailsHelper> allDetails = new();
            // LINQ way of doing INNER JOINS
            var results = from o in _db.Orders
                          join oi in _db.OrderLineItems! on o.Id equals oi.OrderId
                          join p in _db.Products! on oi.ProductId equals p.Id
                          where (o.CustomerId == customer!.Id && o.Id == tid)
                          select new OrderDetailsHelper
                          {
                              OrderId = o.Id,
                              OrderDate = o.OrderDate,
                              OrderAmount = o.OrderAmount,
                              CustomerId = customer!.Id,
                              QtyOrdered = oi.QtyOrdered,
                              QtySold = oi.QtySold,
                              QtyBackOrdered = oi.QtyBackOrdered,
                              SellingPrice = oi.SellingPrice,
                          };
            allDetails = await results.ToListAsync();
            return allDetails;
        }

        public async Task<int> AddOrder(int customerid, CartSelectionHelper[] selections)
        {
            int orderId = -1;
            //we need a transaction as multiple entities involved
            using (var _trans = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    Order order = new();
                    order.OrderDate = System.DateTime.Now;
                    order.OrderAmount = 0.0M;
                    order.CustomerId = customerid;

                    //updating the order table 
                    foreach(CartSelectionHelper selection in selections)
                    {
                        
                        //Total order price (all products)
                        order.OrderAmount += (selection.Product!.MSRP * selection.Qty);
                    }
                    await _db.Orders!.AddAsync(order);
                    await _db.SaveChangesAsync();


                    //updating the orderLineitem table
                    foreach (CartSelectionHelper selection in selections)
                    {

                        OrderLineItem oItem = new();
                        oItem.QtyOrdered = selection.Qty;
                        oItem.ProductId = selection.Product!.Id;
                        oItem.OrderId = order.Id;
                        oItem.SellingPrice = selection.Product!.MSRP;

                        //????
                        ////enough stock
                        //if (oItem.QtyOrdered > selection.Product!.QtyOnHand)
                        //{
                            

                        //}//not enough stock
                        //else if (oItem.QtyOrdered < selection.Product!.QtyOnHand)
                        //{
                            
                        //}
                        await _db.OrderLineItems!.AddAsync(oItem);
                        await _db.SaveChangesAsync();
                    }

                    await _trans.CommitAsync();
                    orderId = order.Id;

                    //updating the products table
                    foreach (CartSelectionHelper selection in selections)
                    {
                        OrderLineItem oItem = new();
                        Product product = new();
                        product.Id = selection.Product!.Id;
                        oItem.QtyOrdered = selection.Qty;
                        product.Description = selection.Product!.Description;
                        product.GraphicName = selection.Product!.GraphicName;

                        product.BrandId = selection.Product!.BrandId;
                        product.Timer = selection.Product!.Timer;
                        product.ProductName = selection.Product!.ProductName;
                        product.GraphicName = selection.Product!.GraphicName;
                        product.CostPrice = selection.Product!.CostPrice;
                        product.MSRP = selection.Product!.MSRP;

                        //enough stock
                        if (oItem.QtyOrdered > selection.Product!.QtyOnHand)
                        {
                            product.QtyOnHand = 0;
                            product.QtyOnBackOrder += (oItem.QtyOrdered - selection.Product!.QtyOnHand);
                            oItem.QtySold = product.QtyOnHand;
                            oItem.QtyOrdered = selection.Qty;
                            oItem.QtyBackOrdered = selection.Qty - selection.Product!.QtyOnHand;

                        }//not enough stock
                        else if(oItem.QtyOrdered < selection.Product!.QtyOnHand)
                        {
                            product.QtyOnHand -= selection.Qty;
                            oItem.QtySold = selection.Qty;
                            oItem.QtyBackOrdered = 0;

                        }
                        _db.Products!.Update(product);
                        await _db.SaveChangesAsync();
                    }
                    
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await _trans.RollbackAsync();
                }
            }
            return orderId;
        }
    }
}