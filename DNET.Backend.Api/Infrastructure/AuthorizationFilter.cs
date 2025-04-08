// using System.Security.Claims;
// using DNET.Backend.Api.Options;
// using DNET.Backend.Api.Services;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Filters;
// using Microsoft.Extensions.Options;
//  
// namespace DNET.Backend.Api.Infrastructure;
//
// public class AuthorizationFilter : IAuthorizationFilter
// {
//     private readonly IJwtValidator _jwtValidator;
//
//     public AuthorizationFilter(IJwtValidator jwtValidator)
//     {
//         _jwtValidator = jwtValidator;
//     }
//     
//     public void OnAuthorization(AuthorizationFilterContext context)
//     {
//         if (context.HttpContext.Request.Headers.TryGetValue("My-Authorization", out var apiKeyHeader))
//         {
//             var header = apiKeyHeader.ToString();
//
//             if (!header.StartsWith("Bearer "))
//             {
//                 SetInvalidResult(context);
//                 return;
//             }
//
//             var token = header.Substring("Bearer ".Length).Trim();
//
//             try
//             {
//                 var claims = _jwtValidator.ValidateJwtToken(token);
//                 if (claims.Count == 0)
//                 {
//                     SetInvalidResult(context);
//                     return;
//                 }
//
//                 context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
//             }
//             catch (Exception)
//             {
//                 SetInvalidResult(context);
//             }
//         }
//         else 
//         {
//             SetInvalidResult(context);
//         }
//         
//         SetInvalidResult(context);
//
//     }
//     
//     private void SetInvalidResult(AuthorizationFilterContext context)
//     {
//         context.Result = new ObjectResult(new { message = "Unauthorized", statusCode = 401 }) 
//         {
//             StatusCode = 401
//         };
//     }
// }