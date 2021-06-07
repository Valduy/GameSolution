using System.Net;
using System.Net.Sockets;
using Matchmaker.Helpers;
using Matchmaker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Network;
using Network.Messages;

namespace Matchmaker.Controllers
{
    [Route("api/[controller]")]
    public class MatchmakerController : ControllerBase
    {
        private static readonly string LocalIp = NetworkHelper.GetLocalIPAddress();

        private readonly IMatchmakerService _matchmakerService;
        private readonly ILogger<MatchmakerController> _logger;

        public MatchmakerController(
            IMatchmakerService matchmakerService,
            ILogger<MatchmakerController> logger)
        {
            _matchmakerService = matchmakerService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("queue")]
        public void Enqueue([FromBody]ClientEndPoint privateEndPoint)
        {
            var userId = User.GetName();
            _logger.LogInformation($"Запрос на постановку в очередь (id пользователя: {userId}).");
            var publicEndPoint = new ClientEndPoint(GetClientIp(), 0);
            var endPoints = new ClientEndPoints(publicEndPoint, privateEndPoint);
            _matchmakerService.Enqueue(userId, endPoints);
        }

        [Authorize]
        [HttpGet("status")]
        public UserStatus GetStatus()
        {
            var userId = User.GetName();
            _logger.LogInformation($"Запрос на получение статуса (id пользователя: {userId}).");
            return _matchmakerService.GetStatus(userId);
        }

        [Authorize]
        [HttpGet("match")]
        public int? GetMatch()
        {
            var userId = User.GetName();
            _logger.LogInformation($"Запрос на получение матча (id пользователя: {userId}).");
            return _matchmakerService.GetMatch(userId);
        }

        [Authorize]
        [HttpDelete("queue")]
        public bool Remove()
        {
            var userId = User.GetName();
            _logger.LogInformation($"Запрос на выход из очереди (id пользователя: {userId}).");
            return _matchmakerService.Remove(userId);
        }

        private string GetClientIp()
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6
                ? LocalIp // Если сервер запущен локально, то RemoteIpAddress представлени в виде IPv6 адреса. Заменяем его на IPv4.
                : HttpContext.Connection.RemoteIpAddress.ToString();

            // Если запрос пришел с компьютера, на котором запущен сервер, заменяем ip на 127.0.0.1.
            return clientIp == LocalIp ? IPAddress.Loopback.ToString() : clientIp;
        }
    }
}
