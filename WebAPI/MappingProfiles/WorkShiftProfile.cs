using AutoMapper;
using BusinessObject.DTOs;
using BusinessObject.Models;

namespace WebAPI.MappingProfiles
{
    public class WorkShiftProfile : Profile
    {
        public WorkShiftProfile()
        {
            CreateMap<WorkShift, WorkShiftDTO>().ReverseMap();
            CreateMap<WorkShiftCreateDTO, WorkShift>();
            CreateMap<WorkShiftUpdateDTO, WorkShift>();
        }
    }
}
