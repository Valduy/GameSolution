using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Matchmaker.Services
{
    public class SimpleAuthorizationService : ISimpleAuthorizationService
    {
        public void Authorize(HttpContext context) => context.Session.SetString("role", "player");
        public bool CheckAuthorization(HttpContext context) => context.Session.Keys.Contains("role");
        public string GetIdentifier(HttpContext context) => context.Session.Id;
    }
}
