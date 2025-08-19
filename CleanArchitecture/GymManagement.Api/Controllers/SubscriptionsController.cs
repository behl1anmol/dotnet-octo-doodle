using GymManagement.Application.Subscriptions.Commands;
using GymManagement.Contracts.Subscriptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SubscriptionsController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionsRequest request)
    {
        var command = new CreateSubscriptionCommand(request.SubscriptionType.ToString(), request.AdminId);

        var subscriptionId = await _mediator.Send(command);
        var response = new CreateSubscriptionResponse(subscriptionId, request.SubscriptionType);
        return CreatedAtAction(nameof(CreateSubscription), response);
    }
}