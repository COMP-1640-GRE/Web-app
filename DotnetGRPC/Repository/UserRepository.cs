using DotnetGRPC.Model;
using Microsoft.EntityFrameworkCore;

public class UserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> FindByIdAsync(long id)
    {
        return await _context.User.SingleOrDefaultAsync(user => user.Id == id);
    }

    public async Task<List<User>> FindFacultyCoordinators(long facultyId)
    {
        return await _context.User
            .Where(user => user.FacultyId == facultyId && user.Role == "faculty_marketing_coordinator")
            .ToListAsync();
    }
}
