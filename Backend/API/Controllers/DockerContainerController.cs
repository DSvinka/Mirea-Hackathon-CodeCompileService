using System.Text.Json;
using API.Auth.Services;
using API.Shared.Queries.Requests.Docker.Containers;
using API.Shared.Queries.Requests.Docker.Images;
using API.Shared.Queries.Responses;
using API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Docker.Shared;
using Services.Docker.Shared.Messages.Requests.Containers;
using Services.Docker.Shared.Messages.Requests.Images;
using Shared.Utils.Redis.Services;
using StackExchange.Redis;

namespace API.Controllers;

[Route("api/services/docker/containers")]
[ApiController]
public class DockerContainerController: ControllerBase
{
    private readonly JwtService _jwtService;

    private readonly ISubscriber _publisher;

    public DockerContainerController(RedisService redisService, JwtService jwtService)
    {
        _jwtService = jwtService;

        _publisher = redisService.Connection.GetSubscriber();
    }


    /// <summary>
    /// Создает и запускает Docker контейнер.
    /// </summary>
    /// <param name="request">Запрос</param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] DockerContainerCreateRequest request)
    {
        var user = await _jwtService.GetUserAsync(User);
        if (user == null)
            return Forbid();

        if (request.ProgramCode.Length >= 10240)
            return BadRequest(new ErrorResponse
            {
                Message = "Program code is to long!",
            }.WithCode("services.docker", ErrorCodeNames.ProgramCodeIsToLong)
        );

        await _publisher.PublishAsync(DockerRedisChannels.ContainerCreateAndRunChannelRequest, 
            JsonSerializer.Serialize(new ContainerCreateAndRunRequest
            {
                ConnectionId = request.ConnectionId,

                ImageId = request.ImageId,
                UserId = user.Id,
                
                ProgramCode = request.ProgramCode
            })
        );
        
        return Ok();
    }
    
    
    /// <summary>
    /// Останавливает и удаляет Docker контейнер.
    /// </summary>
    /// <param name="request">Запрос</param>
    /// <returns></returns>
    [Authorize]
    [HttpDelete("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete([FromBody] DockerContainerDeleteRequest request)
    {
        var user = await _jwtService.GetUserAsync(User);
        if (user == null)
            return Unauthorized();
        
        await _publisher.PublishAsync(DockerRedisChannels.ContainerDeleteAndStopChannelRequest, 
            JsonSerializer.Serialize(new ContainerDeleteAndStopRequest
            {
                ConnectionId = request.ConnectionId,
                
                ContainerId = request.ContainerId,
                CheckUserId = user.IsAdministrator ? null : user.Id
            })
        );
        
        return Ok();
    }
    
    
    /// <summary>
    /// Отправляет запрос на получения всех контейнер текущего пользователя. Ответ на запрос придет через SignalR (WebSocket).
    /// </summary>
    /// <param name="request">Запрос</param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("get/all/user")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllByUser([FromBody] DockerContainerGetListRequest request)
    {
        var user = await _jwtService.GetUserAsync(User);
        if (user == null)
            return Unauthorized();
        
        await _publisher.PublishAsync(DockerRedisChannels.ContainerListChannelRequest, 
            JsonSerializer.Serialize(new ContainerListRequest()
            {
                ConnectionId = request.ConnectionId,
                FilterUserId = user.Id
            })
        );
        
        return Ok();
    }
    
    /// <summary>
    /// Отправляет запрос на получения всех контейнеров. Ответ на запрос придет через SignalR (WebSocket).
    /// </summary>
    /// <param name="request">Запрос</param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("get/all/admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromBody] DockerContainerGetListRequest request)
    {
        var user = await _jwtService.GetUserAsync(User);
        if (user == null || !user.IsAdministrator)
            return Forbid();
        
        await _publisher.PublishAsync(DockerRedisChannels.ContainerListChannelRequest, 
            JsonSerializer.Serialize(new ContainerListRequest()
            {
                ConnectionId = request.ConnectionId
            })
        );
        
        return Ok();
    }
}