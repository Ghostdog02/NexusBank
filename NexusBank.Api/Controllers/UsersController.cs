using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusBank.Application.Users.Queries.GetCurrentUser;

namespace NexusBank.Api.Controllers;

[ApiController]
[Route("users")]
[Authorize(Policy = "AuthenticatedUser")]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken ct)
        => Ok(await mediator.Send(new GetCurrentUserQuery(), ct));
}
