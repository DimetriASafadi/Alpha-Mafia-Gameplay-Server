using MafiaServer.Pages.LobbySection.Models;
using Microsoft.AspNetCore.SignalR;

namespace MafiaServer.Pages.GamePlayLogic;

public class GameStartProcedures
{
    private readonly IHubContext<GameHub> _hubContext;
    private GameLifeCycle _gameLifeCycle;
    public DistributeRolesAndCards _distributeRolesAndCards;
    private Room _room;
    public Dictionary<string, string?> ActivePlayers = new();

    public GameStartProcedures(Room room, IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
        _room = room;
        _gameLifeCycle = new GameLifeCycle(room, _hubContext);
        _distributeRolesAndCards = new DistributeRolesAndCards(_hubContext);
        Console.WriteLine("Test GameStartProcedures");
    }

    public async Task SendStartMsg()
    {
        // Example broadcast message to all connected clients
        Console.WriteLine("Testing Started");
        await _hubContext.Clients.All.SendAsync("GameStarted", "The game has started!");
        // await _gameLifeCycle.StartTheGame();
    }

    public async Task DistibutePlayerOnSeats()
    {
        _distributeRolesAndCards.DistributePositions(_room);
        await _distributeRolesAndCards.AllSizeDistributation(_room);
    }
    
}