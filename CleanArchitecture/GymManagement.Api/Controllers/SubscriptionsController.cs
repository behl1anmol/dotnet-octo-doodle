using GymManagement.Application.Subscriptions.Commands;
using GymManagement.Application.Subscriptions.Queries.GetSubscription;
using GymManagement.Contracts.Subscriptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers;
using DomainSubscriptionType = GymManagement.Domain.Subscriptions.SubscriptionType;

[ApiController]
[Route("[controller]")]
public class SubscriptionsController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionsRequest request)
    {
        var command = new CreateSubscriptionCommand(request.SubscriptionType.ToString(), request.AdminId);

        var createSubscriptionResult = await _mediator.Send(command);

        // Handle the result  
        return createSubscriptionResult.MatchFirst(
            subscription => CreatedAtAction(nameof(CreateSubscription), new SubscriptionResponse(subscription.Id, request.SubscriptionType)),
            errors => Problem()
        );
    }
    
    [HttpGet("{subscriptionId:guid}")]
    public async Task<IActionResult> GetSubscription(Guid subscriptionId)
    {
        var query = new GetSubscriptionQuery(subscriptionId);

        var getSubscriptionsResult = await _mediator.Send(query);

        return getSubscriptionsResult.MatchFirst(
            subscription => Ok(new SubscriptionResponse(
                subscription.Id,
                Enum.Parse<SubscriptionType>(subscription.SubscriptionType))),
            error => Problem());
    }
    private static SubscriptionType ToDto(DomainSubscriptionType subscriptionType)
    {
        return subscriptionType.Name switch
        {
            nameof(DomainSubscriptionType.Free) => SubscriptionType.Free,
            nameof(DomainSubscriptionType.Starter) => SubscriptionType.Starter,
            nameof(DomainSubscriptionType.Pro) => SubscriptionType.Pro,
            _ => throw new InvalidOperationException(),
        };
    }
}