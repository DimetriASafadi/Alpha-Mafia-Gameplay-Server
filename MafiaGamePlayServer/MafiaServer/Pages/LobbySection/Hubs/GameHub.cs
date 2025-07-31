using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using MafiaServer.Pages.GamePlayLogic;
using MafiaServer.Pages.LobbySection.Models;

public class GameHub : Hub
{
    private readonly RoomManager _roomManager;
    private readonly ILogger<GameHub> _logger;

    public GameHub(RoomManager roomManager,ILogger<GameHub> logger)
    {
        _roomManager = roomManager;
        _logger = logger;
    }

    public async Task<SRRoomBasics> CreateRoom(SRPlayerData playerdata, string type, MatchSetting settings)
    {
        // System.Console.WriteLine($"CreateRoom Executed");
        _logger.LogInformation("CreateRoom Executed LogInformation");
        _logger.LogError("CreateRoom Executed LogError");
        _logger.LogDebug("CreateRoom Executed LogDebug");
        _logger.LogWarning("CreateRoom Executed LogWarning");

        Room? room = null;
        if (type is RoomType.Custom)
        {
            room = await _roomManager.CreateCustomRoom(playerdata, type, settings);
        }
        else
        {
            room = await _roomManager.CreateNewRoom(playerdata, type, settings);
        }

        if (room != null)
        {
            SRRoomBasics sRRoomBasics = new SRRoomBasics(room.RoomId, room.RoomType, room.RoomCode, room.Settings);
            return sRRoomBasics;
        }
        else
        {
            SRRoomBasics sRRoomBasics = new SRRoomBasics(RoomJoinResponse.RoomError, null, null, null);
            return sRRoomBasics;
        }
        // await Clients.Caller.SendAsync("OnRoomCreated", "got you body " + playerdata.PlayerName);
    }

    public async Task<SRRoomBasics> JoinRoom(string roomCode, SRPlayerData playerdata)
    {
        // System.Console.WriteLine($"JoinRoom Executed");
        _logger.LogInformation("JoinRoom Executed");

        // System.Console.WriteLine($"playerdataCId " + playerdata.PlayerConnectionId);
        _logger.LogInformation("playerdataCId " + playerdata.PlayerConnectionId);

        Room? room = null;
        room = await _roomManager.JoinRoom(roomCode, playerdata);

        if (room != null)
        {
            SRRoomBasics sRRoomBasics = new SRRoomBasics(room.RoomId, room.RoomType, room.RoomCode, room.Settings);
            return sRRoomBasics;
        }
        else
        {
            SRRoomBasics sRRoomBasics = new SRRoomBasics(RoomJoinResponse.RoomNotExist, null, null, null);
            return sRRoomBasics;
        }


        // if (room != null)
        //     await Clients.Caller.SendAsync("OnRoomJoined", room);
        // else
        //     await Clients.Caller.SendAsync("OnJoinError", "Room not found or full");
    }

    public async Task<SRRoomBasics> SearchPublicRoom(SRPlayerData srPlayerData, string roomType)
    {
        // System.Console.WriteLine($"SearchPublicRoom Executed");
        _logger.LogInformation("SearchPublicRoom Executed LogInformation");
        _logger.LogError("SearchPublicRoom Executed LogError");
        _logger.LogDebug("SearchPublicRoom Executed LogDebug");
        _logger.LogWarning("SearchPublicRoom Executed LogWarning");
        


        // var room = await _roomManager.SearchPublicRoom(srPlayerData, roomType);

        // var atestplayer = new SRPlayerData("454", "454", "testConnectioin");
        Room? room = null;
        room = await _roomManager.SearchPublicRoom(srPlayerData, roomType);
        if (room != null)
        {
            SRRoomBasics sRRoomBasics = new SRRoomBasics(room.RoomId, room.RoomType, room.RoomCode, room.Settings);
            return sRRoomBasics;
        }
        else
        {
            SRRoomBasics sRRoomBasics = new SRRoomBasics(RoomJoinResponse.RoomError, null, null, null);
            return sRRoomBasics;
        }
        // await Clients.Caller.SendAsync("OnRoomJoined", room.RoomId);
    }

