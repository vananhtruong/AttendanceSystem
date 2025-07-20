using AutoMapper;
using BusinessObject.Models;
using BusinessObject.DTOs;
using Microsoft.AspNetCore.Identity.Data;
using BusinessObject.DTOs.BusinessObject.DTOs;

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
