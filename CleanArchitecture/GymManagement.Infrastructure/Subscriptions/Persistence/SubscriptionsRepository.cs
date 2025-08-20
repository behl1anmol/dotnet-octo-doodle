using GymManagement.Application.Common;
using GymManagement.Application.Common.Interfaces;
using GymManagement.Domain.Subscriptions;

namespace GymManagement.Infrastructure.Subscriptions.Persistence;

public class SubscriptionsRepository : ISubscriptionsRepository
{
    private static readonly List<Subscription> _subscriptions = new();
    public Task AddSubscriptionAsync(Subscription subscription)
    {
        _subscriptions.Add(subscription);
        return Task.CompletedTask;
    }
    public Task<Subscription?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(_subscriptions.FirstOrDefault(s => s.Id == id));
    }
}