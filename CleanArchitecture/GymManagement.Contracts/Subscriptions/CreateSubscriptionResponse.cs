namespace GymManagement.Contracts.Subscriptions;

public record CreateSubscriptionResponse (
    Guid Id,
    SubscriptionType SubscriptionType
);
    
