using Casestudy.DAL;
using Casestudy.DAL.DAO;
using Casestudy.DAL.DomainClasses;
using Microsoft.AspNetCore.Mvc;
using Casestudy.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace Casestudy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        readonly AppDbContext _ctx;
        public OrderController(AppDbContext context)
        {
            _ctx = context;
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult<string>> Index(CartHelper helper)
        {
            string retVal;

            try
            {
                CustomerDAO cDao = new(_ctx);
                OrderDAO oDao = new OrderDAO(_ctx);
                //ProductDAO pDao = new ProductDAO(_ctx);

                Customer? cartOwner = await cDao.GetByEmail(helper.Email);
                int orderId = await oDao.AddOrder(cartOwner!.Id, helper.Selections!);

                retVal = orderId > 0
                    ? "Order " + orderId + " saved!"
                    : "Order not saved";
            }
            catch (Exception ex)
            {
                retVal = "Order not saved " + ex.Message;
            }

            return retVal;
        }


        [Route("{email}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<Order>>> List(string email)
        {
            List<Order> trays; ;
            CustomerDAO uDao = new(_ctx);
            Customer? trayOwner = await uDao.GetByEmail(email);
            OrderDAO tDao = new(_ctx);
            trays = await tDao.GetAll(trayOwner!.Id);
            return trays;
        }

        [Route("{orderid}/{email}")]
        [HttpGet]
        public async Task<ActionResult<List<OrderDetailsHelper>>> GetTrayDetails(int orderid, string email)
        {
            OrderDAO dao = new(_ctx);
            return await dao.GetOrderDetails(orderid, email);
        }
    }
}
