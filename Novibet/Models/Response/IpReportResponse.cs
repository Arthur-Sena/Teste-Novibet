namespace Novibet.Models.Response
{
    public class IpReportResponse
    {
        public string CountryName { get; set; }
        public int AddressesCount { get; set; }
        public DateTime? LastAddressUpdated { get; set; }
    }
}