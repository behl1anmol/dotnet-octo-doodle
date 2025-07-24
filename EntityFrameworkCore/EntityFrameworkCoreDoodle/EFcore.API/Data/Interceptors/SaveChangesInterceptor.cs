using EFcore.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFcore.API.Data.Interceptors;

public class SaveChangesInterceptor : ISaveChangesInterceptor
{
    //using change tracker to perform logical/soft delete for Genre
    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var context = eventData.Context as MoviesContext;

        if (context is null) return result;
        
        var tracker = context.ChangeTracker;

        var deletedEntries = tracker.Entries<Genre>()
            .Where(entry => entry.State == EntityState.Deleted);
        
        foreach (var entry in deletedEntries)
        {
            entry.Property<bool>("Deleted").CurrentValue = true;
            entry.State = EntityState.Modified;
        }
        
        return result;
    }
    
    public ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = new())
    {
        return ValueTask.FromResult(SavingChanges(eventData, result));
    }
}