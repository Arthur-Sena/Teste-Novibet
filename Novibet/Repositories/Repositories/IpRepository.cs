using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Novibet.Models;
using Novibet.Models.Response;
using Novibet.Repositories.Interface;

namespace Novibet.Repositories.Repositories
{
    public class IpRepository : IIpRepository
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly CacheConfiguration _cacheConfig;
        private readonly ICountryRepository _countryRepository;

        public IpRepository(AppDbContext context, ICountryRepository countryRepository, IMemoryCache cache, IOptions<CacheConfiguration> cacheConfigOptions)
        {
            _context = context;
            _cache = cache;
            _cacheConfig = cacheConfigOptions.Value;
            _countryRepository = countryRepository;
        }

        public IPAddressDomain SearchIpAddressInDatabase(string ip)
            => _context.IPAddresses.Include(ip => ip.Country).FirstOrDefault(x => x.IP == ip);

        public void PostIpInCache(IPAddressDomain ipInfo)
        {
            _cache.Set(ipInfo.IP, ipInfo, _cacheConfig.ExpirationTime);
        }

        public async Task UpdateIpAddress(IPAddressDomain ipEntity, IPAddressResponse newInfo)
        {
            if (ipEntity.Country.Name != newInfo.CountryName ||
                ipEntity.Country.TwoLetterCode != newInfo.TwoLetterCode ||
                ipEntity.Country.ThreeLetterCode != newInfo.ThreeLetterCode)
            {
                var countryByName = await _context.Countries.FirstOrDefaultAsync(i => i.Name == newInfo.CountryName);
                if (countryByName == null)
                {
                    var newCountry = await _countryRepository.PostNewCountry(newInfo);
                    if (newCountry == null)
                        return;
                    ipEntity.CountryId = newCountry.Id;
                }
                else
                    ipEntity.CountryId = countryByName.Id;

                ipEntity.UpdatedAt = DateTime.UtcNow;
                _context.Update(ipEntity);
                await _context.SaveChangesAsync();
                _cache.Remove(ipEntity.IP);
            }
        }
    }
}