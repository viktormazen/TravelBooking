using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TravelBooking.API.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ApiKeyAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private const string ApiKeyHeaderName = "X-API-Key";

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            bool hasApiKey = context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValue);

            if (!hasApiKey)
            {
                context.Result = new UnauthorizedResult(); 
                return;
            }

            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            string apiKey = configuration.GetValue<string>("ApiKey"); 

            if (apiKeyHeaderValue != apiKey)
            {
                context.Result = new UnauthorizedResult(); 
                return;
            }

            await Task.CompletedTask;
        }

    }
}