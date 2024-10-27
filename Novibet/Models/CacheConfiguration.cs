namespace Novibet.Models
{
    public class CacheConfiguration
    {
        public TimeSpan ExpirationTime { get; set; } = TimeSpan.FromDays(3);
    }
}