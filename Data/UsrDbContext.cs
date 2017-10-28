using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using svc_usr.Models.Identity;

namespace svc_usr.Data {
    public class UsrDbContext : IdentityDbContext<Usr, Role, Guid> {
        public UsrDbContext(DbContextOptions<UsrDbContext> options) : base(options) {

        }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);
        }
    }
}