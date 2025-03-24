public class AuditLog
{
    public int Id { get; set; }
    public DateTime ActionDate { get; set; } = DateTime.UtcNow;
    public string Url { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Created, Updated, Deleted
    public string EntityName { get; set; } = string.Empty;
    public string CurrentValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
}


public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<int> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if (dbContext == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditLogs = new List<AuditLog>();

        var url = _httpContextAccessor.HttpContext?.Request.Path.Value ?? "Unknown URL";

        foreach (var entry in dbContext.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog) continue; // Evita logar a própria tabela de auditoria

            var log = new AuditLog
            {
                Url = url,
                EntityName = entry.Entity.GetType().Name,
                Action = entry.State.ToString(),
                CurrentValue = entry.State == EntityState.Modified || entry.State == EntityState.Deleted
                    ? JsonSerializer.Serialize(entry.OriginalValues.Properties.ToDictionary(p => p.Name, p => entry.OriginalValues[p]))
                    : "{}",
                NewValue = entry.State == EntityState.Added || entry.State == EntityState.Modified
                    ? JsonSerializer.Serialize(entry.CurrentValues.Properties.ToDictionary(p => p.Name, p => entry.CurrentValues[p]))
                    : "{}"
            };

            auditLogs.Add(log);
        }

        dbContext.Set<AuditLog>().AddRange(auditLogs);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}