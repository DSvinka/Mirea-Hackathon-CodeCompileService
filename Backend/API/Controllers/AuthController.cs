using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/services/auth")]
[ApiController]
public class AuthController: ControllerBase
{
    /// <summary>
    /// Создание продукта
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    ///
    ///     POST /Todo
    ///     {
    ///        "id" : 1, 
    ///        "name" : "A4Tech Bloody B188",
    ///        "price" : 111,
    ///        "Type": "PeripheryAndAccessories"
    ///     }
    ///
    /// </remarks>
    /// <param name="model">Продукт</param>
    /// <returns></returns>
    [HttpPost("Login")]
    public IActionResult Create([FromBody] ProductModel model)
    {
        return Ok();
    }
}