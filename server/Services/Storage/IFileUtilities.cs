using System.Threading.Tasks;

namespace PodNoms.Api.Services.Storage {
    public interface IFileUtilities {
        Task<long> GetRemoteFileSize(string containerName, string fileName);
    }
}