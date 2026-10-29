using Microsoft.EntityFrameworkCore;
using SalonApi.Data;
using SalonApi.Models;

namespace SalonApi.Services;

/// <summary>Reads and edits the text of the Home and Contact pages.</summary>
public class ContentService(SalonDbContext db)
{
    public static readonly string[] AllowedKeys = ["home", "contact"];

    public Task<ContentPage?> GetAsync(string key) =>
        db.ContentPages.AsNoTracking().FirstOrDefaultAsync(p => p.Key == key);

    /// <summary>Change the title and body. Returns null when the key is not a real page.</summary>
    public async Task<ContentPage?> UpdateAsync(string key, UpdateContentRequest request)
    {
        var page = await db.ContentPages.FindAsync(key);   // tracked, so EF notices our edits
        if (page is null) return null;

        page.Title = request.Title.Trim();
        page.Body = request.Body.Trim();
        await db.SaveChangesAsync();                       // writes an UPDATE for just the changed columns
        return page;
    }
}
