using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Novibet.Models;
using Novibet.Models.Response;
using Novibet.Repositories.Interface;

namespace Novibet.Repositories.Repositories
{
    public class CountryRepository : ICountryRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UpdateIpJobService> _logger;

        public CountryRepository(AppDbContext context, ILogger<UpdateIpJobService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CountryDomain?> PostNewCountry(IPAddressResponse ipAddressResponse)
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

            if (ipAddressResponse.ThreeLetterCode.Length > 3)
            {
                _logger.LogError("An error occurred during post new Country. (ThreeLetterCode contains more than 3 characters)");
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
    }
}