#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentZ.Domain.Entities;
using System.Reflection.Emit;

namespace RentZ.Infrastructure.Context;

public sealed class ApplicationDbContext : IdentityDbContext<User,IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
        ChangeTracker.LazyLoadingEnabled = true;
    }

	public DbSet<Property> Properties { get; set; }
	public DbSet<PropertyUtility> PropertyUtilities { get; set; }
	public DbSet<FavProperty> FavProperties { get; set; }
	public DbSet<Media> Media { get; set; }
	public DbSet<Utility> Utilities { get; set; }
	public DbSet<Admin> Admins { get; set; }
	public DbSet<OtpSetup> OtpSetups { get; set; }
	public DbSet<Client> Clients { get; set; }
	public DbSet<City> City { get; set; }
	public DbSet<Governorate> Governorate { get; set; }
	public DbSet<FeedBack> FeedBack { get; set; }
	public DbSet<Notification> Notifications { get; set; }
	public DbSet<Message> Messages { get; set; }
	public DbSet<Conversation> Conversations { get; set; }


	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);
		SeedRoles(builder);

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

        builder.Entity<PropertyUtility>()
            .HasKey(sc => new { sc.PropertyId, sc.UtilityId });

        builder.Entity<PropertyUtility>()
            .HasOne(sc => sc.Property)
            .WithMany(s => s.PropertyUtilities)
            .HasForeignKey(sc => sc.PropertyId);

        builder.Entity<PropertyUtility>()
            .HasOne(sc => sc.Utility)
            .WithMany(c => c.PropertyUtilities)
            .HasForeignKey(sc => sc.UtilityId);

        builder.Entity<FavProperty>()
            .HasKey(fp => new { fp.PropertyId, fp.ClientId });
    }
    private void SeedRoles(ModelBuilder builder)
    {
        var adminId = Guid.NewGuid();

        builder.Entity<User>().HasData(
            new User()
            {
                Id = adminId,
                UserName = "admin@rentz.com",
                NormalizedUserName = "ADMIN@RENTZ.COM",
                Email = "admin@rentz.com",
                NormalizedEmail = "ADMIN@RENTZ.COM",
                EmailConfirmed = true,
                IsActive = true,
                PasswordHash = new PasswordHasher<User>().HashPassword(null, "P@ssw0rd123") 
            }
        );
        
        builder.Entity<Admin>().HasData(
            new Admin()
            {
                Id = adminId,
                IsRoot = true
            }
        );

        builder.Entity<IdentityUserRole<Guid>>().HasData(
            new IdentityUserRole<Guid>
            {
                RoleId = Guid.Parse("45ebc48e-b867-4847-a1e6-ba1f275fc406"),
                UserId = adminId
            }
        );
        builder.Entity<IdentityRole>().HasData(
			new IdentityRole() { Id = "9f4cbe69-c735-46d0-9634-4cf435c46184", Name = "Admin", ConcurrencyStamp = "2", NormalizedName = "Admin" },
			new IdentityRole() { Id = "45ebc48e-b867-4847-a1e6-ba1f275fc406", Name = "RootAdmin", ConcurrencyStamp = "1", NormalizedName = "RootAdmin" },
			new IdentityRole() { Id = "fb7f4a16-6f0b-4fa9-9f94-4db50b98014b", Name = "Client", ConcurrencyStamp = "3", NormalizedName = "Client" }
		);
	}
}