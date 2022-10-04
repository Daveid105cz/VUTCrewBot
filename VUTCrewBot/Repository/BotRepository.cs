using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.DAL;

namespace VUTCrewBot.Repository
{
    public sealed class BotRepository : IDisposable, IAsyncDisposable
    {
        private BotDbContext Context { get; }
        private List<RepositoryBase> Repositories { get; } = new();

        public BotRepository(IBotDbContextFactory dbFactory)
        {
            Context = dbFactory.CreateDbContext();
        }

        public IMeetRepository Meet => GetOrCreateRepository<MeetRepository>();
        public IMeetTemplateRepository Templates => GetOrCreateRepository<MeetTemplateRepository>();
        public ISettingsRepository Settings => GetOrCreateRepository<SettingsRepository>();

        //public UserRepository User => GetOrCreateRepository<UserRepository>();

        private TRepository GetOrCreateRepository<TRepository>() where TRepository : RepositoryBase
        {
            var repository = Repositories.OfType<TRepository>().FirstOrDefault();
            if (repository != null)
                return repository;

            repository = Activator.CreateInstance(typeof(TRepository), Context) as TRepository;
            if (repository == null)
                throw new InvalidOperationException($"Error while creating repository {typeof(TRepository).Name}");

            Repositories.Add(repository);

            return repository;
        }

        public Task AddAsync<TEntity>(TEntity entity) where TEntity : class
            => Context.Set<TEntity>().AddAsync(entity).AsTask();

        public Task AddCollectionAsync<TEntity>(IEnumerable<TEntity> collection) where TEntity : class
            => Context.Set<TEntity>().AddRangeAsync(collection);

        public void Remove<TEntity>(TEntity entity) where TEntity : class
            => Context.Set<TEntity>().Remove(entity);

        public void RemoveCollection<TEntity>(IEnumerable<TEntity> collection) where TEntity : class
        {
            var enumerable = collection as List<TEntity> ?? collection.ToList();
            if (enumerable.Count == 0)
                return;

            Context.Set<TEntity>().RemoveRange(enumerable);
        }

        public async Task<int> CommitAsync()
        {
            return await Context.SaveChangesAsync();
        }

        public void ClearChangeTracker()
        {
            Context.ChangeTracker.Clear();
        }

        public void ProcessMigrations()
        {
            if (Context.Database.GetPendingMigrations().Any())
                Context.Database.Migrate();

        }

        public async Task ProcessMigrationsAsync()
        {
            if ((await Context.Database.GetPendingMigrationsAsync()).Any())
                await Context.Database.MigrateAsync();
        }

        public void Dispose()
        {
            Context.Dispose();
            Repositories.Clear();
        }

        public ValueTask DisposeAsync()
        {
            Repositories.Clear();
            return Context.DisposeAsync();
        }
    }

}
