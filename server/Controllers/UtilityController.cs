using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PodNoms.Api.Services.Processor;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    public class UtilityController : Controller {
        private readonly IUrlProcessService _processor;

        public UtilityController(IUrlProcessService processor) {
            this._processor = processor;

        }
        [HttpGet("isvalid")]
        public async Task<IActionResult> IsValid(string url) {
            var result = await _processor.IsValidUrl(url);
            if (result) {
                return Ok();
            }
            return BadRequest();
        }
    }
}