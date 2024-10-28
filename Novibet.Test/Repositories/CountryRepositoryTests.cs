using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Novibet.Models.Response;
using Novibet.Models;
using Novibet.Repositories.Repositories;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class CountryRepositoryTests
{
    private readonly Mock<ILogger<UpdateIpJobService>> _loggerMock;
    private readonly AppDbContext _context;

    public CountryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new AppDbContext(options);
        _loggerMock = new Mock<ILogger<UpdateIpJobService>>();
    }

    [Fact]
    public async Task PostNewCountry_ShouldReturnNull_WhenCountryNameIsTooLong()
    {
        // Arrange
        var repository = new CountryRepository(_context, _loggerMock.Object);
        var ipAddressResponse = new IPAddressResponse
        {
            CountryName = new string('A', 51), // CountryName should has less than 51 characters
            TwoLetterCode = "US",
            ThreeLetterCode = "USA"
        };

        // Act
        var result = await repository.PostNewCountry(ipAddressResponse);

        // Assert
        Assert.Null(result);
        _loggerMock.Verify(logger =>
            logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((obj, t) => obj.ToString().Contains("Country name contains more than 50 characters")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task PostNewCountry_ShouldReturnNull_WhenTwoLetterCodeIsTooLong()
    {
        // Arrange
        var repository = new CountryRepository(_context, _loggerMock.Object);
        var ipAddressResponse = new IPAddressResponse
        {
            CountryName = "United States",
            TwoLetterCode = "USA", // TwoLetterCode should has less than 3 characters
            ThreeLetterCode = "USA"
        };

        // Act
        var result = await repository.PostNewCountry(ipAddressResponse);

        // Assert
        Assert.Null(result);
        _loggerMock.Verify(logger =>
            logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((obj, t) => obj.ToString().Contains("TwoLetterCode contains more than 2 characters")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task PostNewCountry_ShouldReturnNull_WhenThreeLetterCodeIsTooLong()
    {
        // Arrange
        var repository = new CountryRepository(_context, _loggerMock.Object);
        var ipAddressResponse = new IPAddressResponse
        {
            CountryName = "United States",
            TwoLetterCode = "US",
            ThreeLetterCode = "USAA" // ThreeLetterCode should has less than 3 characters
        };

        // Act
        var result = await repository.PostNewCountry(ipAddressResponse);

        // Assert
        Assert.Null(result);
        _loggerMock.Verify(logger =>
            logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((obj, t) => obj.ToString().Contains("ThreeLetterCode contains more than 3 characters")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task PostNewCountry_ShouldAddCountry_WhenDataIsValid()
    {
        // Arrange
        var repository = new CountryRepository(_context, _loggerMock.Object);
        var ipAddressResponse = new IPAddressResponse //Everything is OK
        {
            CountryName = "United States",
            TwoLetterCode = "US",
            ThreeLetterCode = "USA"
        };

        // Act
        var result = await repository.PostNewCountry(ipAddressResponse);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("United States", result.Name);
        Assert.Equal("US", result.TwoLetterCode);
        Assert.Equal("USA", result.ThreeLetterCode);

        var countries = await _context.Countries.ToListAsync();
        Assert.Single(countries);
        Assert.Equal("United States", countries[0].Name);
        Assert.Equal("US", countries[0].TwoLetterCode);
        Assert.Equal("USA", countries[0].ThreeLetterCode);
    }
}