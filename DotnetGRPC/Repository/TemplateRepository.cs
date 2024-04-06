using DotnetGRPC.Model;
using Microsoft.EntityFrameworkCore;

public class TemplateRepository


{
    private readonly AppDbContext _context;

    public TemplateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Template> FindByTemplateCodeAsync(string templateCode)
    {
        return await _context.Template.FirstOrDefaultAsync(t => t.TemplateCode == templateCode);
    }

    // Add other methods as needed
}
