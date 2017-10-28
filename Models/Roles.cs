using System;
using System.Collections.Generic;

namespace svc_usr.Models {

    public class RoleData {
        public String Name { get; set; }
    }

    public class CreateRoles {
        public List<RoleData> Roles { get; set; }
    }


    public class UserRoleData {
        public String Username { get; set; }
        public List<RoleData> Roles { get; set; }
    }

    public class UsersRoles {
        public List<UserRoleData> Users { get; set; }
    }
}