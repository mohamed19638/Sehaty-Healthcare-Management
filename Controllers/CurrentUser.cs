using System.Security.Claims;

namespace i_am_building_a_simple_graduation.Controllers;

public static class CurrentUser
{
    public static int Id(ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

