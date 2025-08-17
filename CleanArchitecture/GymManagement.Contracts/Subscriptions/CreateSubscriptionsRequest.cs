namespace GymManagement.Contracts.Subscriptions;

public record CreateSubscriptionsRequest(
    SubscriptionType SubscriptionType,
    Guid AdminId
);