using EverestRemit.Api.Data;
using EverestRemit.Api.DTOs;
using EverestRemit.Api.Entities;
using EverestRemit.Api.Interfaces;
using EverestRemit.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EverestRemit.Api.Controllers;

[ApiController]
[Route("api/kyc")]
[Authorize]
public sealed class KycController : ControllerBase
{
    private readonly IKycService _service;

    public KycController(
        IKycService service)
    {
        _service = service;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();

    // GET api/kyc/me
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var x = await _service.GetMineAsync(CurrentUserId , ct);

        if (x == null)
        {
            return Ok(new
            {
                exists = false,
                status = "NotStarted",
                currentStep = 1
            });
        }

        return Ok(new
        {
            exists = true,
            x.Id,
            status = x.Status.ToString(),
            x.CurrentStep,
            x.FullName,
            x.DateOfBirth,
            x.Nationality,
            x.Address,
            x.DocumentType,
            hasDocumentFront = !string.IsNullOrEmpty(x.DocumentFrontUrl),
            hasDocumentBack = !string.IsNullOrEmpty(x.DocumentBackUrl),
            hasSelfie = !string.IsNullOrEmpty(x.SelfieUrl),
            x.AdminNote,
            x.SubmittedAtUtc,
            x.ReviewedAtUtc,
            canEdit = x.Status is KycStatus.Draft
                or KycStatus.ActionRequired
        });
    }

    // POST api/kyc/steps/personal
    [HttpPost("steps/personal")]
    public async Task<IActionResult> SavePersonal(
        [FromBody] PersonalInformationRequest request,
        CancellationToken ct)
    {
        return await ExecuteAsync(
            () => _service.SavePersonalAsync(
                CurrentUserId, request, ct));
    }

    // POST api/kyc/steps/document
    // multipart/form-data
    [HttpPost("steps/document")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> SaveDocument(
        [FromForm] IdentityDocumentRequest request,
        CancellationToken ct)
    {
        return await ExecuteAsync(
            () => _service.SaveDocumentAsync(
                CurrentUserId, request, ct));
    }

    // POST api/kyc/steps/selfie
    // multipart/form-data
    [HttpPost("steps/selfie")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> SaveSelfie(
        [FromForm] SelfieRequest request,
        CancellationToken ct)
    {
        return await ExecuteAsync(
            () => _service.SaveSelfieAsync(
                CurrentUserId, request, ct));
    }

    // POST api/kyc/submit
    [HttpPost("submit")]
    public async Task<IActionResult> Submit(CancellationToken ct)
    {
        return await ExecuteAsync(
            () => _service.SubmitAsync(CurrentUserId, ct));
    }

    private async Task<IActionResult> ExecuteAsync(
        Func<Task<KycApplication>> action)
    {
        try
        {
            var x = await action();

            return Ok(new
            {
                message = "KYC progress saved successfully.",
                status = x.Status.ToString(),
                currentStep = x.CurrentStep,
                submittedAtUtc = x.SubmittedAtUtc
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}