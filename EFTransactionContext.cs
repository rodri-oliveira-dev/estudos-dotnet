public class EFTransactionContext
{
    public MyDbContext Context { get; }
    public DbTransaction Transaction { get; }

    // Bloqueio para evitar commit/rollback simultâneos no mesmo transactionId
    public object LockObj { get; } = new object();

    // Nova propriedade: data/hora de criação
    public DateTime StartTimeUtc { get; } = DateTime.UtcNow;

    public EFTransactionContext(MyDbContext context, DbTransaction transaction)
    {
        Context = context;
        Transaction = transaction;
    }
}
