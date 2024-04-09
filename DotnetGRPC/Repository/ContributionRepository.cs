using DotnetGRPC.Model;
using Microsoft.EntityFrameworkCore;

public class ContributionRepository
{
    private readonly AppDbContext _context;

    public ContributionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Contribution>> FindPendingContributions14DaysAgo()
    {
        var fourteenDaysAgo = DateTime.UtcNow.AddDays(-14);

        return await _context.Contribution
            .Where(c => c.UpdatedAt < fourteenDaysAgo && c.Status == "pending")
            .ToListAsync();
    }

}