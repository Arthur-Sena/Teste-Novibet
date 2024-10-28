using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Novibet.Models;
using Novibet.Models.Response;
using Novibet.Repositories.Repositories;
using Novibet.Repositories.Interface;

public class IpRepositoryTests
{
    private readonly Mock<ICountryRepository> _countryRepositoryMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly AppDbContext _context;
    private readonly CacheConfiguration _cacheConfig;
    private readonly Dictionary<object, object> _cachedData = new Dictionary<object, object>();

    public IpRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new AppDbContext(options);
        _cacheMock = new Mock<IMemoryCache>();
        _cacheConfig = new CacheConfiguration { ExpirationTime = TimeSpan.FromMinutes(5) };
        _countryRepositoryMock = new Mock<ICountryRepository>();
    }

    [Fact]
    public void SearchIpAddressInDatabase_ShouldReturnNull_WhenIpDoesNotExist()
    {
        // Arrange
        var repository = new IpRepository(_context, _countryRepositoryMock.Object, _cacheMock.Object, Options.Create(_cacheConfig));

        // Act
        var result = repository.SearchIpAddressInDatabase("192.168.0.1");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchIpAddressInDatabase_ShouldReturnIpAddress_WhenIpExists()
    {
        // Arrange
        var repository = new IpRepository(_context, _countryRepositoryMock.Object, _cacheMock.Object, Options.Create(_cacheConfig));

        var country = new CountryDomain
        {
            Id = 1,
            Name = "Test Country",
            TwoLetterCode = "TC",
            ThreeLetterCode = "TST"
        };

        var ipAddress = new IPAddressDomain
        {
            IP = "192.168.0.1",
            CountryId = country.Id,
            Country = country
        };

        _context.Countries.Add(country);
        _context.IPAddresses.Add(ipAddress);
        await _context.SaveChangesAsync();

        // Act
        var result = repository.SearchIpAddressInDatabase(ipAddress.IP);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ipAddress.IP, result.IP);
        Assert.Equal(country.Name, result.Country.Name);
    }

    [Fact]
    public async Task UpdateIpAddress_ShouldNotUpdate_WhenCountryIsTheSame()
    {
        // Arrange
        var existingCountry = new CountryDomain { Id = 1, Name = "United States", TwoLetterCode = "US", ThreeLetterCode = "USA" };
        _context.Countries.Add(existingCountry);
        var ipAddress = new IPAddressDomain { IP = "192.168.0.1", Country = existingCountry, CountryId = existingCountry.Id };
        _context.IPAddresses.Add(ipAddress);
        await _context.SaveChangesAsync();

        var newInfo = new IPAddressResponse { CountryName = "United States", TwoLetterCode = "US", ThreeLetterCode = "USA" };

        var repository = new IpRepository(_context, _countryRepositoryMock.Object, _cacheMock.Object, Options.Create(_cacheConfig));

        // Act
        await repository.UpdateIpAddress(ipAddress, newInfo);

        // Assert
        var updatedIpAddress = await _context.IPAddresses.Include(ip => ip.Country).FirstOrDefaultAsync(ip => ip.IP == ipAddress.IP);
        Assert.NotNull(updatedIpAddress);
        Assert.Equal(existingCountry.Id, updatedIpAddress.CountryId);
    }
}