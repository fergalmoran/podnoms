using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Api.Models.Settings;
using PodNoms.Api.Persistence;
using PodNoms.Api.Services.Storage;

namespace PodNoms.Api.Services.Jobs {
    public class ProcessRemoteAudioFileAttributesJob : IJob {
        private readonly IEntryRepository _entryRepository;
        private readonly IFileUtilities _fileUtilities;
        private readonly ILogger<ProcessRemoteAudioFileAttributesJob> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ProcessRemoteAudioFileAttributesJob(IEntryRepository entryRepository,
                            IFileUtilities fileUtilities, ILogger<ProcessRemoteAudioFileAttributesJob> logger,
                            IUnitOfWork unitOfWork) {
            this._logger = logger;
            this._entryRepository = entryRepository;
            this._fileUtilities = fileUtilities;
            this._unitOfWork = unitOfWork;
        }
        public async Task<bool> Execute() {
            var entries = await _entryRepository.GetAll()
                        .ToListAsync();
            foreach (var entry in entries) {
                string[] parts = entry.AudioUrl.Split("/");

                if (parts.Length == 2) {
                    _logger.LogInformation($"Processing remote: {entry.AudioUrl}");
                    var size = await _fileUtilities.GetRemoteFileSize(
                        parts[0], parts[1]);
                    if (size != -1) {
                        entry.AudioFileSize = size;
                    }
                }
            }
            await _unitOfWork.CompleteAsync();
            return false;
        }
    }
}