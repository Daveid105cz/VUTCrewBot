using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.DAL;
using VUTCrewBot.Repository;

namespace VUTCrewBot
{
    public interface IRepositoryFactory
    {
        public BotRepository Create();
    }
    public class RepositoryFactory : IRepositoryFactory
    {
        IServiceProvider provider;
        public RepositoryFactory(IServiceProvider provider)
        {
            this.provider = provider;
        }
        public BotRepository Create()
        {
            return provider.GetService<BotRepository>();
        }
    }
    /*public interface IProviderFactory<T> where T: class, IProvider
    {
        public Task<T> Create();
    }
    public class GenericProviderFactory<T> : IProviderFactory<T> where T : class, IProvider
    {
        private IServiceProvider _serviceProvider;
        public GenericProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task<T> Create()
        {
            if( _serviceProvider == null )
            {
                throw new NullReferenceException("ServiceProvider not set");
            }
            var service = _serviceProvider.GetService<T>();
            if(service == null )
            {
                throw new InvalidOperationException("Service not found in ServiceProvider");
            }
            await service.CreateContext();
            return service;
        }
    }*/
}
