using Microsoft.Extensions.Hosting;
using mail_provisioner_apply_aliases.Interfaces;
namespace mail_provisioner_apply_aliases.WorkTasks;

public class Worker : BackgroundService
{
    private IWorkerTask _workerTask;

    public Worker(IWorkerTask workerTask)
    {
        _workerTask = workerTask;

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _workerTask.ExecuteAsync(stoppingToken);
    }
}
