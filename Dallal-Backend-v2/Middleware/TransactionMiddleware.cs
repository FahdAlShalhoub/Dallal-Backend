namespace Dallal_Backend_v2.Middleware;

public class TransactionMiddleware
{
    private readonly RequestDelegate _next;

    public TransactionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, DatabaseContext dbContext)
    {
        var method = context.Request.Method.ToUpper();
        
        if (method == "GET" || method == "OPTIONS")
        {
            await _next(context);
            return;
        }

        using var transaction = await dbContext.Database.BeginTransactionAsync();
        
        try
        {
            await _next(context);
            
            if (dbContext.ChangeTracker.HasChanges())
            {
                await dbContext.SaveChangesAsync();
            }
            
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}