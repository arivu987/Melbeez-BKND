namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class CitiesResponseModel
    {
        public long Id { get; set; }
        public string CityName { get; set; }
        public long StateId { get; set; }
        public string StateName { get; set; }
        public long CountryId { get; set; }
        public string CountryName { get; set; }
    }
}
