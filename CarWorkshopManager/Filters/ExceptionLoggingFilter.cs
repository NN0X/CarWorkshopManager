using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace CarWorkshopManager.Filters
{
    public class ExceptionLoggingFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionLoggingFilter> _logger;
        public ExceptionLoggingFilter(ILogger<ExceptionLoggingFilter> logger)
            => _logger = logger;

        public void OnException(ExceptionContext context)
        {
            var controller = context.RouteData.Values["controller"];
            var action = context.RouteData.Values["action"];

            _logger.LogError(context.Exception,
                             "Unhandled exception in {Controller}.{Action}",
                             controller, action);
        }
    }
}
