using Novibet.Models;
using Novibet.Models.Response;

namespace Novibet.Repositories.Interfaces
{
    public interface IIpService
    {
        IPAddressResponse? GetIpAddressFromCache(string ip);
        IPAddressResponse? GetIpAddressFromDataBase(string ip);
        Task<IPAddressResponse> GetIpFromIP2C(string ip);
        Task PostIpInDataBase(IPAddressResponse ipAddress, string ip);
        Task<List<IPAddressDomain>> GetIpsInBatches(int batchSize);
        Task UpdateIpAddress(string ip, IPAddressResponse newInfo);
    }
}