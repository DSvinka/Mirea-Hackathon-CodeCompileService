using System.Text.Json;
using API.Auth.Services;
using API.Shared.Queries.Requests.Docker.Images;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Docker.Shared;
using Services.Docker.Shared.Messages.Requests.Images;
using Shared.Utils.Redis.Services;
using StackExchange.Redis;

namespace API.Controllers;

[Route("api/services/docker/images")]
[ApiController]
public class DockerImageController: ControllerBase
{
    private readonly JwtService _jwtService;

    private readonly ISubscriber _publisher;

    public DockerImageController(RedisService redisService, JwtService jwtService)
    {
        _jwtService = jwtService;

        _publisher = redisService.Connection.GetSubscriber();
    }


    /// <summary>
    /// Добавление Docker образа. После добавления образ сразу становится доступен для использования пользователям, будьте аккуратны с настройками.
    /// </summary>
    /// <param name="request">Запрос</param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("add")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] DockerImageAddRequest request)
    {
        var user = await _jwtService.GetUserAsync(User);
        if (user == null || !user.IsAdministrator)
            return Forbid();

        await _publisher.PublishAsync(DockerRedisChannels.ImageAddChannelRequest, 
            JsonSerializer.Serialize(new ImageAddRequest
            {
                ConnectionId = request.ConnectionId,
                
                Image = request.Image,
                ImageTag = request.ImageTag,
                
                DisplayName = request.DisplayName,
                Description = request.Description,
                
                CodeFileExtension = request.CodeFileExtension,
                CodeEditorLang = request.CodeEditorLang,
                
                CodeInitCommand = request.CodeInitCommand,
                CodeStartCommand = request.CodeStartCommand,
                
                MaxMemory = request.MaxMemory,
                MaxCpuShares = request.MaxCpuShares,
                MaxStorage = request.MaxStorage,
                
                MaxCountByUser = request.MaxCountByUser,
                MaxCountByServer = request.MaxCountByServer
            })
        );
        
        return Ok();
    }
    
    
    /// <summary>
    /// Уберает Docker образ. После уберания вместе с образом удаляются и контейнеры. Будьте аккуратны с ImageId.
    /// </summary>
    /// <param name="request">Запрос</param>
    /// <returns></returns>
    [Authorize]
    [HttpDelete("remove")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete([FromBody] DockerImageRemoveRequest request)
    {
        var user = await _jwtService.GetUserAsync(User);
        if (user == null || !user.IsAdministrator)
            return Forbid();
        
        await _publisher.PublishAsync(DockerRedisChannels.ImageRemoveChannelRequest, 
            JsonSerializer.Serialize(new ImageRemoveRequest
            {
                ConnectionId = request.ConnectionId,
                ImageId = request.ImageId
            })
        );
        
        return Ok();
    }
    
    
    /// <summary>
    /// Отправляет запрос на получения всех образов. Ответ на запрос придет через SignalR (WebSocket).
    /// </summary>
    /// <param name="request">Запрос</param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("get/all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromBody] DockerImageGetAllRequest request)
    {
        var user = await _jwtService.GetUserAsync(User);
        if (user == null)
            return Unauthorized();
        
        await _publisher.PublishAsync(DockerRedisChannels.ImageListChannelRequest, 
            JsonSerializer.Serialize(new ImageListRequest()
            {
                ConnectionId = request.ConnectionId
            })
        );
        
        return Ok();
    }
}