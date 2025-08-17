using GymManagement.Application.Services;
using GymManagement.Contracts.Subscriptions;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SubscriptionsController(ISubscriptionsWriteService subscriptionsService) : ControllerBase
{
    private readonly ISubscriptionsWriteService _subscriptionsService = subscriptionsService;

    [HttpPost]
    public IActionResult CreateSubscription([FromBody] CreateSubscriptionsRequest request)
    {
        var subscriptionId = _subscriptionsService.CreateSubscription(request.SubscriptionType.ToString(), request.AdminId);
        var response = new CreateSubscriptionResponse(subscriptionId, request.SubscriptionType);
        return CreatedAtAction(nameof(CreateSubscription), response);
    }
}