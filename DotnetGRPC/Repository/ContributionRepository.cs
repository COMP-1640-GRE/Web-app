using DotnetGRPC.Model;
using DotnetGRPC.Model.DTO;
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

    public async Task<Dictionary<string, int>> CountContributionsByFaculty()
    {
        var contributionsWithFaculty = _context.Contribution
            .Join(_context.User,
                c => c.DbAuthorId,
                u => u.Id,
                (c, u) => new { Contribution = c, User = u })
            .Join(_context.Faculty,
                cu => cu.User.FacultyId,
                f => f.Id,
                (cu, f) => new { cu.Contribution, cu.User, Faculty = f });

        return await contributionsWithFaculty
            .GroupBy(c => c.Faculty.Name)
            .Select(g => new { Faculty = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Faculty, x => x.Count);
    }

    public async Task<Dictionary<string, int>> CountContributorsByFaculty()
    {
        var contributorsWithFaculty = _context.Contribution
            .Join(_context.User,
                c => c.DbAuthorId,
                u => u.Id,
                (c, u) => new { Contribution = c, User = u })
            .Join(_context.Faculty,
                cu => cu.User.FacultyId,
                f => f.Id,
                (cu, f) => new { cu.Contribution, cu.User, Faculty = f });

        return await contributorsWithFaculty
            .GroupBy(c => c.Faculty.Name)
            .Select(g => new { Faculty = g.Key, Count = g.Select(x => x.User.Id).Distinct().Count() })
            .ToDictionaryAsync(x => x.Faculty, x => x.Count);
    }

    public async Task<Dictionary<string, double>> AverageContributionsByFaculty()
    {
        var contributionsWithFaculty = _context.Contribution
            .Join(_context.User,
                c => c.DbAuthorId,
                u => u.Id,
                (c, u) => new { Contribution = c, User = u })
            .Join(_context.Faculty,
                cu => cu.User.FacultyId,
                f => f.Id,
                (cu, f) => new { cu.Contribution, cu.User, Faculty = f });

        return await contributionsWithFaculty
            .GroupBy(c => c.Faculty.Name)
            .Select(g => new { Faculty = g.Key, Average = g.Count() / (double)g.Select(x => x.User.Id).Distinct().Count() })
            .ToDictionaryAsync(x => x.Faculty, x => x.Average);
    }

    public async Task<List<ContributionTrend>> ContributionTrendsByFaculty()
    {
        var contributionsWithFaculty = _context.Contribution
            .Join(_context.User,
                c => c.DbAuthorId,
                u => u.Id,
                (c, u) => new { Contribution = c, User = u })
            .Join(_context.Faculty,
                cu => cu.User.FacultyId,
                f => f.Id,
                (cu, f) => new { cu.Contribution, cu.User, Faculty = f });

        return await contributionsWithFaculty
            .GroupBy(c => new { c.Faculty.Name, Year = c.Contribution.UpdatedAt.Year, Month = c.Contribution.UpdatedAt.Month })
            .Select(g => new ContributionTrend { Faculty = g.Key.Name, Date = $"{g.Key.Year}-{g.Key.Month}", Count = g.Count() })
            .ToListAsync();
    }
}