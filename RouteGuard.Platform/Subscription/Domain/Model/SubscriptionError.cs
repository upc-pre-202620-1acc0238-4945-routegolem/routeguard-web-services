namespace RouteGuard.Platform.Subscription.Domain.Model;

public enum SubscriptionError
{
    InvalidSubscriptionData,
    PlanNotFound,
    SubscriptionNotFound,
    DatabaseError,
    InternalServerError
}
