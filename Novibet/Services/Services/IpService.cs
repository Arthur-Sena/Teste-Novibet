using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Novibet.Application.Services;
using Novibet.Models;
using Novibet.Models.Response;
using Novibet.Repositories.Interface;
using Novibet.Repositories.Interfaces;
using System.Net;

namespace Novibet.Repositories.Repositories
{
    public class IpService : IIpService
    {
        private readonly IMemoryCache _cache;
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly CacheConfiguration _cacheConfig;
        private readonly ILogger<UpdateIpJobService> _logger;
        private readonly IIpRepository _ipRepository;
        private readonly ICountryRepository _countryRepository;

        public IpService(AppDbContext context, IIpRepository ipRepository, ICountryRepository countryRepository, IMemoryCache cache, HttpClient httpClient, ILogger<UpdateIpJobService> logger)
        {
            _context = context;
            _ipRepository = ipRepository;
            _countryRepository = countryRepository;
            _cache = cache;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger;
        }

        public IPAddressResponse? GetIpAddressFromCache(string ip)
        {
            if (_cache.TryGetValue(ip, out IPAddressDomain cachedIpInfo))
            {
                IPAddressResponse ipInfoResponse = MapToResponse(cachedIpInfo);
                return ipInfoResponse;
            }
            return null;
        }

        public IPAddressResponse? GetIpAddressFromDataBase(string ip)
        {
            var ipInfoFromDataBase = _ipRepository.SearchIpAddressInDatabase(ip);
            if (ipInfoFromDataBase == null)
                return null;

            IPAddressResponse ipInfoResponse = MapToResponse(ipInfoFromDataBase);
            _ipRepository.PostIpInCache(ipInfoFromDataBase);
            return ipInfoResponse;
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
                country = await _countryRepository.PostNewCountry(ipAddressResponse);

            IPAddressDomain ipAddress = new IPAddressDomain();
            ipAddress.IP = ip;
            ipAddress.CountryId = country.Id;

            await _context.IPAddresses.AddAsync(ipAddress);
            await _context.SaveChangesAsync();
            _ipRepository.PostIpInCache(ipAddress);
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

            await _ipRepository.UpdateIpAddress(ipEntity, newInfo);            
        }
    }
}