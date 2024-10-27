using Microsoft.EntityFrameworkCore;
using Novibet.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<CountryDomain> Countries { get; set; }
    public DbSet<IPAddressDomain> IPAddresses { get; set; }
}