using MafiaServer.Pages.GamePlayLogic;

namespace MafiaServer.Pages;

using Microsoft.AspNetCore.SignalR;

public class MafiaGamePlayServer : Hub
{
    // private static readonly Dictionary<string, string?> ActivePlayers = new();
    private readonly GameStartProcedures _gameStartProcedures;
    
    public MafiaGamePlayServer(GameStartProcedures gameStartProcedures)
    {
        _gameStartProcedures = gameStartProcedures;
        Console.WriteLine("Test MafiaGamePlayServer");
    }

    public async Task NewConnectionPlayerId(string user, string message)
    {
        System.Console.WriteLine($"1ActivePlayers count is " + _gameStartProcedures.ActivePlayers.Count);
        System.Console.WriteLine($"Received message from {user}: {message}");
        if (_gameStartProcedures.ActivePlayers.ContainsKey(user) && _gameStartProcedures.ActivePlayers[user] != null)
        {
            System.Console.WriteLine($"Found User");
            await DuplicatesProcedure(_gameStartProcedures.ActivePlayers[user]);
        }
        else
        {
            System.Console.WriteLine($"Not Found but new");
            _gameStartProcedures.ActivePlayers.Add(user, Context.ConnectionId);
            System.Console.WriteLine($"2ActivePlayers count is " + _gameStartProcedures.ActivePlayers.Count);
        }
    }

    // public override async Task OnConnectedAsync()
    // {
    //     return;
    //     if (!GameDataCenter.GameStarted)
    //     {
    //         await _gameStartProcedures.SendStartMsg();
    //         GameDataCenter.GameStarted = true;
    //         return;
    //     }
    //
    //     // Optionally, handle any setup logic when a client connects
    //     System.Console.WriteLine($"Client connected: {Context.ConnectionId}");
    //     await base.OnConnectedAsync();
    // }

    // This method is called when a client disconnects
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        // Handle disconnection logic here
        System.Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        if (exception != null)
        {
            System.Console.WriteLine($"Disconnection error: {exception.Message}");
        }

        // Perform any cleanup or notify other clients about the disconnection
        await base.OnDisconnectedAsync(exception);
    }

    public Task DuplicatesProcedure(string userId)
    {
        // Create a new HubConnectionContext to abort the specific connection
        var connection = Clients.Client(userId) as HubCallerContext;
        if (connection != null)
        {
            connection.Abort();
            System.Console.WriteLine($"Connection found and Aborted");
        }
        else
        {
            System.Console.WriteLine($"Connection not found");
        }

        System.Console.WriteLine($"DisconnectProcedure Done");
        return Task.CompletedTask;
    }

    public async Task ReconnectUser(string userId)
    {
        string connectionId = Context.ConnectionId;
        // Reassociate the user ID with the new connection ID
        System.Console.WriteLine($"User {userId} reconnected with connection ID: {connectionId}");
        // Optionally, validate and restore any session-specific data
    }
}