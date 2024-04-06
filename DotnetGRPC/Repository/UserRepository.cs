using DotnetGRPC.Model;

public class UserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> FindByIdAsync(long id)
    {
        return await _context.User.FindAsync(id);
    }
}
