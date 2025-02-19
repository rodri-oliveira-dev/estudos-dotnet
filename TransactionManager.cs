public class TransactionManager
{
    private readonly ConcurrentDictionary<Guid, EFTransactionContext> _transactions = new();
    private readonly IServiceProvider _serviceProvider;

    // Novo timer para "varrer" as transações e expirar as que tiverem mais de 10s
    private readonly Timer _cleanupTimer;

    public TransactionManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        // Inicia o timer que chamará CleanupExpiredTransactions() a cada 1 segundo
        _cleanupTimer = new Timer(_ => CleanupExpiredTransactions(), null, 1000, 1000);
    }

    // (demais métodos BeginTransactionAsync, CommitTransactionAsync e RollbackTransactionAsync permanecem inalterados)

    // Novo método para expirar transações com mais de 10s
    private void CleanupExpiredTransactions()
    {
        var agora = DateTime.UtcNow;

        foreach (var kvp in _transactions)
        {
            var txId = kvp.Key;
            var txContext = kvp.Value;
            var elapsed = agora - txContext.StartTimeUtc;

            if (elapsed.TotalSeconds > 10)
            {
                lock (txContext.LockObj)
                {
                    // Se ainda existir, remove e dá rollback
                    if (_transactions.TryRemove(txId, out var removedTxContext))
                    {
                        try
                        {
                            removedTxContext.Context.Database.RollbackTransaction();
                        }
                        catch
                        {
                            // Logar se necessário
                        }
                        finally
                        {
                            removedTxContext.Context.Dispose();
                        }
                    }
                }
            }
        }
    }
}
