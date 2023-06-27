using JWT.Algorithms;
using JWT.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MyConversation.Helper;
using MyConversation.Model.Model;
using MyConversation.Repository.Repository;
using System.Threading.Tasks;

namespace MyConversation.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var path = httpContext.Request.Path.HasValue ? httpContext.Request.Path.Value.ToString() : String.Empty;
            path = path.Split('?').FirstOrDefault();
            if (path.StartsWith("/"))
            {
                path = path.Substring(1, path.Length - 1).ToString();
            }
            if (path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 1).ToString();
            }
            if (httpContext.Request.Path.HasValue && (UnauthorizeCollection.Collection.Any(x=>x.ToLower() == path.ToLower()) || !path.StartsWith("api/")))
            {
                await _next(httpContext);
            }
            else
            {
                var token = httpContext.Request.Headers["Authorization"].ToString();
                token = token.Replace("Bearer ", string.Empty);
                try
                {
                    var SecretKeyJWT = AppsettingConfig.GetByKey("SecretKeyJWT");
                    var jwt = JwtBuilder.Create()
                            .WithAlgorithm(new HMACSHA512Algorithm()) // symmetric
                            .WithSecret(SecretKeyJWT)
                            .MustVerifySignature()
                            .Decode<IDictionary<string, object>>(token);
                    using (var repClientApp = new ClientAppRepository(CommonHandler.GeneralDB)) 
                    {
                        using (var repUser = new UserRepository(CommonHandler.GeneralDB))
                        {
                            var userResponse = repUser.Single(x => x.Active == true && x.Username == jwt["username"].ToString() && x.Password == jwt["password"].ToString() && x.CurrentToken == token);
                            var clientAppResponse = repClientApp.Single(x => x.Active && x.ClientId == userResponse.Data.ClientId);
                            if (!string.IsNullOrEmpty(token) && userResponse.IsSuccess && userResponse.Data != null && 
                                (userResponse.Data.IsRoot || clientAppResponse.Data != null))
                            {
                                await _next(httpContext);
                            }
                            else
                            {
                                //update status user
                                if (userResponse.Data != null) {
                                    userResponse.Data.CurrentToken = null;
                                    userResponse.Data = (User)CommonHandler.UpdateField(userResponse.Data);
                                    userResponse.Data.Status = Model.Common.EnumDefinition.UserStatus.offline;
                                    repUser.UpdateOne(x => x.CurrentToken == token, userResponse.Data, "Status;CurrentToken;ModifiedDate;ModifiedBy");
                                }
                                httpContext.Response.StatusCode = 401;
                                await httpContext.Response.WriteAsync("<div> Unauthorize </div>");
                            }
                        }
                    }
                }
                catch
                {
                    httpContext.Response.StatusCode = 401;
                    await httpContext.Response.WriteAsync("<div> Unauthorize </div>");
                    using (var repUser = new UserRepository(CommonHandler.GeneralDB))
                    {
                        var existedUser = repUser.Single(x => x.CurrentToken == token);
                        if(existedUser.Data == null)
                        {
                            return;
                        }
                        existedUser.Data.CurrentToken = null;
                        existedUser.Data = (User)CommonHandler.UpdateField(existedUser.Data);
                        existedUser.Data.Status = Model.Common.EnumDefinition.UserStatus.offline;
                        repUser.UpdateOne(x => x.CurrentToken == token, existedUser.Data, "Status;CurrentToken;ModifiedDate;ModifiedBy");
                    }
                }
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class JwtMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtMiddleware>();
        }
    }
}