    public async Task<SRRoomUpdate?> GetRoomDetails(string roomId)
    {
        // System.Console.WriteLine($"GetRoomDetails Executed");
        _logger.LogInformation("GetRoomDetails Executed");

        Room? room = null;
        room = _roomManager._runningRooms.FirstOrDefault(r =>
            r.RoomId == roomId && r.CRoomState is RoomState.Waiting);

        if (room != null)
        {
            SRRoomUpdate newrUpdate = new SRRoomUpdate(room.CRoomState, room.RoomPlayersCount + "");
            return newrUpdate;
        }
        else
        {
            return null;
        }


        // if (room != null)
        //     await Clients.Caller.SendAsync("OnRoomJoined", room);
        // else
        //     await Clients.Caller.SendAsync("OnJoinError", "Room not found or full");
    }

    public async Task<string> GetRoomPlayersDetails(string roomId)
    {
        // System.Console.WriteLine($"GetRoomDetails Executed");
        _logger.LogInformation("GetRoomDetails Executed");

        Room? room = null;
        room = _roomManager._runningRooms.FirstOrDefault(r =>
            r.RoomId == roomId && r.CRoomState is RoomState.Playing);

        if (room != null)
        {
            string newrUpdate = _roomManager.cryptDataHandler.CryptPlayersDetails(room.RoomDataCenter.AllPlayers);
            return newrUpdate;
        }
        else
        {
            return "";
        }


        // if (room != null)
        //     await Clients.Caller.SendAsync("OnRoomJoined", room);
        // else
        //     await Clients.Caller.SendAsync("OnJoinError", "Room not found or full");
    }

    public async Task StartMatchRoom(string roomId)
    {
        await _roomManager.StartWithDelay(roomId);
        // await _roomManager.StartGame(roomId);
    }


    ///////// _______ Disconnect and Reconnect Section
    public async Task<ReconnectionData> ReconnetingHandler(SRPlayerData myPlayerdata) // When any user Connect to lobby
    {
        var room = _roomManager._runningRooms.FirstOrDefault(r =>
            r.RoomLivePlayers.ContainsKey(myPlayerdata.PlayerId));

        // System.Console.WriteLine($"Found User");
        _logger.LogInformation("Found User");


        if (room != null)
        {
            // Fix the duplicated Cids and replace the old Cid with new Cid

            SRRoomBasics sRRoomBasics = new SRRoomBasics(room.RoomId, room.RoomType, room.RoomCode, room.Settings);
            string sRRoomPlayersData =
                _roomManager.cryptDataHandler.CryptPlayersDetails(room.RoomDataCenter.AllPlayers);
            string reconnectionResponse = "";
            if (room.CRoomState == RoomState.Playing)
            {
                await _roomManager.ReconnectingProcedure(room, myPlayerdata);
                reconnectionResponse = ReconnectResponse.Successful;
            }
            else if (room.CRoomState == RoomState.Finished)
            {
                reconnectionResponse = ReconnectResponse.Finished;
            }
            else
            {
                reconnectionResponse = ReconnectResponse.Failed;
            }
            ReconnectionData reconnectionData =
                new ReconnectionData(reconnectionResponse, sRRoomBasics, sRRoomPlayersData,room.RoomDataCenter.CurrentDayCount,room.RoomDataCenter.DayOrNight.ToString(),room.RoomDataCenter.LastActionInStep);
            return reconnectionData;
        }
        else
        {
            ReconnectionData reconnectionData =
                new ReconnectionData(ReconnectResponse.GameNotFound, null, "");
            return reconnectionData;
        }
    }

    // Executed Only when user make new Connection
    // public async Task<ReconnectionData> NewConnectionHandler(SRPlayerData myPlayerdata,
    //     ReconnectStatus status) // When any user Connect to lobby
    // {
    //     var room = _roomManager._runningRooms.FirstOrDefault(r =>
    //         r.RoomLivePlayers.ContainsKey(myPlayerdata.PlayerId));
    //
    //     System.Console.WriteLine($"Found User");
    //     if (status is ReconnectStatus.RejoinGame)
    //     {
    //         if (room != null)
    //         {
    //             // Fix the duplicated Cids and replace the old Cid with new Cid
    //
    //             await _roomManager.DuplicatesProcedure(room, myPlayerdata);
    //             await Clients.Caller.SendAsync("RedirectToGameScene", room.RoomId);
    //         }
    //     }
    //     else if (status is ReconnectStatus.NewGame)
    //     {
    //         // Delete player permently from other rooms RoomLivePlayers list
    //         await _roomManager.RemoveUserFromLastRoom(myPlayerdata.PlayerId);
    //     }
    // }

// Executed when User connect to the Server
    public override async Task OnConnectedAsync()
    {
        // Optionally, handle any setup logic when a client connects
        // System.Console.WriteLine($"Client connected: {Context.ConnectionId}");
        _logger.LogInformation($"Client connected: {Context.ConnectionId}");

        await base.OnConnectedAsync();
    }

