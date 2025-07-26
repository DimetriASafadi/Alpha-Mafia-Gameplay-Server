using MafiaServer.Pages.GamePlayLogic;

namespace MafiaServer.Pages.StartUpServices;

public class StartUpListeners : IHostedService
{
    // public GameLifeCycle
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("GameInitializationService started. Performing initial setup...");
        // Perform initialization tasks here
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("GameInitializationService stopping.");
        return Task.CompletedTask;
    }
}