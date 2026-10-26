using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace Enfilade.Tests.Hosts;

[Route("/everyone")]
public sealed class EveryonePage : ComponentBase;

[Authorize(Roles = "admin")]
[Route("/admins")]
public sealed class AdminsPage : ComponentBase;

[Authorize(Policy = "admin")]
[Route("/audit")]
public sealed class AuditPage : ComponentBase;

[AllowAnonymous]
[Route("/welcome")]
public sealed class WelcomePage : ComponentBase;
