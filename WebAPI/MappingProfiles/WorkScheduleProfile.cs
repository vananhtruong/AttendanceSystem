using AutoMapper;
using BusinessObject.Models;
using BusinessObject.DTOs;

namespace WebAPI.MappingProfiles
{
    public class WorkScheduleProfile : Profile
    {
        public WorkScheduleProfile()
        {
            // Map entity -> DTO (đọc dữ liệu)
            CreateMap<WorkSchedule, WorkScheduleDTO>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.WorkShiftName, opt => opt.MapFrom(src => src.WorkShift.Name))
                .ForMember(dest => dest.IsOvertime, opt => opt.MapFrom(src => src.WorkShift.IsOvertime));

            // Map CreateDTO -> Entity
            CreateMap<WorkScheduleCreateDTO, WorkSchedule>();

            // Map UpdateDTO -> Entity, bỏ qua Id/UserId
            CreateMap<WorkScheduleUpdateDTO, WorkSchedule>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.WorkShiftId, opt => opt.Ignore());
        }
    }
} 