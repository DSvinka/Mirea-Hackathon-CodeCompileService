using Services.Docker.Shared.Enums;

namespace API.Shared.Queries.Responses.Docker.Containers;

public class DockerContainerAdminResponse: DockerContainerUserResponse
{
    public required string ProgramCodeFolder;
}