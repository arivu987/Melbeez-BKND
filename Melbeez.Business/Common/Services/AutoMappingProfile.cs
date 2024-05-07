using AutoMapper;
using Melbeez.Business.Models;
using Melbeez.Business.Models.AccountModels;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Domain.Entities;

namespace Melbeez.Business.Common.Services
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {           
            CreateMap<CountryEntity, CountryViewModel>().ReverseMap();
            CreateMap<ProductWarrantiesEntity, ProductWarrantiesRequestModel>().ReverseMap();
        }
    }
}
