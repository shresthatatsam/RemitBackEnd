using EverestRemit.Api.Data;using EverestRemit.Api.DTOs;using EverestRemit.Api.Entities;using EverestRemit.Api.Interfaces;using Microsoft.EntityFrameworkCore;
namespace EverestRemit.Api.Services;
public sealed class DepositService(ApplicationDbContext db):IDepositService{
public async Task<IReadOnlyList<Deposit>> GetMineAsync(string uid,CancellationToken ct)=>await db.Deposits.AsNoTracking().Where(x=>x.UserId==uid).OrderByDescending(x=>x.CreatedAtUtc).ToListAsync(ct);
public async Task<Deposit> CreateAsync(string uid,CreateDepositRequest r,CancellationToken ct){var x=new Deposit{UserId=uid,Amount=r.Amount,Currency=r.Currency.Trim().ToUpperInvariant(),ReferenceNumber=r.ReferenceNumber.Trim(),ProofUrl=r.ProofUrl};db.Add(x);await db.SaveChangesAsync(ct);return x;}
public async Task<IReadOnlyList<Deposit>> GetAllAsync(CancellationToken ct)=>await db.Deposits.AsNoTracking().OrderBy(x=>x.CreatedAtUtc).ToListAsync(ct);
public async Task<Deposit?> ReviewAsync(int id,ReviewDepositRequest r,CancellationToken ct){var x=await db.Deposits.FindAsync([id],ct);if(x==null)return null;if(x.Status!=DepositStatus.Pending)throw new InvalidOperationException("Only pending deposits can be reviewed.");x.Status=r.Status;x.AdminNote=r.AdminNote;x.ReviewedAtUtc=DateTime.UtcNow;await db.SaveChangesAsync(ct);return x;}}
