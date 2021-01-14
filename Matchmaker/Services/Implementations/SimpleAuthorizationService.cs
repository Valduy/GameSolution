using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Matchmaker.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Matchmaker.Services.Implementations
{
    public class SimpleAuthorizationService : IAuthorizationService
    {
        private readonly Uri AuthorizationServiceAddress;

        public SimpleAuthorizationService()
        {
            AuthorizationServiceAddress = new Uri("https://localhost:5001/api/authorization/id");
        }

        public async Task<bool> CheckAuthorizationAsync(HttpContext context)
        {
            var container = new CookieContainer();
            var client = new HttpClient(new HttpClientHandler {CookieContainer = container});

            foreach (var cookie in context.Request.Cookies.Select(c => new Cookie(c.Key, c.Value)))
            {
                container.Add(AuthorizationServiceAddress, cookie);
            }

            var response = await client.GetAsync(AuthorizationServiceAddress);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
    }
}
