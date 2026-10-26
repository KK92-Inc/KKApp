// ============================================================================
// Copyright (c) 2026 - W2Inc, All Rights Reserved.
// See README.md in the project root for license information.
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.Backend.Models.Requests.Apply;
using App.Backend.Models.Responses.Entities;
using App.Backend.Models.Responses.Entities.Apply;
using App.Backend.Core.Services.Interface.Apply;

// ============================================================================

namespace App.Backend.API.Controllers;

[ApiController]
[Route("admissions"), Tags("Admissions")]
public class AdmissionController(IAdmissionService service) : Controller
{
    [HttpPost("status")]
    [Authorize(Policy = "staff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    [EndpointSummary("Get where an applicant is in the admissions")]
    [EndpointDescription(@"
Looks an applicant up by email and returns how far they got: CheckIn -> OpenDay -> Trial -> Kickoff.
`next` is the step they still have to complete, `done` once they are in a kickoff or already a student.
    ")]
    public async Task<ActionResult<AdmissionStatusDO>> GetStatus([FromBody] PostAdmissionStatusRequestDTO body, CancellationToken token)
    {
        var progress = await service.GetProgressAsync(body.Email, token);

        return Ok(new AdmissionStatusDO(
            progress.Email,
            progress.Next,
            progress.CheckIn is null ? null : new AdmissionStageDO(progress.CheckIn),
            progress.OpenDay is null ? null : new AdmissionStageDO(progress.OpenDay),
            progress.User?.Id,
            progress.Trial is null ? null : new UserTrialDO(progress.Trial)
        ));
    }
}
