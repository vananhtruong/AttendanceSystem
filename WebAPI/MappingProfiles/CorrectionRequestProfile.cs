using AutoMapper;
using BusinessObject.Models;
using BusinessObject.DTOs;

namespace WebAPI.MappingProfiles
{
    public class CorrectionRequestProfile : Profile
    {
        public CorrectionRequestProfile()
        {
            CreateMap<CorrectionRequest, CorrectionRequestDTO>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName));
            CreateMap<CorrectionRequestDTO, CorrectionRequest>();
        }
    }
} 