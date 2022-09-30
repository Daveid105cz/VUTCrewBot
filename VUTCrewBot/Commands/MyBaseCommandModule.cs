//using DSharpPlus.CommandsNext;
//using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot;
using VUTCrewBot.Misc;
using VUTCrewBot.Repository;

namespace ZbytkyBot.Commands
{

    //[MyModuleLifespan]
    public abstract class MyBaseCommandModule : ApplicationCommandModule
    {
        public IRepositoryFactory RepositoryFactory { get; private set; }
        //protected BotRepository Repo { get; private set; }
        public MyBaseCommandModule(IRepositoryFactory factory)
        {
            RepositoryFactory = factory;
        }
        public async override Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
        {
            //Repo = RepositoryFactory.Create();
            return await base.BeforeSlashExecutionAsync(ctx);
        }
        public async override Task AfterSlashExecutionAsync(InteractionContext ctx)
        {
           // await Repo.DisposeAsync();
            await base.AfterSlashExecutionAsync(ctx);
        }
    }

    //public class NoRepoSave:Attribute
    //{

    //}
    //[AttributeUsage(AttributeTargets.Class, Inherited =true)]
    //public class MyModuleLifespanAttribute : ModuleLifespanAttribute
    //{
    //    public MyModuleLifespanAttribute() : base(ModuleLifespan.Transient)
    //    {
    //    }
    //}
}
