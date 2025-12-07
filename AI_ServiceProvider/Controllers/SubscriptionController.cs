using AI_ServiceProvider.Data;
using AI_ServiceProvider.DTOs;
using AI_ServiceProvider.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace AI_ServiceProvider.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public SubscriptionController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            StripeConfiguration.ApiKey = configuration["StripeSettings:SecretKey"];
        }

        // GET: api/Subscription/plans
        [HttpGet("plans")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllPlans()
        {
            var userId = User.FindFirst("id")?.Value;
            Guid? currentSubscriptionId = null;

            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _context.Users.FindAsync(Guid.Parse(userId));
                currentSubscriptionId = user?.SubscriptionId;
            }

            var plans = await _context.Subscriptions
                .Select(s => new SubscriptionDetailsDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    BillingCycle = s.BillingCycle,
                    MaxUsagePerMonth = s.MaxUsagePerMonth,
                    IsCurrentPlan = s.Id == currentSubscriptionId
                })
                .ToListAsync();

            return Ok(plans);
        }

        // GET: api/Subscription/status
        [HttpGet("status")]
        public async Task<IActionResult> GetSubscriptionStatus()
        {
            var userId = Guid.Parse(User.FindFirst("id")?.Value!);
            var user = await _context.Users
                .Include(u => u.Subscription)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found");

            // Calculate usage
            var usageThisMonth = await _context.ImageParserInputs
                .Where(i => i.Chat.UserId == userId &&
                           i.UploadedAt.Month == DateTime.UtcNow.Month &&
                           i.UploadedAt.Year == DateTime.UtcNow.Year)
                .CountAsync();

            var status = new UserSubscriptionStatusDTO
            {
                CurrentPlan = user.Subscription?.Name ?? "No Plan",
                ExpiresAt = user.SubscriptionExpiresAt,
                IsActive = user.SubscriptionExpiresAt == null || user.SubscriptionExpiresAt > DateTime.UtcNow,
                UsageThisMonth = usageThisMonth,
                MaxUsage = user.Subscription?.MaxUsagePerMonth ?? 0
            };

            return Ok(status);
        }

        // POST: api/Subscription/create-checkout-session
        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequestDTO request)
        {
            var userId = Guid.Parse(User.FindFirst("id")?.Value!);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound("User not found");

            var subscription = await _context.Subscriptions.FindAsync(request.SubscriptionId);
            if (subscription == null)
                return NotFound("Subscription plan not found");

            if (subscription.Price == 0)
                return BadRequest("Cannot purchase free tier");

            // Create or retrieve Stripe customer
            if (string.IsNullOrEmpty(user.StripeCustomerId))
            {
                var customerOptions = new CustomerCreateOptions
                {
                    Email = user.Email,
                    Name = user.DisplayName,
                    Metadata = new Dictionary<string, string>
                    {
                        { "UserId", user.Id.ToString() }
                    }
                };

                var customerService = new CustomerService();
                var customer = await customerService.CreateAsync(customerOptions);
                user.StripeCustomerId = customer.Id;
                await _context.SaveChangesAsync();
            }

            // Create Checkout Session
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = subscription.Name,
                                Description = $"{subscription.MaxUsagePerMonth} requests per {subscription.BillingCycle}"
                            },
                            UnitAmount = (long)(subscription.Price * 100),
                            Recurring = new SessionLineItemPriceDataRecurringOptions
                            {
                                Interval = subscription.BillingCycle == "yearly" ? "year" : "month"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "subscription",
                SuccessUrl = $"{Request.Headers["Origin"]}/subscription/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{Request.Headers["Origin"]}/subscription/cancel",
                Customer = user.StripeCustomerId,
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", user.Id.ToString() },
                    { "SubscriptionId", subscription.Id.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return Ok(new CreateCheckoutSessionResponseDTO
            {
                SessionId = session.Id,
                PublishableKey = _configuration["StripeSettings:PublishableKey"]!
            });
        }

        // POST: api/Subscription/webhook
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"];

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    _configuration["StripeSettings:WebhookSecret"]
                );

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    await HandleCheckoutSessionCompleted(session!);
                }
                else if (stripeEvent.Type == "customer.subscription.updated")
                {
                    var subscription = stripeEvent.Data.Object as Stripe.Subscription;
                    await HandleSubscriptionUpdated(subscription!);
                }
                else if (stripeEvent.Type == "customer.subscription.deleted")
                {
                    var subscription = stripeEvent.Data.Object as Stripe.Subscription;
                    await HandleSubscriptionCancelled(subscription!);
                }

                return Ok();
            }
            catch (StripeException)
            {
                return BadRequest();
            }
        }

        private async Task HandleCheckoutSessionCompleted(Session session)
        {
            var userId = Guid.Parse(session.Metadata["UserId"]);
            var subscriptionId = Guid.Parse(session.Metadata["SubscriptionId"]);

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                var subscription = await _context.Subscriptions.FindAsync(subscriptionId);

                user.SubscriptionId = subscriptionId;
                user.StripeSubscriptionId = session.SubscriptionId;

                // Calculate expiration based on billing cycle
                if (subscription?.BillingCycle == "yearly")
                {
                    user.SubscriptionExpiresAt = DateTime.UtcNow.AddYears(1);
                }
                else
                {
                    user.SubscriptionExpiresAt = DateTime.UtcNow.AddMonths(1);
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task HandleSubscriptionUpdated(Stripe.Subscription subscription)
        {
            var user = await _context.Users
                .Include(u => u.Subscription)
                .FirstOrDefaultAsync(u => u.StripeSubscriptionId == subscription.Id);

            if (user != null)
            {
                // Manually calculate next billing date
                if (user.Subscription?.BillingCycle == "yearly")
                {
                    user.SubscriptionExpiresAt = DateTime.UtcNow.AddYears(1);
                }
                else
                {
                    user.SubscriptionExpiresAt = DateTime.UtcNow.AddMonths(1);
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task HandleSubscriptionCancelled(Stripe.Subscription subscription)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.StripeSubscriptionId == subscription.Id);

            if (user != null)
            {
                // Set back to Free Tier
                var freeTier = await _context.Subscriptions
                    .FirstOrDefaultAsync(s => s.Name == "Free Tier");

                user.SubscriptionId = freeTier?.Id;
                user.StripeSubscriptionId = null;
                user.SubscriptionExpiresAt = null;

                await _context.SaveChangesAsync();
            }
        }
    }
}