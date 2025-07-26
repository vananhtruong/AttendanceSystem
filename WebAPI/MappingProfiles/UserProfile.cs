using AutoMapper;
using BusinessObject.Models;
using BusinessObject.DTOs;

namespace WebAPI.MappingProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDTO>();
            CreateMap<BusinessObject.DTOs.RegisterRequest, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        }
    }
}
