using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_ServiceProvider.Models
{
    public class Subscription
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(50)]
        public string BillingCycle { get; set; } // e.g., "monthly", "yearly"

        public int MaxUsagePerMonth { get; set; } // -1 for unlimited

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}