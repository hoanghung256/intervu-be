using AutoMapper;
using Intervu.Application.DTOs.User;
using Intervu.Domain.Entities;

namespace Intervu.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, LoginResponse>()
                .ForMember(dest => dest.Token, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiresIn, opt => opt.Ignore());

            CreateMap<LoginRequest, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            // Register mappings
            CreateMap<RegisterRequest, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore()) // Will be hashed separately
                .ForMember(dest => dest.Role, opt => opt.Ignore()) // Will be parsed from string
                .ForMember(dest => dest.ProfilePicture, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());
        }
    }
}
