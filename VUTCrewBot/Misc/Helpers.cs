using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VUTCrewBot.Misc;
using VUTCrewBot.Repository;

namespace VUTCrewBot
{
    public static class Tasc
    {
        public static Task WaitUntil(DateTime date, CancellationToken cancellationToken)
        {
            if (date <= DateTime.Now)
            {
                return Task.CompletedTask;
            }
            TimeSpan until = date - DateTime.Now;
            return Task.Delay(until,cancellationToken);
        }
        public static Task WaitUntil(DateTime date)
        {
            if (date <= DateTime.Now)
            {
                return Task.CompletedTask;
            }
            TimeSpan until = date - DateTime.Now;
            return Task.Delay(until);
        }
    }
    public static class Helpers
    {
        public async static Task Think(this InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        }
        public async static Task Respond(this InteractionContext ctx, String message)
        {
            
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(message));
        }
        public async static Task RespondError(this InteractionContext ctx, String message)
        {
            await ctx.Respond("ERR: " + message);
        }
        public static bool ToId(this String meet, out int value)
        {
            if(meet.StartsWith("meet_"))
            {
                var split = meet.Split("_");
                if(split.Length==2)
                {
                    if (int.TryParse(split[1], out var pp))
                    {
                        value = pp;
                        return true;
                    }
                }
            }
            value = -1;
            return false;
        }
        public static bool ToTimeOfDay(this String time, out TimeSpan value)
        {
            string pattern = @"^([0-9]|0[0-9]|1[0-9]|2[0-3]):([0-5][0-9])$";

            Match m = Regex.Match(time, pattern);
            if(m.Success)
            {
                int h = int.Parse(m.Groups[1].Value);
                int mm = int.Parse(m.Groups[2].Value);
                value = new TimeSpan(h, mm, 0);
                return true;
            }
            
            value = TimeSpan.Zero;
            return false;
        }
        public static bool ToFullDate(this String time, out DateTime value)
        {
            //var csCulture = CultureInfo.CreateSpecificCulture("cs-CZ");
            try
            {
                var tmp = DateTime.ParseExact(time, "d.M.yyyy H:mm", null);
                //var tmp = DateTime.Parse(time,, CultureInfo.InvariantCulture);
                value = new DateTime(tmp.Year, tmp.Month, tmp.Day, tmp.Hour, tmp.Minute, 0);
                return true;
            }
            catch (Exception e)
            {

            }
            //if (DateTime.TryParseExact(time,"dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture,DateTimeStyles.None,out var tmp))
            //{
            //    value = new DateTime(tmp.Year, tmp.Month, tmp.Day, tmp.Hour, tmp.Minute, 0);
            //    return true;
            //}
            value = DateTime.MinValue;
            return false;
        }
        public async static Task<DiscordChannel?> GetChannelByIds(this DiscordClient client, Tuple<ulong,ulong>? ids)
        {
            if (ids == null)
                return null;
            return await client.GetChannelAsync(ids.Item2);
        }
        /*public static void RegisterProvider<TInterface, TClass>(this IServiceCollection collection) 
            where TInterface : class, IProvider
            where TClass : class, TInterface
        {
            collection.AddSingleton<IProviderFactory<TInterface>, GenericProviderFactory<TInterface>>();
            collection.AddTransient<TInterface, TClass>();
        }*/

    }
}
