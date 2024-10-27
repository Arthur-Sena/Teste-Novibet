using Microsoft.AspNetCore.Mvc;
using Novibet.Application.Interfaces;
using Novibet.Models.Response;
using System.Text.RegularExpressions;

namespace Novibet.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IpController : ControllerBase
    {
        private readonly IIpService _ipService;
        private readonly string _ipv4Pattern = @"^(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[0-1]?[0-9][0-9]?)$";

        public IpController(IIpService ipService)
        {
            _ipService = ipService;
        }

        [HttpGet("{ip}")]
        public async Task<ActionResult<IPAddressResponse>> GetIpAddress(string ip)
        {
            if (!Regex.IsMatch(ip, _ipv4Pattern))
                return BadRequest("The IP address is invalid.");

            var ipInfo = await _ipService.GetIpAddress(ip);
            if (ipInfo == null)
                return NotFound(); 
            
            return Ok(ipInfo); 
        }

        [HttpGet("report")]
        public async Task<ActionResult<IEnumerable<IpReportResponse>>> GetReport([FromQuery] string[]? countryCodes)
        {
            var report = await _ipService.GetIpReport(countryCodes);
            return Ok(report);
        }
    }
}