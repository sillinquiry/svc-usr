using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using AspNet.Security.OpenIdConnect.Primitives;
using svc_usr.Models;
using svc_usr.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using AspNet.Security.OpenIdConnect.Server;
using OpenIddict.Core;
using AspNet.Security.OpenIdConnect.Extensions;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using svc_usr.Data;
using System;
using Microsoft.AspNetCore.Http;

namespace svc_usr.Controllers {
    [Route("api/auth")]
    public class AuthController : Controller {
        private readonly UserManager<Usr> _userManager;
        private readonly SignInManager<Usr> _signInManager;
        private readonly IOptions<IdentityOptions> _identityOptions;
        
        public AuthController(UserManager<Usr> userManager, SignInManager<Usr> signInManager, IOptions<IdentityOptions> identityOptions) {
            _userManager = userManager;
            _signInManager = signInManager;
            _identityOptions = identityOptions;
        }

        [HttpPost("token"), Produces("application/json")]
        public async Task<IActionResult> GenerateToken(OpenIdConnectRequest request) {
            try {
                AuthenticationTicket ticket = null;
                if(request.IsPasswordGrantType()) {
                    ticket = await PasswordGrant(request);
                }
                else if (request.IsRefreshTokenGrantType()) {
                    ticket = await RefreshGrant(request);
                } else {
                    throw new ArgumentOutOfRangeException(Messages.ERROR_UNSUPPORTED_GRANT_TYPE);
                }

                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            } catch(ArgumentOutOfRangeException e) {
                return BadRequest(new OpenIdConnectResponse {
                    Error = OpenIdConnectConstants.Errors.UnsupportedGrantType,
                    ErrorDescription = e.Message
                });
            } catch(ArgumentException e) {
                return BadRequest(new OpenIdConnectResponse {
                    Error = OpenIdConnectConstants.Errors.InvalidGrant,
                    ErrorDescription = e.Message
                });
            } catch {
                //TODO: Catch Exceptions for logging/tracking
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

        }

        private async Task<AuthenticationTicket> PasswordGrant(OpenIdConnectRequest request) {
            var user = await _userManager.FindByNameAsync(request.Username) ?? await _userManager.FindByEmailAsync(request.Username);
            if(user == null) {
                throw new ArgumentException(Messages.ERROR_INVALID_USERNAME_PASSWORD);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if(!result.Succeeded) {
                throw new ArgumentException(Messages.ERROR_INVALID_USERNAME_PASSWORD);
            }

            return await CreateTicketAsync(request, user);
        }

        private async Task<AuthenticationTicket> RefreshGrant(OpenIdConnectRequest request) {
            var info = await HttpContext.AuthenticateAsync(OpenIdConnectServerDefaults.AuthenticationScheme);

            var user = await _userManager.GetUserAsync(info.Principal);
            if(user == null) {
                throw new ArgumentException(Messages.ERROR_INVALID_USERNAME_PASSWORD);
            }

            if(!await _signInManager.CanSignInAsync(user)) {
                throw new ArgumentException(Messages.ERROR_ACCOUNT_LOGIN_REVOKED);
            }

            return await CreateTicketAsync(request, user, info.Properties);
        }

        private async Task<AuthenticationTicket> CreateTicketAsync(OpenIdConnectRequest request, Usr user, AuthenticationProperties properties = null) {
            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            var ticket = new AuthenticationTicket(principal, properties, OpenIdConnectServerDefaults.AuthenticationScheme);
            
            if(!request.IsRefreshTokenGrantType()) {
                ticket.SetScopes(new[]{
                    OpenIdConnectConstants.Scopes.OfflineAccess,
                    OpenIdConnectConstants.Scopes.OpenId,
                    OpenIdConnectConstants.Scopes.Email,
                    OpenIdConnectConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles
                }.Intersect(request.GetScopes()));
            }

            ticket.SetResources("resource-server");

            foreach(var claim in ticket.Principal.Claims) {
                if(claim.Type == _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType) continue;

                var destinations = new List<string> {
                    OpenIdConnectConstants.Destinations.AccessToken
                };

                if((claim.Type == OpenIdConnectConstants.Claims.Name && ticket.HasScope(OpenIdConnectConstants.Scopes.Profile)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Email && ticket.HasScope(OpenIdConnectConstants.Scopes.Email)) ||
                    (claim.Type == OpenIdConnectConstants.Claims.Role && ticket.HasScope(OpenIddictConstants.Claims.Roles))) {

                    destinations.Add(OpenIdConnectConstants.Destinations.IdentityToken);
                }

                claim.SetDestinations(destinations);
            }

            return ticket;
        }
    }
}