
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
            foreach(var role in roles.Roles) {
                await AddRole(role);
            }

            return Ok();
        }

        [HttpPut("role")]
        public async Task<IActionResult> AddUsersToRoles([FromBody]UsersRoles usersRoles){
            foreach(var u in usersRoles.Users) {
                await AddUserToRoles(u);
            }
            
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
            var roles = data.Roles.Select(r => r.Name);
            await _userManager.AddToRolesAsync(user, roles);
        }
    }
}