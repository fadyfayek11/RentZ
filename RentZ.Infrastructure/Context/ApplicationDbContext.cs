#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentZ.Domain.Entities;

namespace RentZ.Infrastructure.Context;

public class ApplicationDbContext : IdentityDbContext<User,IdentityRole<Guid>, Guid>
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
        ChangeTracker.LazyLoadingEnabled = true;
    }

	public DbSet<Admin> Admins { get; set; }
	public DbSet<OtpSetup> OtpSetups { get; set; }
	public DbSet<Client> Clients { get; set; }
	public DbSet<City> City { get; set; }
	public DbSet<Governorate> Governorate { get; set; }

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		// Configure GUID primary keys for user entities
		builder.Entity<User>().Property(u => u.Id).ValueGeneratedOnAdd();
       
        builder.Entity<Admin>()
            .HasOne(a => a.User)
            .WithOne()
            .HasForeignKey<Admin>(a => a.Id);

        builder.Entity<Client>()
            .HasOne(c => c.User)
            .WithOne()
            .HasForeignKey<Client>(c => c.Id);

    }
}

