
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using svc_usr.Models;
using svc_usr.Models.Identity;

namespace svc_usr.Controllers {
    [Route("api")]
    public class RoleController : Controller {

        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<Usr> _userManager;

        public RoleController(RoleManager<Role> roleManager, UserManager<Usr> userManager) {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpPost("role")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoles roles) {
            await Task.WhenAll(
                roles.Roles.Select(r => { return AddRole(r); }).ToArray()
            );

            return Ok();
        }

        [HttpPut("role")]
        public async Task<IActionResult> AddUsersToRoles([FromBody]UsersRoles usersRoles){
            await Task.WhenAll(
                usersRoles.Users.Select(ur => { return AddUserToRoles(ur); })
            );
            return Ok();
        }

        private async Task AddRole(RoleData role) {
            if(await _roleManager.RoleExistsAsync(role.Name) == false) {
                var newRole = new Role();
                newRole.Name = role.Name;
                await _roleManager.CreateAsync(newRole);
            }
        }

        private async Task AddUserToRoles(UserRoleData data) {
            Usr user = await _userManager.FindByNameAsync(data.Username) ?? await _userManager.FindByEmailAsync(data.Username);
            await Task.WhenAll(
                data.Roles.Select(r => { return AddUserToRole(user, r); }).ToArray()
            );
        }

        private async Task AddUserToRole(Usr user, RoleData roleData) {
            if(await _roleManager.RoleExistsAsync(roleData.Name)) {
                await _userManager.AddToRoleAsync(user, roleData.Name);
            }
        }
    }
}