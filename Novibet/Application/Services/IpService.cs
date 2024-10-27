using Microsoft.EntityFrameworkCore;
using Novibet.Application.Interfaces;
using Novibet.Models.Response;
using Novibet.Repositories.Interfaces;

namespace Novibet.Application.Services
{
    public class IpService : IIpService
    {
        private readonly IIpRepository _ipRepository;
        private readonly AppDbContext _context;

        public IpService(AppDbContext context, IIpRepository ipRepository)
        {
            _ipRepository = ipRepository;
            _context = context;
        }

        public async Task<IPAddressResponse?> GetIpAddress(string ip)
        {
            var ipFromCache = _ipRepository.GetIpAddressFromCache(ip);
            if (ipFromCache != null)
                return ipFromCache;

            var ipFromDB = _ipRepository.GetIpAddressFromDataBase(ip);
            if (ipFromDB != null)
                return ipFromDB;

            var ipFromIP2C = await _ipRepository.GetIpFromIP2C(ip);
            if (ipFromIP2C != null)
                await _ipRepository.PostIpInDataBase(ipFromIP2C, ip);
            return ipFromIP2C;            
        }

        public async Task<IEnumerable<IpReportResponse>> GetIpReport(string[]? countryCodes)
        {
            var query = @"
                SELECT 
                    c.Name AS CountryName, 
                    COUNT(ip.IP) AS AddressesCount, 
                    MAX(ip.UpdatedAt) AS LastAddressUpdated
                FROM 
                    Countries c
                LEFT JOIN 
                    IPAddresses ip ON ip.CountryId = c.Id
                {0}
                GROUP BY 
                    c.Name
                ORDER BY 
                c.Name
            ";

            string condition = string.Empty;
            var parameters = new List<object>();

            if (countryCodes != null && countryCodes.Length > 0)
            {
                condition = "WHERE c.TwoLetterCode IN (";
                condition += string.Join(", ", countryCodes.Select((code, index) => $"@p{index}"));
                condition += ")";

                parameters.AddRange(countryCodes);
            }

            var finalQuery = string.Format(query, condition);

            return await _context.Database
                .SqlQueryRaw<IpReportResponse>(finalQuery, parameters.ToArray())
                .ToListAsync();
        }
    }
}