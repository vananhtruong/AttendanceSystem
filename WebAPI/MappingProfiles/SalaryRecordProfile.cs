using AutoMapper;
using BusinessObject.Models;
using BusinessObject.DTOs;

namespace WebAPI.MappingProfiles
{
    public class SalaryRecordProfile : Profile
    {
        public SalaryRecordProfile()
        {
            CreateMap<SalaryRecord, SalaryRecordDTO>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName));
            CreateMap<SalaryRecordDTO, SalaryRecord>();
        }
    }
} 