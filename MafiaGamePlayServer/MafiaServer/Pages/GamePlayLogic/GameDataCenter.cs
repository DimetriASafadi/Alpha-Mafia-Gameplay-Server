using MafiaServer.Pages.LobbySection.Models;
using MafiaServer.Pages.Models;

namespace MafiaServer.Pages.GamePlayLogic;

public class GameDataCenter
{
    public bool GameStarted = false;
    public bool GameFinished = false;
    public int DayCount = 0;
    public DayNight DayOrNight = DayNight.Day; // Send for reconnect issues
    public int DayDecitionCount = 0;
    public List<PlayerSeat> AllPlayers = new List<PlayerSeat>();
    public List<BotAccount> AllBotAccounts = new List<BotAccount>();

    public List<EventToStore>
        StoredEvents =
            new List<EventToStore>(); // Store all Action Executed only then when player only reconnect send it to him

    public List<EventAction> EventsToShow = new List<EventAction>(); // Send them only after result for current players
    public List<int> StoreFlags = new List<int>();
    public string LastActionInStep = "Wait"; // Send for reconnect issues
    public int CurrentDayCount = 0; // Send for reconnect issues


    public int PanelTimeShort = 5;
    public int PanelTimeMedium = 8;
    public int PanelTimeLong = 20;
    public int PanelTimeHalf = 30;

    // Reconnect Issues Send
    // DayorNight + DaysCounts + PlayersGameCardDetails + 

    public bool VotingDone = false;
    
}