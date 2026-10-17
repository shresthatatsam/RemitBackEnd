using EverestRemit.Api.Data;
using EverestRemit.Api.DTOs;
using EverestRemit.Api.Entities;
using EverestRemit.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace EverestRemit.Api.Services;

public sealed class KycService : IKycService
{
    private readonly ApplicationDbContext _db;
    private readonly KycFileStorage _storage;

    public KycService(ApplicationDbContext db, KycFileStorage storage)
    {
        this._db = db;
        this._storage = storage;
    }


    private async Task<KycApplication> GetOrCreateAsync(string userId, CancellationToken ct)
    {
        var application = await _db.KycApplications
            .SingleOrDefaultAsync(x => x.UserId == userId, ct);

        if (application != null)
            return application;

        application = new KycApplication
        {
            UserId = userId,
            Status = KycStatus.Draft,
            CurrentStep = 1
        };

        _db.KycApplications.Add(application);
        await _db.SaveChangesAsync(ct);

        return application;
    }

    private static void EnsureEditable(KycApplication x)
    {
        if (x.Status is KycStatus.Approved
            or KycStatus.Pending
            or KycStatus.UnderReview
            or KycStatus.Rejected)
        {
            throw new InvalidOperationException(
                "This KYC application cannot currently be edited.");
        }
    }

    private static void MarkUpdated(KycApplication x)
    {
        x.Status = KycStatus.Draft;
        x.AdminNote = null;
        x.ReviewedAtUtc = null;
        x.UpdatedAtUtc = DateTime.UtcNow;
    }

    // STEP 1: Personal information
    public async Task<KycApplication> SavePersonalAsync(string userId, PersonalInformationRequest request, CancellationToken ct)
    {
        var x = await GetOrCreateAsync(userId, ct);
        EnsureEditable(x);

        if (request.DateOfBirth >=
            DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException(
                "Date of birth must be in the past.");
        }

        x.FullName = request.FullName.Trim();
        x.DateOfBirth = request.DateOfBirth;
        x.Nationality = request.Nationality.Trim();
        x.Address = request.Address?.Trim();

        MarkUpdated(x);
        x.CurrentStep = 2;

        await _db.SaveChangesAsync(ct);
        return x;
    }

    // STEP 2: Identity document and photo uploads
    public async Task<KycApplication> SaveDocumentAsync(string userId, IdentityDocumentRequest request, CancellationToken ct)
    {
        var x = await GetOrCreateAsync(userId, ct);
        EnsureEditable(x);

        if (string.IsNullOrWhiteSpace(x.FullName) ||
            x.DateOfBirth == default ||
            string.IsNullOrWhiteSpace(x.Nationality))
        {
            throw new InvalidOperationException(
                "Complete personal information first.");
        }

        var frontKey = await _storage.SaveAsync(
            request.DocumentFront, allowPdf: true, ct);

        string? backKey = null;

        if (request.DocumentBack is { Length: > 0 })
        {
            backKey = await _storage.SaveAsync(
                request.DocumentBack, allowPdf: true, ct);
        }

        x.DocumentType = request.DocumentType.Trim();
        x.DocumentNumber = request.DocumentNumber.Trim();
        x.DocumentFrontUrl = frontKey;
        x.DocumentBackUrl = backKey;

        MarkUpdated(x);
        x.CurrentStep = 3;

        await _db.SaveChangesAsync(ct);
        return x;
    }

    // STEP 3: Selfie photo
    public async Task<KycApplication> SaveSelfieAsync(string userId, SelfieRequest request, CancellationToken ct)
    {
        var x = await GetOrCreateAsync(userId, ct);
        EnsureEditable(x);

        if (string.IsNullOrWhiteSpace(x.DocumentType) ||
            string.IsNullOrWhiteSpace(x.DocumentNumber) ||
            string.IsNullOrWhiteSpace(x.DocumentFrontUrl))
        {
            throw new InvalidOperationException(
                "Upload your identity document first.");
        }

        var selfieKey = await _storage.SaveAsync(
            request.Selfie, allowPdf: false, ct);

        x.SelfieUrl = selfieKey;

        MarkUpdated(x);
        x.CurrentStep = 4;

        await _db.SaveChangesAsync(ct);
        return x;
    }

    // STEP 4: Final submission
    public async Task<KycApplication> SubmitAsync(string userId, CancellationToken ct)
    {
        var x = await GetOrCreateAsync(userId, ct);
        EnsureEditable(x);

        if (string.IsNullOrWhiteSpace(x.FullName) ||
            x.DateOfBirth == default ||
            string.IsNullOrWhiteSpace(x.Nationality))
        {
            throw new InvalidOperationException(
                "Personal information is incomplete.");
        }

        if (string.IsNullOrWhiteSpace(x.DocumentType) ||
            string.IsNullOrWhiteSpace(x.DocumentNumber) ||
            string.IsNullOrWhiteSpace(x.DocumentFrontUrl) ||
            string.IsNullOrWhiteSpace(x.SelfieUrl))
        {
            throw new InvalidOperationException(
                "Identity document and selfie are required.");
        }

        x.Status = KycStatus.Pending;
        x.CurrentStep = 4;
        x.AdminNote = null;
        x.SubmittedAtUtc = DateTime.UtcNow;
        x.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return x;
    }

    public Task<KycApplication?> GetMineAsync(string uid, CancellationToken ct) => _db.KycApplications.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == uid, ct);
    public async Task<IReadOnlyList<KycApplication>> GetAllAsync(CancellationToken ct) => await _db.KycApplications.AsNoTracking().OrderBy(x => x.SubmittedAtUtc).ToListAsync(ct);
    public async Task<KycApplication?> ReviewAsync(int id, ReviewKycRequest r, CancellationToken ct) { var x = await _db.KycApplications.FindAsync([id], ct); if (x == null) return null; if (r.Status == KycStatus.Pending) throw new InvalidOperationException("Review status cannot be Pending."); x.Status = r.Status; x.AdminNote = r.AdminNote; x.ReviewedAtUtc = DateTime.UtcNow; await _db.SaveChangesAsync(ct); return x; }


}
