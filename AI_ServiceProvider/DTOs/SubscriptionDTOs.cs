using System.ComponentModel.DataAnnotations;

namespace AI_ServiceProvider.DTOs
{
    public class CreateCheckoutSessionRequestDTO
    {
        [Required]
        public Guid SubscriptionId { get; set; }
    }

    public class CreateCheckoutSessionResponseDTO
    {
        public string SessionId { get; set; }
        public string SessionUrl { get; set; }
        public string PublishableKey { get; set; }
    }

    public class SubscriptionDetailsDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string BillingCycle { get; set; }
        public int MaxUsagePerMonth { get; set; }
        public bool IsCurrentPlan { get; set; }
    }

    public class UserSubscriptionStatusDTO
    {
        public string CurrentPlan { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public int UsageThisMonth { get; set; }
        public int MaxUsage { get; set; }
    }
}
