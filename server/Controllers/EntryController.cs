using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Api.Models;
using PodNoms.Api.Models.Settings;
using PodNoms.Api.Models.ViewModels;
using PodNoms.Api.Persistence;
using PodNoms.Api.Services;
using PodNoms.Api.Services.Auth;
using PodNoms.Api.Services.Jobs;
using PodNoms.Api.Services.Processor;
using PodNoms.Api.Services.Storage;
using PodNoms.Api.Utils.RemoteParsers;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize]
    public class EntryController : BaseAuthController {
        private readonly IPodcastRepository _podcastRepository;
        private readonly IEntryRepository _repository;

        public IConfiguration _options { get; }

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUrlProcessService _processor;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger _logger;
        private readonly AudioFileStorageSettings _audioFileStorageSettings;
        private readonly StorageSettings _storageSettings;

        public EntryController(IEntryRepository repository,
            IPodcastRepository podcastRepository,
            IUnitOfWork unitOfWork, IMapper mapper, IOptions<StorageSettings> storageSettings,
            IOptions<AudioFileStorageSettings> audioFileStorageSettings,
            IConfiguration options,
            IUrlProcessService processor, ILoggerFactory logger,
            UserManager<ApplicationUser> userManager,
            IHostingEnvironment hostingEnvironment,
            IHttpContextAccessor contextAccessor) : base(contextAccessor, userManager) {
            this._logger = logger.CreateLogger<EntryController>();
            this._podcastRepository = podcastRepository;
            this._repository = repository;
            this._options = options;
            this._storageSettings = storageSettings.Value;
            this._unitOfWork = unitOfWork;
            this._audioFileStorageSettings = audioFileStorageSettings.Value;
            this._mapper = mapper;
            this._processor = processor;
            this._hostingEnvironment = hostingEnvironment;
        }

        private void _processEntry(PodcastEntry entry) {
            try {
                var extractJobId = BackgroundJob.Enqueue<IUrlProcessService>(
                    service => service.DownloadAudio(entry.Id));
                var uploadJobId = BackgroundJob.ContinueWith<IAudioUploadProcessService>(
                    extractJobId, service => service.UploadAudio(entry.Id, entry.AudioUrl));
                var notify = BackgroundJob.ContinueWith<INotifyJobCompleteService>(
                    uploadJobId, service => service.NotifyUser(entry.Podcast.AppUser.Id, "PodNoms", $"{entry.Title} has finished processing",
                    entry.Podcast.GetThumbnailUrl(
                        this._options.GetSection("Storage")["CdnUrl"],
                        this._options.GetSection("ImageFileStorageSettings")["ContainerName"])
                    ));
            } catch (InvalidOperationException ex) {
                _logger.LogError($"Failed submitting job to processor\n{ex.Message}");
                entry.ProcessingStatus = ProcessingStatus.Failed;
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllForUser() {
            var entries = await _repository.GetAllForUserAsync(_applicationUser.Id);
            var results = _mapper.Map<List<PodcastEntry>, List<PodcastEntryViewModel>>(
                entries.OrderByDescending(e => e.Id).ToList()
            );
            return Ok(results);
        }

        [HttpGet("all/{podcastSlug}")]
        public async Task<IActionResult> GetAllForSlug(string podcastSlug) {
            var entries = await _repository.GetAllForSlugAsync(podcastSlug);
            var results = _mapper.Map<List<PodcastEntry>, List<PodcastEntryViewModel>>(entries.ToList());

            return Ok(results);
        }

        [HttpPost]
        public async Task<ActionResult<PodcastEntryViewModel>> Post([FromBody] PodcastEntryViewModel item) {

            // first check url is valid
            var entry = _mapper.Map<PodcastEntryViewModel, PodcastEntry>(item);
            var status = await _processor.GetInformation(entry);
            if (status == AudioType.Valid) {
                if (entry.ProcessingStatus == ProcessingStatus.Processing) {
                    if (string.IsNullOrEmpty(entry.ImageUrl)) {
                        entry.ImageUrl = $"{_storageSettings.CdnUrl}static/images/default-entry.png";
                    }
                    entry.Processed = false;
                    _repository.AddOrUpdate(entry);
                    bool succeeded = await _unitOfWork.CompleteAsync();
                    await _repository.LoadPodcastAsync(entry);
                    if (succeeded) {
                        _processEntry(entry);
                        var result = _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);
                        return result;
                    }
                }
            } else if ((status == AudioType.Playlist && YouTubeParser.ValidateUrl(item.SourceUrl))
                        || MixcloudParser.ValidateUrl(item.SourceUrl))  {
                entry.ProcessingStatus = ProcessingStatus.Deferred;
                var result = _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);
                return Accepted(result);
            }
            return BadRequest("Failed to create podcast entry");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) {
            await this._repository.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            return Ok();
        }
        [HttpPost("/preprocess")]
        public async Task<ActionResult<PodcastEntryViewModel>> PreProcess(PodcastEntryViewModel item) {
            var entry = await _repository.GetAsync(item.Id);
            entry.ProcessingStatus = ProcessingStatus.Accepted;
            var response = _processor.GetInformation(item.Id);
            entry.ProcessingStatus = ProcessingStatus.Processing;
            await _unitOfWork.CompleteAsync();

            var result = _mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry);
            return result;
        }

        [HttpPost("resubmit")]
        public async Task<IActionResult> ReSubmit([FromBody] PodcastEntryViewModel item) {
            var entry = await _repository.GetAsync(item.Id);
            entry.ProcessingStatus = ProcessingStatus.Processing;
            await _unitOfWork.CompleteAsync();
            if (entry.ProcessingStatus != ProcessingStatus.Processed) {
                _processEntry(entry);
            }
            return Ok(entry);
        }
    }
}
