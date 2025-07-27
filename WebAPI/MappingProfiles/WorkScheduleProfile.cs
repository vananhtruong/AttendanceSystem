using AutoMapper;
using BusinessObject.Models;
using BusinessObject.DTOs;

namespace WebAPI.MappingProfiles
{
    public class WorkScheduleProfile : Profile
    {
        public WorkScheduleProfile()
        {
            CreateMap<WorkSchedule, WorkScheduleDTO>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.WorkShiftName, opt => opt.MapFrom(src => src.WorkShift.Name))
                .ForMember(dest => dest.IsOvertime, opt => opt.MapFrom(src => src.WorkShift.IsOvertime));
            CreateMap<WorkScheduleDTO, WorkSchedule>();
        }
    }
} 