using System.Threading.Tasks;

namespace PodNoms.Api.Services.Jobs {
    public interface IJob {
        Task<bool> Execute();
    }
}