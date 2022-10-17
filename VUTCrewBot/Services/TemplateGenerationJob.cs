using BotToolkit.BaseTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.DAL.Entities;
using VUTCrewBot.Models;

namespace VUTCrewBot.Services
{
    public class TemplateGenerationJob : IMyJob
    {
        public TemplateService TemplateService { get; set; }
        public MeetService MeetService { get; set; }
        ILogger Logger { get; set; }
        public TemplateGenerationJob(TemplateService templateService, MeetService meetService, ILogger<TemplateGenerationJob> logger)
        {
            TemplateService = templateService;
            MeetService = meetService;
            Logger = logger;
        }
        //public static String DateFormat = "dd.MM.yyyy HH:mm";
        public async Task RunAsync()
        {
            var templates = await TemplateService.GetTemplatesForTomorrow();
            var tomorrowDay = templates.Item1;
            foreach (var template in templates.Item2)
            {
                DateTime dateTime = new DateTime(tomorrowDay.Year, tomorrowDay.Month, tomorrowDay.Day,
                        template.MeetupDayTime.Hours, template.MeetupDayTime.Minutes, 0);

                await MeetService.CreateMeetAsync(template.Name,
                    dateTime,
                    template.Users.Select(e => e.UserId).ToList()
                    );
                Logger.LogInformation($"Generated a meet {template.Name}:{dateTime:dd.MM.yyyy HH:mm} from template ID {template.Id}");
            }
        }
    }
}
