namespace Novibet.Models
{
    public class IPAddressDomain
    {
        public int Id { get; set; }
        public int CountryId { get; set; }
        public virtual CountryDomain Country { get; set; }
        public string IP { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IPAddressDomain()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}