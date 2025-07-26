using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MafiaServer.Pages.GamePlayLogic;
using MafiaServer.Pages.LobbySection.Models;
using Microsoft.AspNetCore.SignalR;

public class RoomManager
{
    public readonly List<Room> _runningRooms;
    private readonly IHubContext<GameHub> _hubContext;
    private readonly Random _random;


    private readonly MatchSetting Public7Settings = new MatchSetting("", 7, true, 2, 30, 30, true, true, true, true,
        new List<string>(), new List<string>());

    private readonly MatchSetting Public9Settings = new MatchSetting("", 9, true, 2, 30, 30, true, true, true, true,
        new List<string>(), new List<string>());

    private readonly MatchSetting PublicRank7Settings = new MatchSetting("", 7, true, 2, 30, 30, true, true, true, true,
        new List<string>(), new List<string>());

    public Dictionary<string, string> AllConnectedPlayers = new Dictionary<string, string>();
    public List<string> AllAvailableCodes = new List<string>();
    public CryptDataHandler cryptDataHandler;

    public RoomManager(IHubContext<GameHub> hubContext)
    {
        _runningRooms = new List<Room>();
        _hubContext = hubContext;
        _random = new Random();
        cryptDataHandler = new CryptDataHandler();
    }

    public async Task<Room> CreateNewRoom(SRPlayerData srPlayerData, string type, MatchSetting settings)
    {
        string roomId = GenerateUniqueRoomId();
        var room = new Room(roomId, type, srPlayerData, settings, _hubContext);
        int ReadyPlayersChance = _random.Next(0, 100);
        room.MinosRandom = _random.Next(1, 3);
        if (ReadyPlayersChance > 20)
        {
            room.RoomPlayersCount = _random.Next(3, 7);
        }
        else
        {
            room.RoomPlayersCount = 1;
        }

        room.AddPlayer(srPlayerData);
        AllConnectedPlayers[srPlayerData.PlayerConnectionId] = srPlayerData.PlayerId;
        // var room = new Room(roomId, type, srPlayerData, settings);
        _runningRooms.Add(room);
        // if (type is not RoomType.Custom)
        // {
        //     StartAutoStartTimer(room);
        // }

        await _hubContext.Groups.AddToGroupAsync(srPlayerData.PlayerConnectionId, roomId);
        _ = AddFakePlayerCount(room);
        return room;
    }

    public async Task<Room> CreateCustomRoom(SRPlayerData hostPlayerData, string type, MatchSetting settings)
    {
        string roomId = GenerateUniqueRoomId();
        var room = new Room(roomId, type, hostPlayerData, settings, _hubContext);
        string customRoomCode = GenerateUniqueRoomCode();
        System.Console.WriteLine("RoomCode is :" + customRoomCode);
        AllAvailableCodes.Add(customRoomCode);
        room.RoomCode = customRoomCode;
        room.AddPlayer(hostPlayerData);
        AllConnectedPlayers[hostPlayerData.PlayerConnectionId] = hostPlayerData.PlayerId;
        // var room = new Room(roomId, type, srPlayerData, settings);
        _runningRooms.Add(room);
        // if (type is RoomType.Public7 || type is RoomType.Public9 || type is RoomType.PublicRank7)
        // {
        //     StartAutoStartTimer(room);
        // }
        await _hubContext.Groups.AddToGroupAsync(hostPlayerData.PlayerConnectionId, roomId);
        return room;
    }

    public async Task<Room?> JoinRoom(string roomCode, SRPlayerData srPlayerData)
    {
        var room = _runningRooms.FirstOrDefault(r =>
            r.RoomCode == roomCode && r.CRoomState is RoomState.Waiting &&
            r.RoomLivePlayers.Count < r.Settings.SettingPlayersCount);

        if (room != null)
        {
            room.AddPlayer(srPlayerData);
            AllConnectedPlayers[srPlayerData.PlayerConnectionId] = srPlayerData.PlayerId;
            await _hubContext.Groups.AddToGroupAsync(srPlayerData.PlayerConnectionId, room.RoomId);
            room.RoomPlayersCount++;
            await NotifyRoomUpdate(room);
            return room;
        }
        else
        {
            return null;
        }
    }

