using Microsoft.AspNetCore.SignalR;
using TmsApi.Api.Hubs;
using TmsApi.Application.Notifications;

namespace TmsApi.Api.Notifications;

public class SignalRTranscriptNotificationService(
    IHubContext<TmsHub> hubContext)
    : ITranscriptNotificationService
{
    public async Task NotifyTranscriptReadyAsync(
        int studentId,
        string reportId,
        string downloadUrl)
    {
        await hubContext.Clients
            .Group(GroupNames.Student(studentId.ToString()))
            .SendAsync(
                "ReceiveTranscriptReady",
                reportId,
                downloadUrl);
    }
}