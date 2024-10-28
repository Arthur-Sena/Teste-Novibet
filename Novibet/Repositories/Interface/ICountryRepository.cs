using Novibet.Models.Response;
using Novibet.Models;

namespace Novibet.Repositories.Interface
{
    public interface ICountryRepository
    {
        Task<CountryDomain?> PostNewCountry(IPAddressResponse ipAddressResponse);
    }
}
