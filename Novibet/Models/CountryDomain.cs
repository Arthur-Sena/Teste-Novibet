namespace Novibet.Models
{
    public class CountryDomain
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TwoLetterCode { get; set; }
        public string ThreeLetterCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual ICollection<IPAddressDomain> IPAddresses { get; set; }
        public CountryDomain()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
