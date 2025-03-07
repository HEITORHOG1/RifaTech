using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RifaTech.API.Entities;

namespace RifaTech.API.Context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Rifa> Rifas { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ExtraNumber> ExtraNumbers { get; set; }
        public DbSet<Draw> Draws { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<UnpaidRifa> UnpaidRifas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurações de relacionamento
            builder.Entity<Ticket>()
                .HasOne(t => t.Rifa)
                .WithMany(r => r.Tickets)
                .HasForeignKey(t => t.RifaId);

            builder.Entity<Ticket>()
                .HasOne(t => t.Cliente)
                .WithMany()
                .HasForeignKey(t => t.ClienteId);

            builder.Entity<Payment>()
                .HasOne(p => p.Ticket)
                .WithMany()
                .HasForeignKey(p => p.TicketId);

            builder.Entity<Payment>()
                .HasOne(p => p.Cliente)
                .WithMany()
                .HasForeignKey(p => p.ClienteId);

            builder.Entity<ExtraNumber>()
                .HasOne(en => en.Rifa)
                .WithMany(r => r.ExtraNumbers)
                .HasForeignKey(en => en.RifaId);

            builder.Entity<Draw>()
                .HasOne(d => d.Rifa)
                .WithMany()
                .HasForeignKey(d => d.RifaId);
            builder.Entity<Rifa>().Ignore(r => r.ExtraNumbers);
        }
    }
}