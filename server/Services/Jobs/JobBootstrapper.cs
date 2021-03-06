using System;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Api.Models;
using PodNoms.Api.Persistence;

namespace PodNoms.Api.Services.Jobs {
    public static class JobBootstrapper {
        public static void BootstrapJobs() {
            RecurringJob.AddOrUpdate<ClearOrphanAudioJob>(x => x.Execute(), Cron.Daily(1));
            RecurringJob.AddOrUpdate<UpdateYouTubeDlJob>(x => x.Execute(), Cron.Daily(1, 30));
            BackgroundJob.Schedule<ProcessPlaylistsJob>(x => x.Execute(3), TimeSpan.FromSeconds(1));
            RecurringJob.AddOrUpdate<ProcessPlaylistsJob>(x => x.Execute(), Cron.Daily(2));

            BackgroundJob.Schedule<ProcessRemoteAudioFileAttributesJob>(
                x => x.Execute(), 
                TimeSpan.FromSeconds(Int16.MaxValue));
        }
    }
}