    public async Task<Room?> JoinRandomRoom(string roomId, SRPlayerData srPlayerData)
    {
        var room = _runningRooms.FirstOrDefault(r =>
            r.RoomId == roomId && r.CRoomState is RoomState.Waiting &&
            r.RoomLivePlayers.Count < r.Settings.SettingPlayersCount);

        if (room != null)
        {
            room.AddPlayer(srPlayerData);
            room.RoomPlayersCount++;
            AllConnectedPlayers[srPlayerData.PlayerConnectionId] = srPlayerData.PlayerId;
            await _hubContext.Groups.AddToGroupAsync(srPlayerData.PlayerConnectionId, roomId);
            await NotifyRoomUpdate(room);
            return room;
        }
        else
        {
            return null;
        }
    }


    public async Task<Room?> SearchPublicRoom(SRPlayerData srPlayerData, string roomType)
    {
        var availableRoom = _runningRooms
            .FirstOrDefault(r => r.RoomType == roomType
                                 && (r.CRoomState == RoomState.Waiting || r.CRoomState == RoomState.SearchingPlayers)
                                 && r.RoomLivePlayers.Count < r.Settings.SettingPlayersCount);

        if (availableRoom != null)
        {
            return await JoinRandomRoom(availableRoom.RoomId, srPlayerData);
        }

        MatchSetting? wantedSetting = null;

        // Create new room if none available
        switch (roomType)
        {
            case RoomType.Public7:
                Console.WriteLine("Public7 Public Room...");
                wantedSetting = Public7Settings;
                break;
            case RoomType.PublicRank7:
                Console.WriteLine("PublicRank7 Public Room...");
                wantedSetting = PublicRank7Settings;
                break;

            case RoomType.Public9:
                Console.WriteLine("Public9 Public Room...");
                wantedSetting = Public9Settings;
                break;

            default:
                Console.WriteLine("Unknown Room Type!");
                wantedSetting = Public7Settings;
                break;
        }

        return await CreateNewRoom(srPlayerData, roomType, wantedSetting);
    }

    private string GenerateUniqueRoomId()
    {
        long currentMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string code = currentMilliseconds + "";
        return code;
    }

    private string GenerateUniqueRoomCode()
    {
        while (true)
        {
            string theRoomCode = _random.Next(1000, 9999).ToString();
            if (!AllAvailableCodes.Contains(theRoomCode))
            {
                return theRoomCode;
            }
        }
    }

    private async Task NotifyRoomUpdate(Room room)
    {
        SRRoomUpdate newupdate = new SRRoomUpdate(room.CRoomState, room.RoomPlayersCount + "");
        await _hubContext.Clients.Group(room.RoomId).SendAsync("OnRoomUpdateListener", newupdate);
    }

    private async Task HostLeftProcedure(Room room)
    {
        await _hubContext.Clients.Group(room.RoomId).SendAsync("OnRoomHostLeft");
    }

