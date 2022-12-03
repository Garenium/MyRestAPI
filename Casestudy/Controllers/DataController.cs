using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Casestudy.DAL;
using System.Text.Json;

namespace Casestudy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        //[HttpGet]
        //public async Task<IActionResult> Index()
        //{
        //    var json = await GetMenuItemJsonFromWebAsync();
        //    return Content(json);
        //}

        private async Task<string> GetProductJsonFromWebAsync()
        {
            string url = "https://raw.githubusercontent.com/garenium/casestudyjson/main/data.json";
            var httpclient = new HttpClient();
            var response = await httpclient.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        readonly AppDbContext? _ctx;
        public DataController(AppDbContext context) // injected here
        {
            _ctx = context;
        }


        [HttpGet]
        public async Task<ActionResult<String>> Index()
        {
            DataUtility util = new(_ctx!);
            string payload = "";
            var json = await GetProductJsonFromWebAsync();
            try
            {
                payload = (await util.LoadProductInfoFromWebToDb(json)) ? "tables loaded" : "problem loading tables";
            }
            catch (Exception ex)
            {
                payload = ex.Message;
            }

            return JsonSerializer.Serialize(payload);
        }

    }
}