    // This method is called when a client disconnects
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        // Handle disconnection logic here
        // System.Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");

        if (exception != null)
        {
            // System.Console.WriteLine($"Disconnection error: {exception.Message}");
            _logger.LogInformation($"Disconnection error: {exception.Message}");

        }

        await _roomManager.PlayerDisconnected(Context.ConnectionId);
        // Perform any cleanup or notify other clients about the disconnection
        await base.OnDisconnectedAsync(exception);
    }

    // public Task DuplicatesProcedure(string userId)


    // public async Task ReconnectUser(string userId)
    // {
    //     string connectionId = Context.ConnectionId;
    //     System.Console.WriteLine($"User {userId} reconnected with connection ID: {connectionId}");
    //     
    // }

    public async Task NormalAction(string roomid, string playerAction)
    {
        var wantedroom = _roomManager._runningRooms.FirstOrDefault(aroom => aroom.RoomId == roomid);
        if (wantedroom != null)
        {
            // System.Console.WriteLine("Player " + playerAction);
            _logger.LogInformation("Player " + playerAction);

            await wantedroom.RoomGameLifeCycle._gameActionsHandler.ActionRecieve(wantedroom, playerAction);
        }
        else
        {
            await Clients.Caller.SendAsync("ActionRejected");
        }
    }

    public async Task QalsBomberAction(string roomid, string playerAction, string targetid, string qalsOrbomber)
    {
        // Compress the Operation inside Room manager
        var wantedroom = _roomManager._runningRooms.FirstOrDefault(aroom => aroom.RoomId == roomid);
        if (wantedroom != null)
        {
            await wantedroom.RoomGameLifeCycle._gameActionsHandler.ActionQalsBomber(wantedroom, playerAction, targetid,
                qalsOrbomber);
        }
        else
        {
            await Clients.Caller.SendAsync("ActionRejected");
        }
    }

    public async Task DayAction(string roomid, string playerAction)
    {
        var wantedroom = _roomManager._runningRooms.FirstOrDefault(aroom => aroom.RoomId == roomid);
        if (wantedroom != null)
        {
            // await wantedroom.RoomGameLifeCycle._gameActionsHandler.ActionDay(wantedroom, playerAction);
            await wantedroom.RoomGameLifeCycle._gameActionsHandler.ActionRecieve(wantedroom, playerAction);
        }
        else
        {
            await Clients.Caller.SendAsync("ActionRejected");
        }
    }

    public async Task NightAction(string roomid, string playerAction)
    {
        var wantedroom = _roomManager._runningRooms.FirstOrDefault(aroom => aroom.RoomId == roomid);
        if (wantedroom != null)
        {
            // await wantedroom.RoomGameLifeCycle._gameActionsHandler.ActionNight(wantedroom, playerAction);
            await wantedroom.RoomGameLifeCycle._gameActionsHandler.ActionRecieve(wantedroom, playerAction);
        }
        else
        {
            await Clients.Caller.SendAsync("ActionRejected");
        }
    }

    // public async Task DisconnectFullyFromMatch(string roomid, string playerid)
    // {
    // }
    
    public string Ping()
    {
        return "pong";
    }

    // public async Task PlayerIsReady(string roomid, string playerid)
    // {
    //     // Compress the Operation inside Room manager
    //     var wantedroom = _roomManager._runningRooms.FirstOrDefault(aroom => aroom.RoomId == roomid);
    //     if (wantedroom != null)
    //     {
    //         await wantedroom.RoomGameLifeCycle._gameActionsHandler.ActionQalsBomber(wantedroom, playerAction);
    //     }
    //     else
    //     {
    //         await Clients.Caller.SendAsync("ActionRejected");
    //     }
    // }
}