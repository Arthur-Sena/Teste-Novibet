using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Novibet.Application.Services;
using Novibet.Models;
using Novibet.Models.Response;
using Novibet.Repositories.Interfaces;
using System.Net;

namespace Novibet.Repositories.Repositories
{
    public class IpRepository : IIpRepository
    {
        private readonly IMemoryCache _cache;
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly CacheConfiguration _cacheConfig;
        private readonly ILogger<UpdateIpJobService> _logger;
        public IpRepository(AppDbContext context, IMemoryCache cache, HttpClient httpClient, IOptions<CacheConfiguration> cacheConfigOptions, ILogger<UpdateIpJobService> logger)
        {
            _context = context;
            _cache = cache;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cacheConfig = cacheConfigOptions.Value;
            _logger = logger;
        }

        public IPAddressResponse? GetIpAddressFromCache(string ip)
        {
            IPAddressResponse ipInfoResponse = new IPAddressResponse();

            if (_cache.TryGetValue(ip, out IPAddressDomain cachedIpInfo))
            {
                ipInfoResponse = MapToResponse(cachedIpInfo);
                return ipInfoResponse;
            }
            return null;
        }
        public IPAddressResponse? GetIpAddressFromDataBase(string ip)
        {
            var ipInfoFromDataBase = SearchIpAddressInDatabase(ip);
            if (ipInfoFromDataBase == null)
                return null;

            IPAddressResponse ipInfoResponse = MapToResponse(ipInfoFromDataBase);
            PostCacheInDataBase(ipInfoFromDataBase);
            return ipInfoResponse;
        }
        private IPAddressDomain SearchIpAddressInDatabase(string ip)
            => _context.IPAddresses.Include(ip => ip.Country).FirstOrDefault(x => x.IP == ip);
        private void PostCacheInDataBase(IPAddressDomain ipInfo)
        {
            _cache.Set(ipInfo.IP, ipInfo, _cacheConfig.ExpirationTime);
        }
        public async Task<IPAddressResponse> GetIpFromIP2C(string ipAddress)
        {   
            var requestUrl = $"https://ip2c.org/{ipAddress}";
            try
            {
                var response = await _httpClient.GetStringAsync(requestUrl);
                var ipInfo = ParseResponse(response);
                return ipInfo;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("An error occurred while fetching IP information.", ex);
            }
        }

        private static IPAddressResponse ParseResponse(string response)
        {
            var responseParts = response.Split(';');
            if (responseParts.Length >= 4 && responseParts[0] == "1") 
                return new IPAddressResponse(
                    CountryName: responseParts[3],
                    TwoLetterCode: responseParts[1],
                    ThreeLetterCode: responseParts[2]
                );
            else
                throw new Exception("Invalid response format from IP2C service.");            
        }
        public async Task PostIpInDataBase(IPAddressResponse ipAddressResponse, string ip)
        {
            CountryDomain country = _context.Countries.FirstOrDefault(x => x.Name == ipAddressResponse.CountryName);
            if (country == null)
                country = await PostNewCountry(ipAddressResponse);

            IPAddressDomain ipAddress = new IPAddressDomain();
            ipAddress.IP = ip;
            ipAddress.CountryId = country.Id;

            await _context.IPAddresses.AddAsync(ipAddress);
            await _context.SaveChangesAsync();
            PostCacheInDataBase(ipAddress);
        }
        private async Task<CountryDomain?> PostNewCountry(IPAddressResponse ipAddressResponse)
        {
            if (ipAddressResponse.CountryName.Length > 50)
            {
                _logger.LogError("An error occurred during post new Country. (Country name contains more than 50 characters)");
                return null;
            }
                        
            if (ipAddressResponse.TwoLetterCode.Length > 2)
            {
                _logger.LogError("An error occurred during post new Country. (TwoLetterCode contains more than 2 characters)");
                return null;
            }

            if (ipAddressResponse.ThreeLetterCode.Length > 2)
            {
                _logger.LogError("An error occurred during post new Country. (TwoLetterCode contains more than 3 characters)");
                return null;
            }

            CountryDomain country = new CountryDomain();
            country.Name = ipAddressResponse.CountryName;
            country.TwoLetterCode = ipAddressResponse.TwoLetterCode;
            country.ThreeLetterCode = ipAddressResponse.ThreeLetterCode;

            await _context.Countries.AddAsync(country);
            await _context.SaveChangesAsync();
            return country;
        }
        private static IPAddressResponse MapToResponse(IPAddressDomain ipDomain)
        {
            return new IPAddressResponse
            {
                CountryName = ipDomain.Country.Name,
                TwoLetterCode = ipDomain.Country.TwoLetterCode,
                ThreeLetterCode = ipDomain.Country.ThreeLetterCode
            };
        }

        public async Task<List<IPAddressDomain>> GetIpsInBatches(int batchSize)
        {
            return await _context.IPAddresses
                .Include(ip => ip.Country)
                .OrderBy(x => x.UpdatedAt)
                .Take(batchSize)
                .ToListAsync();
        }

        public async Task UpdateIpAddress(string ip, IPAddressResponse newInfo)
        {
            var ipEntity = await _context.IPAddresses
                .Include(i => i.Country)
                .FirstOrDefaultAsync(i => i.IP == ip);

            if (ipEntity == null) 
                return;

            if (ipEntity.Country.Name != newInfo.CountryName ||
                ipEntity.Country.TwoLetterCode != newInfo.TwoLetterCode ||
                ipEntity.Country.ThreeLetterCode != newInfo.ThreeLetterCode)
            {
                var countryByName = await _context.Countries.FirstOrDefaultAsync(i => i.Name == newInfo.CountryName);
                if (countryByName == null)
                {
                    var newCountry = await PostNewCountry(newInfo);
                    if (newCountry == null)
                        return;
                    ipEntity.CountryId = newCountry.Id;
                }
                else
                    ipEntity.CountryId = countryByName.Id;

                ipEntity.UpdatedAt = DateTime.UtcNow;
                _context.Update(ipEntity);
                await _context.SaveChangesAsync();
                _cache.Remove(ip);
            }
        }
    }
}