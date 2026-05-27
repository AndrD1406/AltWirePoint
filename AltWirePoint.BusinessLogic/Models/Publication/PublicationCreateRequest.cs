using AltWirePoint.DataAccess.Models;

namespace AltWirePoint.BusinessLogic.Models.Publication;

public class PublicationCreateRequest
{
    public string? Description { get; set; }
}

public static class PublicationCreateRequestExtensions
{
    public static DataAccess.Models.Publication ToPublication(this PublicationCreateRequest request)
    {
        return new DataAccess.Models.Publication
        {
            Description = request.Description
        };
    }

    public static void ApplyTo(this PublicationCreateRequest request, DataAccess.Models.Publication publication)
    {
        publication.Description = request.Description;
    }
}
