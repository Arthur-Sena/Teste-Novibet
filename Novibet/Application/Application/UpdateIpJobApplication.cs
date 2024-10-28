using Novibet.Repositories.Interfaces;
using Quartz;

namespace Novibet.Application.Services
{
    public class UpdateIpJobApplication : IJob
    {
        private readonly ILogger<UpdateIpJobApplication> _logger;
        private readonly IUpdateIpJobService _updateIpJobRepository;
        public UpdateIpJobApplication(ILogger<UpdateIpJobApplication> logger, IUpdateIpJobService updateIpJobRepository)
        {
            _logger = logger;
            _updateIpJobRepository = updateIpJobRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("IP Update Job started at: {time}", DateTimeOffset.Now);

            try
            {
                await _updateIpJobRepository.UpdateIpInformation();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during IP update job.");
            }

            _logger.LogInformation("IP Update Job completed at: {time}", DateTimeOffset.Now);
        }
    }
}