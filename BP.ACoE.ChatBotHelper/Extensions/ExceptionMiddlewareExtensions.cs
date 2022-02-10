using System.Net;
using BP.ACoE.ChatBotHelper.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BP.ACoE.ChatBotHelper.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILogger logger)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {

                    context.Response.ContentType = "application/json";
                    var errorMessage = "Internal Server Error";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var error = contextFeature.Error;
                        switch (error)
                        {
                            //  return JSON in proper format
                            case MuleSoftException _:
                                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                errorMessage = error.Message;
                                break;
                            case HttpRequestException _:
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                errorMessage = error.Message;
                                break;
                            case ArgumentNullException _:
                                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                errorMessage = error.Message;
                                break;

                            default:
                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                break;
                        }
                        logger.LogError($"{context.Request.Path}: failed with error: {error}");
                        await context.Response.WriteAsync(new ErrorDetail()
                        {
                            StatusCode = context.Response.StatusCode = (int)HttpStatusCode.InternalServerError,
                            Message = errorMessage
                        }.ToString());
                    }
                });
            });
        }
    }
}
