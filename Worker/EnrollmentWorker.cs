using System.Diagnostics.Eventing.Reader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tms.Api.Services;
public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EnrollmentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void ProcessBatch()
    {
        using var scope = _scopeFactory.CreateScope();
        var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

        // call your recalculation:
        // enrollmentService.RecalculateScholarships();
    }
}

public class EnrollmentWorkerService : BackgroundService
{
    private readonly EnrollmentWorker _worker;

    public EnrollmentWorkerService(EnrollmentWorker worker)
    {
        _worker = worker;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _worker.ProcessBatch();

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}


