namespace RouteGuard.Platform.SubscriptionPlanManagement.Domain.Model;

public enum SubscriptionError
{
    InvalidSubscriptionData,
    PlanNotFound,
    SubscriptionNotFound,
    DatabaseError,
    InternalServerError
}
