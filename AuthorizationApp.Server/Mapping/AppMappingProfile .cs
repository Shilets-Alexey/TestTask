using AuthorizationApp.Server.Models;
using AutoMapper;
using System;

namespace AuthorizationApp.Server.Mapping
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            CreateMap<ApplicationUser, User>()
                .ForMember("LastLoginDate", opt => opt.MapFrom(appUser => appUser.LastLoginDate == default ? "" : appUser.LastLoginDate.ToString()));
        }
    }
}
