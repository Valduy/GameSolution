using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Matchmaker.Services.Interfaces
{
    public interface IAuthorizationService
    {
        void Authorize(HttpContext context);
        bool CheckAuthorization(HttpContext context);
        string GetIdentifier(HttpContext context);
    }
}
