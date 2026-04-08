using Microsoft.EntityFrameworkCore;
using WorkExchange_API.Models;

namespace WorkExchange_API.Services;

public class BlacklistService
{
    private readonly WorkExchangeDBContext _db;

    public BlacklistService(WorkExchangeDBContext db)
    {
        _db = db;
    }

    // 單向：blocker 是否封鎖 blocked
    public Task<bool> IsBlockedAsync(int blockerId, int blockedId)
        => _db.Blacklists.AnyAsync(x => x.BlockerId == blockerId && x.BlockedId == blockedId);

    public async Task<List<int>> GetBlockedIdsAsync(int userId)
    {
        return await _db.Blacklists
            .Where(b => b.BlockerId == userId)
            .Select(b => b.BlockedId)
            .ToListAsync();
    }
    // 封鎖（冪等：重複封鎖不會爆）
    public async Task BlockAsync(int blockerId, int blockedId)
    {
        if (blockerId == blockedId) return;

        var exists = await IsBlockedAsync(blockerId, blockedId);
        if (exists) return;

        _db.Blacklists.Add(new Blacklists
        {
            BlockerId = blockerId,
            BlockedId = blockedId,
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync();
    }

    // 解除封鎖（冪等：沒有資料也當成功）
    public async Task UnblockAsync(int blockerId, int blockedId)
    {
        var row = await _db.Blacklists
            .FirstOrDefaultAsync(x => x.BlockerId == blockerId && x.BlockedId == blockedId);

        if (row == null) return;

        _db.Blacklists.Remove(row);
        await _db.SaveChangesAsync();
    }
}
