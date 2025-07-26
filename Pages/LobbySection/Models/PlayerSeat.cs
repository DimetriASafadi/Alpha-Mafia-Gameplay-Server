using MafiaServer.Pages.GamePlayLogic.BotSection;
using MafiaServer.Pages.LobbySection.Models;

public class PlayerSeat
{
    public string PlayerId { get; set; }
    public int BotId { get; set; }
    public int FlagId { get; set; } = 909090;
    public string Name { get; set; }
    public bool IsBot { get; set; } = false;
    public bool IsReserved { get; set; } = false;
    public bool HasPlayCard { get; set; } = false;

    public Card? SeatCard { get; set; } = null;
    public SRPlayerData SeatPlayerData { get; set; }
    public List<int> PlayerItems = new List<int>();
    public bool IsConnected { get; set; } = false;

    public bool IsAlive { get; set; } = true;
    public bool IsProtected { get; set; } = false;
    public bool IsMuted { get; set; } = false;
    public bool IsSleeping { get; set; } = false;
    public bool IsOneTimeAbilityUsed { get; set; } = false;
    public readonly List<string> PlayerWasDiscoveredTo = new();
    public bool IsDiscovered { get; set; } = false;
    public bool IsDiscoveredForSpy { get; set; } = false;
    public bool IsMafiaInhereted { get; set; } = false;
    public bool DidDecide { get; set; } = false;
    public bool DidKill { get; set; } = false;

    public BotController BotController { get; set; }


    public PlayerSeat()
    {
    }

    public PlayerSeat(string playerId, int botId, int flagId, string name, bool isBot, bool isReserved,
        bool hasPlayCard, Card seatCard, SRPlayerData seatPlayerData, bool isConnected, bool isAlive, bool isProtected,
        bool isMuted, bool isSleeping, bool isOneTimeAbilityUsed, bool isDiscovered, bool isDiscoveredForSpy,
        bool isMafiaInhereted, bool didDecide, bool didKill, BotController botController)
    {
        PlayerId = playerId;
        BotId = botId;
        FlagId = flagId;
        Name = name;
        IsBot = isBot;
        IsReserved = isReserved;
        HasPlayCard = hasPlayCard;
        SeatCard = seatCard;
        SeatPlayerData = seatPlayerData;
        IsConnected = isConnected;
        IsAlive = isAlive;
        IsProtected = isProtected;
        IsMuted = isMuted;
        IsSleeping = isSleeping;
        IsOneTimeAbilityUsed = isOneTimeAbilityUsed;
        IsDiscovered = isDiscovered;
        IsDiscoveredForSpy = isDiscoveredForSpy;
        IsMafiaInhereted = isMafiaInhereted;
        DidDecide = didDecide;
        DidKill = didKill;
        BotController = botController;
    }
}