using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.BotToolkit;
using VUTCrewBot.Services;

namespace VUTCrewBot.Misc
{
    public static class ServiceCollectionHelpers
    {
        public static void RegisterAllJobs(this IServiceCollection services, Assembly assembly)
        {
            var types = assembly.ExportedTypes.Where((c) => 
            {
                bool pp = c.IsClass && c.GetInterfaces().Contains(typeof(IMyJob));
                return pp;
            }).ToList();


            foreach (var type in types)
            {
                services.AddTransient(type);
            }
        }
        public static void RegisterAllServices(this IServiceCollection services, Assembly assembly)
        {
            var types = assembly.ExportedTypes.Where(c => c.IsClass && c.IsSubclassOf(typeof(BotService)));

            foreach (var type in types)
            {
                
                services.AddSingleton(type);
            }
        }
    }
}