    public Task StartWithDelay(string roomId)
    {
        var room = _runningRooms.FirstOrDefault(r => r.RoomId == roomId);
        if (room == null)
            return Task.CompletedTask;
        room.CRoomState = RoomState.Playing;
        // await _hubContext.Clients.Group(room.RoomId).SendAsync("OnRoomStartedGame");
        room.GameTimer = new Timer(async _ =>
        {
            // if (room.RoomLivePlayers.Count == room.Settings.SettingPlayersCount)
            // {
            await StartGame(room.RoomId);
            // }
        }, null, TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
        return Task.CompletedTask;
    }

    public async Task StartGame(string roomId)
    {
        System.Console.WriteLine("Started Game");
        var room = _runningRooms.FirstOrDefault(r => r.RoomId == roomId);

        if (room == null)
            return;

        room.ChangeState(RoomState.Playing);
        await room.RoomStartProcedures.DistibutePlayerOnSeats();
        await _hubContext.Clients.Group(room.RoomId).SendAsync("OnRoomStartedGame");
        System.Console.WriteLine("OnGameStarted Executed");
        await Task.Delay(17000);
        await room.RoomGameLifeCycle.StartTheGame();
    }

    public async Task CloseRoom(string roomId)
    {
        var room = _runningRooms.FirstOrDefault(r => r.RoomId == roomId);

        if (room == null)
            return;

        room.CleanupTimer(); // Cleanup the timer

        foreach (var aplayerData in room.RoomLivePlayers)
        {
            await _hubContext.Groups.RemoveFromGroupAsync(aplayerData.Value.PlayerConnectionId, room.RoomId);
        }

        // Remove the group from your tracking
        _runningRooms.Remove(room);
        await _hubContext.Clients.Group(roomId).SendAsync("OnRoomClosed");
    }

    public async Task RemoveUserFromLastRoom(string userid)
    {
        var room = _runningRooms.FirstOrDefault(r => r.RoomLivePlayers.ContainsKey(userid));

        if (room == null)
            return;

        // Create a new HubConnectionContext to abort the specific connection
        var connection =
            _hubContext.Clients.Client(room.RoomLivePlayers[userid].PlayerConnectionId) as HubCallerContext;
        if (connection != null)
        {
            // Remove the old connections
            connection.Abort();
            room.RemovePlayer(room.RoomLivePlayers[userid]);
            // room.RoomLivePlayers.Remove(userid);
            System.Console.WriteLine($"Connection found and Aborted");
        }

        // Remove Player From Room
        // await _hubContext.Groups.RemoveFromGroupAsync(room.RoomLivePlayers[userid].PlayerConnectionId, room.RoomId);
        // Remove the group from your tracking
        // await _hubContext.Clients.Group(room.RoomId).SendAsync("OnRoomClosed");
    }

    public async Task AddFakePlayerCount(Room room)
    {
        // var room = _runningRooms.FirstOrDefault(r => r.RoomId == roomid);
        if (room == null)
            return;
        /*
        Wait for random second.
        if not master: do nothing!
        if all players connected, start the game
        or else ....
         */
        int randomWaitingTime = _random.Next(8, 15);
        int FastOrSLowRandom = _random.Next(1, 3);
        if (FastOrSLowRandom == 1)
        {
            randomWaitingTime = _random.Next(1, 4);
        }

        await Task.Delay(randomWaitingTime * 1000);
        Console.WriteLine("room.RoomPlayersCount = " + room.RoomPlayersCount +
                          "room.Settings.SettingPlayersCount = " + room.Settings.SettingPlayersCount);

        if (room.RoomPlayersCount >= room.Settings.SettingPlayersCount)
        {
            Console.WriteLine("fakePlayerCount >= MaxP");
            // if (StartRandomGameCoroutine != null)
            // {
            //     Console.WriteLine("StopCoroutine(StartRandomGameCoroutine);");
            //     StopCoroutine(StartRandomGameCoroutine);
            // }
            // // fakeWaitingSeconds = 3;
            // StartRandomGameCoroutine = StartCoroutine(MasterLaunchGameScreen());
            _ = StartWithDelay(room.RoomId);
            Console.WriteLine("StartCoroutine(MasterLaunchGameScreen())");
        }
        else if (room.RoomPlayersCount < room.Settings.SettingPlayersCount)
        {
            /*
            shouldMinos random [0 .. 100]
            "FakePlayer" has the number of fake players
             */
            // shouldMinos is used to show that a player has left the waiting game. (A FAKE PLAYER)
            // shouldMinos less than 10 for a random range of (0 .. 100) is 10%
            int shouldMinos = _random.Next(0, 100);
            if (shouldMinos < 11 && room.RoomPlayersCount > 1 &&
                room.RoomPlayersCount < room.Settings.SettingPlayersCount && room.MinosRandom > room.MinosRandomReach)
            {
                room.RoomPlayersCount--;
                room.MinosRandomReach++;
            }
            else
            {
                room.RoomPlayersCount++;
            }

            // Notify Users with players count
            // SendFakePlayersCount(room.RoomPlayersCount);
            room.CRoomState = RoomState.Waiting;
            await NotifyRoomUpdate(room);
            Console.WriteLine("(playersFake < MaxP)");
            _ = AddFakePlayerCount(room);
        }
    }

    public async Task PlayerDisconnected(string userConnectionid)
    {
        if (AllConnectedPlayers.ContainsKey(userConnectionid))
        {
            string playerId = AllConnectedPlayers[userConnectionid];
            var room = _runningRooms.FirstOrDefault(r => r.RoomLivePlayers.ContainsKey(playerId)) ?? null;

            if (room == null)
                return;
            bool isTheHost = room.Host.PlayerId == playerId && room.RoomType is RoomType.Custom;
            // Create a new HubConnectionContext to abort the specific connection
            if (room.CRoomState is RoomState.Playing && room.RoomDataCenter.AllPlayers.Count > 0)
            {
                if (room.RoomDataCenter.AllPlayers.Any(aseat => aseat.PlayerId == playerId))
                {
                    room.RoomDataCenter.AllPlayers.FirstOrDefault(aseat => aseat.PlayerId == playerId)!.IsConnected =
                        false;
                }

                AllConnectedPlayers.Remove(userConnectionid);
                await _hubContext.Groups.RemoveFromGroupAsync(room.RoomLivePlayers[playerId].PlayerConnectionId,
                    room.RoomId);
            }
            else if (room.CRoomState is RoomState.SearchingPlayers or RoomState.Waiting)
            {
                if (isTheHost)
                {
                    // Execute function to let players leave room
                    await HostLeftProcedure(room);
                }

                AllConnectedPlayers.Remove(userConnectionid);
                await _hubContext.Groups.RemoveFromGroupAsync(room.RoomLivePlayers[playerId].PlayerConnectionId,
                    room.RoomId);
                room.RoomLivePlayers.Remove(playerId);
                room.RoomPlayersCount--;
                await NotifyRoomUpdate(room);
            }
            // Remove Player From Room
            // await _hubContext.Groups.RemoveFromGroupAsync(room.RoomLivePlayers[userid].PlayerConnectionId, room.RoomId);
            // Remove the group from your tracking
            // await _hubContext.Clients.Group(room.RoomId).SendAsync("OnRoomClosed");
        }
    }

    public Task ReconnectingProcedure(Room room, SRPlayerData myPlayerdata)
    {
        // Create a new HubConnectionContext to abort the specific connection
        if (room.RoomDataCenter.AllPlayers.Any(aseat => aseat.PlayerId == myPlayerdata.PlayerId))
        {
            room.RoomDataCenter.AllPlayers.FirstOrDefault(aseat => aseat.PlayerId == myPlayerdata.PlayerId)!.IsConnected =
                true;
        }
        AllConnectedPlayers[myPlayerdata.PlayerConnectionId] = myPlayerdata.PlayerId;
        room.RoomLivePlayers[myPlayerdata.PlayerId] = myPlayerdata;
        _hubContext.Groups.AddToGroupAsync(myPlayerdata.PlayerConnectionId, room.RoomId);
        System.Console.WriteLine($"Connection found and Aborted");


        System.Console.WriteLine($"DisconnectProcedure Done");
        return Task.CompletedTask;
    }

    // public Task DuplicatesProcedure(Room room, SRPlayerData myPlayerdata)
    // {
    //     // Create a new HubConnectionContext to abort the specific connection
    //     var connection =
    //         _hubContext.Clients.Client(room.RoomLivePlayers[myPlayerdata.PlayerId].PlayerConnectionId) as
    //             HubCallerContext;
    //     if (connection != null)
    //     {
    //         connection.Abort();
    //         room.RoomLivePlayers[myPlayerdata.PlayerId] = myPlayerdata;
    //         _hubContext.Groups.AddToGroupAsync(myPlayerdata.PlayerConnectionId, room.RoomId);
    //         System.Console.WriteLine($"Connection found and Aborted");
    //     }
    //     else
    //     {
    //         System.Console.WriteLine($"Connection not found");
    //     }
    //
    //     System.Console.WriteLine($"DisconnectProcedure Done");
    //     return Task.CompletedTask;
    // }
}