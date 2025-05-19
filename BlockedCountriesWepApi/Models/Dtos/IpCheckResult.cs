namespace BlockedCountriesWepApi.Models.Dtos
{
    public class IpCheckResult
    {
        public string IpAddress { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public bool IsBlocked { get; set; }
    }
}
