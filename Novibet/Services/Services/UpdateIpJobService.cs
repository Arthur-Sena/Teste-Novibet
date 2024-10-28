using Novibet.Models.Response;
using Novibet.Models;
using Novibet.Repositories.Interfaces;

namespace Novibet.Repositories.Repositories
{
    public class UpdateIpJobService : IUpdateIpJobService
    {
        private readonly IIpService _ipService;
        public UpdateIpJobService(IIpService ipService)
        {
            _ipService = ipService;
        }
        public async Task UpdateIpInformation()
        {
            int batchSize = 100;
            var ipList = await _ipService.GetIpsInBatches(batchSize);
            
            foreach (var ip in ipList)
            {
                var latestInfo = await _ipService.GetIpFromIP2C(ip.IP);
            
                if (latestInfo != null && HasIpInfoChanged(ip, latestInfo))
                {
                    await _ipService.UpdateIpAddress(ip.IP, latestInfo);
                }
            }
        }

        private bool HasIpInfoChanged(IPAddressDomain existingIp, IPAddressResponse latestInfo)
        {
            return existingIp.Country.Name != latestInfo.CountryName ||
                   existingIp.Country.TwoLetterCode != latestInfo.TwoLetterCode ||
                   existingIp.Country.ThreeLetterCode != latestInfo.ThreeLetterCode;
        }
    }
}