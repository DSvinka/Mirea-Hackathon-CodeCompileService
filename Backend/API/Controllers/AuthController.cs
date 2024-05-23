using System.Text.Json;
using API.Auth.Services;
using API.Database;
using API.Database.Models;
using API.Shared.Queries.Requests.Auth;
using API.Shared.Queries.Responses;
using API.Shared.Queries.Responses.Auth;
using API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace API.Controllers;

[Route("api/services/auth")]
[ApiController]
public class AuthController: ControllerBase
{
    private readonly AuthDbContext _authDbContext;
    private readonly JwtService _jwtService;
    private readonly HashService _hashService;
    
    public AuthController(AuthDbContext authDbContext, HashService hashService, JwtService jwtService)
    {
        _authDbContext = authDbContext;
        _hashService = hashService;
        _jwtService = jwtService;
    }
    
    
    /// <summary>
    /// Авторизует в аккаунт
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    ///
    ///     POST /login
    ///     {
    ///        "email" : "itsauser@dsvinka.ru",
    ///        "password" : "its-a-secret",
    ///     }
    ///
    /// </remarks>
    /// <param name="request">Запрос</param>
    /// <returns>Токен авторизации и токен обновления</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> LoginAsync([FromBody] LoginRequest request)
    {
        var user = await _authDbContext.Users.FirstOrDefaultAsync(model => model.Email == request.Email);
        if (user == null)
        {
            return Unauthorized(new ErrorResponse
            {
                Message = "Login or Password is wrong!"
            }.WithCode("services.auth", ErrorCodeNames.LoginOrPasswordIsIncorrect));
        }

        if (!_hashService.EqualsHash(request.Password, user.PasswordHash))
        {
            return Unauthorized(new ErrorResponse
                {
                    Message = "Login or Password is wrong!"
                }.WithCode("services.auth", ErrorCodeNames.LoginOrPasswordIsIncorrect)
            );
        }

        user.RefreshToken = _jwtService.GenerateRefreshToken();
        user.RefreshTokenExpire = _jwtService.GetRefreshTokenExpire();
        
        var response = new AuthResponse()
        {
            AccessToken = _jwtService.GenerateAccessToken(user.Id, user.Email),
            RefreshToken = user.RefreshToken
        };

        _authDbContext.Users.Update(user);
        await _authDbContext.SaveChangesAsync();

        return Ok(response);
    }
    
    
    /// <summary>
    /// Создает новый аккаунт (но не входит в него)
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    ///
    ///     POST /register
    ///     {
    ///         "firstName": "Владимир"
    ///         "lastName": "Владимирович"
    ///
    ///         "email": "vladimir@icloud.com"
    ///         "phone": "+7(912)123-45-67"
    ///
    ///         "password": "Hochu Koshku"
    ///     }
    ///
    /// </remarks>
    /// <param name="request">Запрос</param>
    /// <returns></returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
    {
        if (_authDbContext.Users.Any(user => user.Email == request.Email))
        {
            return Conflict(new ErrorResponse
            {
                Message = "This email is already use!"
            }.WithCode("services.auth", ErrorCodeNames.EmailIsAlreadyUse));
        }
        
        if (request.Phone != null && _authDbContext.Users.Any(user => user.Phone == request.Phone))
        {
            return Conflict(new ErrorResponse
            {
                Message = "This phone is already use!"
            }.WithCode("services.auth", ErrorCodeNames.PhoneIsAlreadyUse));
        }

        await _authDbContext.Users.AddAsync(new UserModel
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            
            Email = request.Email,
            Phone = request.Phone,
            
            IsAdministrator = false,
            PasswordHash = _hashService.GetHash(request.Password)
        });
        
        await _authDbContext.SaveChangesAsync();
        return Ok();
    }
    
    
    /// <summary>
    /// Посмотреть свой профиль
    /// </summary>
    /// <returns>Профиль авторизованного пользователя</returns>
    [Authorize]
    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProfileResponse>> ProfileAsync()
    {
        var user = await _jwtService.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new ErrorResponse
            {
                Message = "You are not authorized!"
            }.WithCode("services.auth", ErrorCodeNames.AccessTokenIsIncorrect));
        }
        
        var response = new ProfileResponse()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,

            Email = user.Email,
            Phone = user.Phone,

            IsAdministrator = user.IsAdministrator,
        };

        return Ok(response);
    }
    
    
    /// <summary>
    /// Обновляет токен авторизации токеном обновления
    /// </summary>
    /// <remarks>
    /// Пример запроса:
    ///
    ///     POST /refresh
    ///     {
    ///         "refreshToken": "asdsSNjdijDS*98j3nJKSDb1"
    ///     }
    ///
    /// </remarks>
    /// <param name="request">Запрос</param>
    /// <returns>Новый токен авторизации и обновления</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> RefreshAsync([FromBody] RefreshRequest request)
    {
        var user = await _authDbContext.Users.FirstOrDefaultAsync(model => model.RefreshToken != null && model.RefreshToken.Equals(request.RefreshToken));
        if (user == null)
        {
            return Unauthorized(new ErrorResponse
                {
                    Message = "You are not authorized!"
                }.WithCode("services.auth", ErrorCodeNames.RefreshTokenIsIncorrect)
            );
        }

        if (user.RefreshTokenExpire < DateTime.UtcNow)
        {
            return Unauthorized(new ErrorResponse
                {
                    Message = "You are not authorized!"
                }.WithCode("services.auth", ErrorCodeNames.RefreshTokenIsExpire)
            );
        }
        
        user.RefreshToken = _jwtService.GenerateRefreshToken();
        user.RefreshTokenExpire = _jwtService.GetRefreshTokenExpire();
        
        var response = new AuthResponse()
        {
            AccessToken = _jwtService.GenerateAccessToken(user.Id, user.Email),
            RefreshToken = user.RefreshToken
        };

        _authDbContext.Users.Update(user);
        await _authDbContext.SaveChangesAsync();

        return Ok(response);
    }
     
    
    /// <summary>
    /// Показывает авторизован ли пользователь и имеет ли он права администратора
    /// </summary>
    /// <param name="request">Запрос</param>
    /// <returns>Статус авторизации и наличие прав администратора</returns>
    [Authorize]
    [HttpGet("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<VerifyResponse>> VerifyAsync()
    {
        var response = new VerifyResponse()
        {
            IsAdministrator = false,
            IsAuthorized = false
        };
        
        var user = await _jwtService.GetUserAsync(User);
        if (user != null)
        {
            response.IsAuthorized = true;
            response.IsAdministrator = user.IsAdministrator;
        }

        return Ok(response);
    }
}