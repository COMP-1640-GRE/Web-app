using DotnetGRPC.Model;
using Microsoft.EntityFrameworkCore;

public class FacultyRepository
{
    private readonly AppDbContext _context;

    public FacultyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Faculty> FindByIdAsync(long id)
    {
        return await _context.Faculty.FindAsync(id);
    }
}