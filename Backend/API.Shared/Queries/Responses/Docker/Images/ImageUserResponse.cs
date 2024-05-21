namespace API.Shared.Queries.Responses.Docker.Images;

public class ImageUserResponse
{
    public required string DisplayName;
    public required string Description;

    public required string CodeFileExtension;
    public required string CodeEditorLang;

    public required int MaxCountByUser;
}