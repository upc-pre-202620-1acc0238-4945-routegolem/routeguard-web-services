using Microsoft.AspNetCore.Mvc;
using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.Shared.Interfaces.Rest.ProblemDetails;
using RouteGuard.Platform.Stakeholder.Domain.Model;

namespace RouteGuard.Platform.Stakeholder.Interfaces.Rest.Transform;

public static class StakeholderActionResultAssembler
{
    private static int ToStatusCode(StakeholderError error) => error switch
    {
        StakeholderError.ParentNotFound or StakeholderError.DriverNotFound or StakeholderError.StudentGroupNotFound
            => StatusCodes.Status404NotFound,
        StakeholderError.InvalidStudentGroupState => StatusCodes.Status409Conflict,
        StakeholderError.InvalidStakeholderData => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError
    };

    public static IActionResult ToActionResult<T>(
        ControllerBase controller,
        Result<T> result,
        ProblemDetailsFactory problemDetailsFactory,
        Func<T, IActionResult> successAction)
    {
        if (result.IsSuccess) return successAction(result.Value!);
        return problemDetailsFactory.CreateProblemDetails(controller,
            ToStatusCode((StakeholderError)result.Error!), result.Error, result.Message);
    }
}
