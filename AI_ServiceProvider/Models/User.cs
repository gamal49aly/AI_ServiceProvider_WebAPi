using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_ServiceProvider.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        [MaxLength(100)]
        public string DisplayName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? SubscriptionId { get; set; }

        [ForeignKey("SubscriptionId")]
        public virtual Subscription? Subscription { get; set; }

        // Stripe Integration
        [MaxLength(100)]
        public string? StripeCustomerId { get; set; }
        [MaxLength(100)]
        public string? StripeSubscriptionId { get; set; }
        public DateTime? SubscriptionExpiresAt { get; set; }

        public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
    }
}