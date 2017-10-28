using System;
using Microsoft.AspNetCore.Identity;

namespace svc_usr.Models.Identity {
    public class Role : IdentityRole<Guid> {
        public Role() : base() { }
        public Role(string name) : base(name) { }
    }
}