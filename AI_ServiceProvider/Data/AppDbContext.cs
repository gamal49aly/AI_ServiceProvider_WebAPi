//namespace AI_ServiceProvider.Data
using AI_ServiceProvider.Models;
using Microsoft.EntityFrameworkCore;

namespace AI_ServiceProvider.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ImageParserInput> ImageParserInputs { get; set; }
        public DbSet<ImageParserOutput> ImageParserOutputs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the one-to-one relationship between Input and Output
            modelBuilder.Entity<ImageParserOutput>()
                .HasOne(o => o.Input)
                .WithOne(i => i.Output)
                .HasForeignKey<ImageParserOutput>(o => o.InputId);

            // Seed initial data for subscriptions
            SeedSubscriptions(modelBuilder);
        }

        private void SeedSubscriptions(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subscription>().HasData(
                new Subscription
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111111"),
                    Name = "Free Tier",
                    Price = 0,
                    BillingCycle = "monthly",
                    MaxUsagePerMonth = 10,
                    // Use a static, hardcoded DateTime
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
        new Subscription
        {
            Id = new Guid("22222222-2222-2222-2222-222222222222"),
            Name = "Pro Plan",
            Price = 29.99m,
            BillingCycle = "monthly",
            MaxUsagePerMonth = 500,
            // Use a static, hardcoded DateTime
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        },
        new Subscription
        {
            Id = new Guid("33333333-3333-3333-3333-333333333333"),
            Name = "Enterprise",
            Price = 199.99m,
            BillingCycle = "monthly",
            MaxUsagePerMonth = -1,
            // Use a static, hardcoded DateTime
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        }
            );
        }
    }
}