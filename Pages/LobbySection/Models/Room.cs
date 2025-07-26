using System;
using System.Collections.Generic;
using System.Linq;
using MafiaServer.Pages.GamePlayLogic;
using MafiaServer.Pages.LobbySection.Models;
using Microsoft.AspNetCore.SignalR;

public class Room
{
    private readonly IHubContext<GameHub> _hubContext;
    public string RoomId { get; private set; }
    public string RoomType { get; private set; }
    public string RoomCode { get; set; } = "----";
    public string CRoomState { get; set; }
    public Dictionary<string, SRPlayerData> RoomLivePlayers { get; private set; }
    public int RoomPlayersCount { get; set; } = 1;
    public SRPlayerData Host { get; private set; }
    public MatchSetting Settings { get; private set; }
    public Timer GameTimer { get; set; }
    public GameDataCenter RoomDataCenter { get; set; }
    public GameStartProcedures RoomStartProcedures { get; set; }
    public GameLifeCycle RoomGameLifeCycle { get; set; }
    public int MinosRandom { get; set; } = 0;
    public int MinosRandomReach { get; set; } = 0;

    public List<Card> CharactersCards = new List<Card>() // Should be fetched from Api
    {
        new Card(1, "Civilian1", CardAbilities.Non, CardTeams.Civilian, CardSymbols.Civilian_C1, false,
            CardAbilityTimes.DayOnly_D),
        new Card(2, "Civilian2", CardAbilities.Non, CardTeams.Civilian, CardSymbols.Civilian_C2, false,
            CardAbilityTimes.DayOnly_D),
        new Card(3, "Civilian3", CardAbilities.Non, CardTeams.Civilian, CardSymbols.Civilian_C3, false,
            CardAbilityTimes.DayOnly_D),
        new Card(4, "Mayor", CardAbilities.V3, CardTeams.Civilian, CardSymbols.Mayor_My, true,
            CardAbilityTimes.DayOnly_D),
        new Card(5, "Detective", CardAbilities.ShowCard, CardTeams.Civilian, CardSymbols.Detective_De, false,
            CardAbilityTimes.NightOnly_N),
        new Card(6, "Sniper", CardAbilities.Snipe, CardTeams.Civilian, CardSymbols.Sniper_Sn, false,
            CardAbilityTimes.NightOnly_N),
        new Card(7, "Qals", CardAbilities.Pull, CardTeams.Civilian, CardSymbols.Qals_Qa, false,
            CardAbilityTimes.Both_DN),
        new Card(8, "Doctor", CardAbilities.SaveLife, CardTeams.Civilian, CardSymbols.Doctor_Do, false,
            CardAbilityTimes.NightOnly_N),
        new Card(9, "Warrior", CardAbilities.SaveSelf, CardTeams.Civilian, CardSymbols.Warrior, false,
            CardAbilityTimes.NightOnly_N),
        new Card(10, "LionHeart", CardAbilities.Immune, CardTeams.Civilian, CardSymbols.LionHeart, true,
            CardAbilityTimes.NightOnly_N),
        new Card(11, "Princess", CardAbilities.AntiVote, CardTeams.Civilian, CardSymbols.Princess, false,
            CardAbilityTimes.DayOnly_D),
        new Card(12, "Wiseman", CardAbilities.Revive, CardTeams.Civilian, CardSymbols.WiseMan, false,
            CardAbilityTimes.NightOnly_N),
        new Card(13, "Joker", CardAbilities.RandomButton, CardTeams.Civilian, CardSymbols.Joker, false,
            CardAbilityTimes.Both_DN),
        new Card(14, "MafiaNormal", CardAbilities.Non, CardTeams.Mafia, CardSymbols.MafiaNormal_Mn, false,
            CardAbilityTimes.NightOnly_N),
        new Card(15, "MafiaBoss", CardAbilities.Kill, CardTeams.Mafia, CardSymbols.MafiaBoss_Mb, false,
            CardAbilityTimes.NightOnly_N),
        new Card(16, "MafiaSleeper", CardAbilities.Sleep, CardTeams.Mafia, CardSymbols.MafiaSleeper_Slp, false,
            CardAbilityTimes.DayOnly_D),
        new Card(17, "MafiaSpy", CardAbilities.Spy, CardTeams.Mafia, CardSymbols.MafiaSpy, false,
            CardAbilityTimes.NightOnly_N),
        new Card(18, "MafiaSilent", CardAbilities.Mute, CardTeams.Mafia, CardSymbols.MafiaSilent_Si, false,
            CardAbilityTimes.NightOnly_N),
        new Card(19, "MafiaBomber", CardAbilities.Bomb, CardTeams.Mafia, CardSymbols.MafiaBomber_Bo, false,
            CardAbilityTimes.Both_DN),
    };


    public Room(string roomId, string type, SRPlayerData host, MatchSetting settings, IHubContext<GameHub> hubContext)
    {
        RoomId = roomId;
        RoomType = type;
        Host = host;
        Settings = settings;
        CRoomState = RoomState.Waiting;
        RoomLivePlayers = new Dictionary<string, SRPlayerData>();
        RoomDataCenter = new GameDataCenter();
        RoomStartProcedures = new GameStartProcedures(this, hubContext);
        RoomGameLifeCycle = new GameLifeCycle(this, hubContext);
        _hubContext = hubContext;
    }

    public void AddPlayer(SRPlayerData srPlayerSeat)
    {
        RoomLivePlayers[srPlayerSeat.PlayerId] = srPlayerSeat;
    }

    public bool RemovePlayer(SRPlayerData srPlayerData)
    {
        if (!RoomLivePlayers.ContainsKey(srPlayerData.PlayerId))
            return false;
        RoomLivePlayers.Remove(srPlayerData.PlayerId);

        // If host leaves, assign new host
        if (Host.PlayerId == srPlayerData.PlayerId && RoomLivePlayers.Count > 0)
        {
            Host = RoomLivePlayers.Values.First();
        }

        return true;
    }
    

    public void ChangeState(string newState)
    {
        CRoomState = newState;
        // FillEmptySlotsWithBots();
    }

    
    // private void FillEmptySlotsWithBots()
    // {
    //     int botsNeeded = Settings.SettingPlayersCount - RoomLivePlayers.Count;
    //     // Add Players Seats
    //     for (int i = 0; i < botsNeeded; i++)
    //     {
    //         var bot = new PlayerSeat($"Bot_{i}", $"bot_{Guid.NewGuid()}", true);
    //         AddPlayer(bot);
    //         Fill Seats depend on GameSize
    //         Fill Bots in empty slots
    //     }
    //     for (int i = 0; i < botsNeeded; i++)
    //     {
    //         var bot = new PlayerSeat($"Bot_{i}", $"bot_{Guid.NewGuid()}", true);
    //         AddPlayer(bot);
    //         Fill Seats depend on GameSize
    //         Fill Bots in empty slots
    //     }
    //     
    // }

    public void CleanupTimer()
    {
        if (GameTimer != null)
        {
            GameTimer.Dispose();
        }
    }
}