using AuthSystem.Entities;
using AuthSystem.Models;
using AutoMapper;

namespace AuthSystem.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterModel, User>();
        }
    }
}