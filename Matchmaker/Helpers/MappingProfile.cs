using Matchmaker.ViewModels;
using Models;
using Profile = AutoMapper.Profile;

namespace Matchmaker.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserViewModel>();
            CreateMap<UserViewModel, User>();
        }
    }
}
