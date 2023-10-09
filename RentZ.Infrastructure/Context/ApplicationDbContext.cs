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
	}

	public DbSet<Admin> Admins { get; set; }
	public DbSet<Client> Clients { get; set; }

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		// Configure GUID primary keys for user entities
		builder.Entity<User>().Property(u => u.Id).ValueGeneratedOnAdd();
		builder.Entity<Admin>().Property(a => a.Id).ValueGeneratedOnAdd();
		builder.Entity<Client>().Property(c => c.Id).ValueGeneratedOnAdd();
	}
}

