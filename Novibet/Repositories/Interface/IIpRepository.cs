using Novibet.Models;
using Novibet.Models.Response;

namespace Novibet.Repositories.Interface
{
    public interface IIpRepository
    {
        IPAddressDomain SearchIpAddressInDatabase(string ip);
        void PostIpInCache(IPAddressDomain ipInfo);
        Task UpdateIpAddress(IPAddressDomain ipEntity, IPAddressResponse newInfo);
    }
}
