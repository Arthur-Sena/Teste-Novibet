using Microsoft.AspNetCore.Http.HttpResults;

namespace Novibet.Models.Response
{
    public class IPAddressResponse
    {
        public string CountryName { get; set; }
        public string TwoLetterCode { get; set; }
        public string ThreeLetterCode { get; set; }
        public IPAddressResponse() { }
        public IPAddressResponse(string CountryName, string TwoLetterCode, string ThreeLetterCode)
        {
            this.CountryName = CountryName;
            this.TwoLetterCode = TwoLetterCode;
            this.ThreeLetterCode = ThreeLetterCode;
        }
    }
}