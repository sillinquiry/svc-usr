using System;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OpenIddict.Core;
using svc_usr.Models;
using svc_usr.Models.Identity;

namespace svc_usr.Controllers {
    [Route("api")]
    public class UsrController : Controller {
        private readonly UserManager<Usr> _userManager;

        public UsrController(UserManager<Usr> userManager) {
            _userManager = userManager;
        }

        [HttpPost("usr")]
        public async Task<IActionResult> Register([FromBody]Registration info) {
            var user = new Usr {
                UserName = String.IsNullOrEmpty(info.Username) ? info.Email : info.Username,
                Email = info.Email
            };
            
            var createResult = await _userManager.CreateAsync(user, info.Password);
            if(createResult.Succeeded) {
                return Ok();
            }

            return BadRequest(new { ErrorMessage = createResult.Errors });
        }

        [Authorize()]
        [HttpGet("usrinfo")]
        public async Task<IActionResult> UserInfo() {
            var user = await _userManager.GetUserAsync(User);
            if(user == null) {
                return BadRequest(new OpenIdConnectResponse {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = "The user profile is no longer available"
                });
            }

            var claims = new JObject();
            claims[OpenIdConnectConstants.Claims.Subject] = await _userManager.GetUserIdAsync(user);
            
            if(User.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIdConnectConstants.Scopes.Email)) {
                claims[OpenIdConnectConstants.Claims.Email] = await _userManager.GetEmailAsync(user);
            }

            if(User.HasClaim(OpenIdConnectConstants.Claims.Scope, OpenIddictConstants.Scopes.Roles)) {
                claims[OpenIddictConstants.Claims.Roles] = JArray.FromObject(await _userManager.GetRolesAsync(user));
            }

            return Json(claims);
        }
    }
}