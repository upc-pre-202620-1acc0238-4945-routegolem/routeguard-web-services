namespace RouteGuard.Platform.SubscriptionPlanManagement.Interfaces.Rest.Resources;

public record CreatePaymentIntentResource(Guid OrganizationId, Guid PlanId);

public record PaymentIntentResource(string ClientSecret, string PaymentIntentId, long Amount, string Currency);
