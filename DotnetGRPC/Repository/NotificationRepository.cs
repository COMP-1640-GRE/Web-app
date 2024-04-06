using DotnetGRPC.Model;
using Microsoft.EntityFrameworkCore;

public class NotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> FindByIdAsync(long id)
    {
        return await _context.Notification.FindAsync(id);
    }

    public async Task SaveAsync(Notification notification)
    {
        notification.CreatedAt = notification.CreatedAt.ToUniversalTime();
        _context.Notification.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetNotifications(long userId, bool seen)
    {
        return await _context.Notification
            .Where(n => n.User.Id == userId && n.Seen == seen)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }
}
