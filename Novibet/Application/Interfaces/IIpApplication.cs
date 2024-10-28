using Novibet.Models.Response;

namespace Novibet.Application.Interfaces
{
    public interface IIpApplication
    {
        Task<IPAddressResponse?> GetIpAddress(string ip);
        Task<IEnumerable<IpReportResponse>> GetIpReport(string[]? countryCodes);
    }
}