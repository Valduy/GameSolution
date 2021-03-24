using Matchmaker.Exceptions;
using Matchmaker.Messages;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Matchmaker.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        [Route("error")]
        public ErrorMessage Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context.Error;

            if (exception is HttpStatusException httpException)
            {
                Response.StatusCode = (int)httpException.Status;
            }

            return new ErrorMessage(exception.Message);
        }
    }
}
