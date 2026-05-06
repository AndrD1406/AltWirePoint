using AltWirePoint.BusinessLogic.Models;
using AltWirePoint.BusinessLogic.Models.Identity;
using AltWirePoint.BusinessLogic.Models.Profile;
using AltWirePoint.BusinessLogic.Models.Publication;
using AltWirePoint.DataAccess.Identity;
using AltWirePoint.DataAccess.Models;
using AutoMapper;

namespace AltWirePoint.BusinessLogic.Util;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterRequest, ApplicationUser>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

        CreateMap<PublicationCreateRequest, Publication>()
            .ForMember(dest => dest.ParentId, opt => opt.Ignore());

        CreateMap<Publication, PublicationDto>()
            .ForMember(dest => dest.FileUrls, opt => opt.MapFrom(src => src.CloudStoredFiles != null ? src.CloudStoredFiles.Select(f => f.Url) : null))
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author!.Name))
            .ForMember(dest => dest.AuthorLogo, opt => opt.MapFrom(src => src.Author!.Logo));

        CreateMap<Publication, CommentDto>()
            .ForMember(d => d.ParentId, opt => opt.MapFrom(p => p.ParentId!.Value))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(p => p.CreatedAt))
            .ForMember(d => d.AuthorName, opt => opt.MapFrom(p => p.Author!.Name))
            .ForMember(d => d.AuthorLogo, opt => opt.MapFrom(p => p.Author!.Logo))
            .ForMember(d => d.FileUrls, opt => opt.MapFrom(p => p.CloudStoredFiles != null ? p.CloudStoredFiles.Select(f => f.Url) : null));

        CreateMap<Like, LikeDto>()
            .ForMember(dest => dest.PublicationId,
                       opt => opt.MapFrom(src => src.PublicationId))
            .ForMember(dest => dest.AuthorName,
                       opt => opt.MapFrom(src => src.Author!.Name));

        CreateMap<ApplicationUser, ProfileDto>()
            .ForMember(d => d.UserId, o => o.MapFrom(u => u.Id))
            .ForMember(d => d.Name, o => o.MapFrom(u => u.Name))
            .ForMember(d => d.Logo, o => o.MapFrom(u => u.Logo))
            .ForMember(d => d.PublicationIds, o => o.MapFrom(u =>
                u.Publications != null
                    ? u.Publications.Select(p => p.Id).ToList()
                    : new List<Guid>()));

        CreateMap<CommentCreateRequest, Publication>()
            .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => src.AuthorId))
            .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.PublicationId))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Content))

            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}
