namespace mail_provisioner_apply_aliases.Interfaces;

public interface IWorkerTask
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}