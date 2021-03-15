using DarnTheLuck.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DarnTheLuck.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Ticket> Tickets { get; set; }

        /*
         * Both status and statuses are correct plural spellings. ✔
         * https://onlinewritingtraining.com.au/plural-of-status/
         */
        public DbSet<TicketStatus> TicketStatuses { get; set; }

        public DbSet<TicketNotes> TicketNotes { get; set; }

        public DbSet<UserGroup> UserGroups { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserGroup>()
                .HasKey(u => new { u.UserId, u.GrantId });
        }
    }
}
