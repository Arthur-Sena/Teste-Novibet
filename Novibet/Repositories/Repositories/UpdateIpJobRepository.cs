using Novibet.Models.Response;
using Novibet.Models;
using Novibet.Repositories.Interfaces;

namespace Novibet.Repositories.Repositories
{
    public class UpdateIpJobRepository : IUpdateIpJobRepository
    {
        private readonly IIpRepository _ipRepository;
        public UpdateIpJobRepository(IIpRepository ipRepository)
        {
            _ipRepository = ipRepository;
        }
        public async Task UpdateIpInformation()
        {
            int batchSize = 100;
            var ipList = await _ipRepository.GetIpsInBatches(batchSize);
            
            foreach (var ip in ipList)
            {
                var latestInfo = await _ipRepository.GetIpFromIP2C(ip.IP);
            
                if (latestInfo != null && HasIpInfoChanged(ip, latestInfo))
                {
                    await _ipRepository.UpdateIpAddress(ip.IP, latestInfo);
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