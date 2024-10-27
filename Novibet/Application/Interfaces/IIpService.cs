using Novibet.Models.Response;
using System.Runtime;

namespace Novibet.Application.Interfaces
{
    public interface IIpService
    {
        Task<IPAddressResponse?> GetIpAddress(string ip);
        Task<IEnumerable<IpReportResponse>> GetIpReport(string[]? countryCodes);
    }
}