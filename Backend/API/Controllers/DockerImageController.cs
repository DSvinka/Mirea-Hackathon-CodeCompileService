using API.Shared.Queries.Requests.Docker.Images;
using Microsoft.AspNetCore.Mvc;
using Shared.Utils.Redis.Services;

namespace API.Controllers;

[Route("api/services/docker/images")]
[ApiController]
public class DockerImageController: ControllerBase
{
    private readonly RedisService _redisService;

    public DockerImageController(RedisService redisService)
    {
        _redisService = redisService;
    }


    /// <summary>
    /// Добавление Docker образа. После добавления образ сразу становится доступен для использования пользователям, будьте аккуратны с настройками.
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    ///
    ///     POST /
    ///     {
    ///        "id" : 1, 
    ///        "name" : "A4Tech Bloody B188",
    ///        "price" : 111,
    ///        "Type": "PeripheryAndAccessories"
    ///     }
    ///
    /// </remarks>
    /// <param name="model">Настройки Образа</param>
    /// <returns></returns>
    [HttpPost]
    public IActionResult Create([FromBody] DockerImagePostRequest model)
    {
        
        
        return Ok(model);
    }
}