namespace BlockedCountriesWepApi.Models.Entities
{
    public class Country
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime? TemporalBlockExpiry { get; set; }
    }
}
