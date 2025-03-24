using FDS.CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FDS.CRM.Persistence;

public class CrmDbContext : DbContext, IUnitOfWork, IDataProtectionKeyContext
{
    private readonly ILogger<CrmDbContext> _logger;

    private IDbContextTransaction _dbContextTransaction;

    public DbSet<DataProtectionKey> DataProtectionKeys {  get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<PurchaseTransaction> PurchaseTransactions { get; set; }    
    public DbSet<User> Users { get; set; }   
    public DbSet<Role> Roles { get; set; }   
    public DbSet<UserRole> UserRoles { get; set; }   
    public DbSet<CommonSetting> CommonSettings { get; set; }   
    public DbSet<PaymentTerm> PaymentTerms { get; set; }
    public DbSet<Choice> Choices { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Poll> Polls { get; set; }
    public DbSet<PollAnalytics> PollAnalytics { get; set; }
    public DbSet<PollInvitation> PollInvitations { get; set; }
    public DbSet<VoteDetail> VoteDetails { get; set; }
    public DbSet<VotingHistory> VotingHistories { get; set; }


    public CrmDbContext(DbContextOptions<CrmDbContext> options, ILogger<CrmDbContext> logger)
        : base(options) 
    {
        _logger = logger;
    }

    public async Task<IDisposable> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
    {
        _dbContextTransaction = await Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        return _dbContextTransaction;
    }

    public async Task<IDisposable> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, string lockName = null, CancellationToken cancellationToken = default)
    {
        _dbContextTransaction = await Database.BeginTransactionAsync(isolationLevel, cancellationToken);

        var sqlLock = new SqlDistributedLock(_dbContextTransaction.GetDbTransaction() as SqlTransaction);
        var lockScope = sqlLock.Acquire(lockName);
        if (lockScope == null)
        {
            throw new Exception($"Could not acquire lock: {lockName}");
        }

        return _dbContextTransaction;
    }
    
    public void Configure(EntityTypeBuilder<VoteDetail> builder)
    {
        builder.HasOne(v => v.Question)
            .WithMany()
            .HasForeignKey(v => v.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Choice)
            .WithMany()
            .HasForeignKey(v => v.ChoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _dbContextTransaction.CommitAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new SelectWithoutWhereCommandInterceptor(_logger));
        optionsBuilder.AddInterceptors(new SelectWhereInCommandInterceptor(_logger));
        optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
    }
}
