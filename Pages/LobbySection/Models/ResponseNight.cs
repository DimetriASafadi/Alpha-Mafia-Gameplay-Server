using MafiaServer.Pages.Models;

namespace MafiaServer.Pages.LobbySection.Models;

public class ResponseNight
{
    public List<EventAction> NightActions { get; set; }
    public bool NoEliminations { get; set; }
    
    public ResponseNight(List<EventAction> nightActions, bool noEliminations)
    {
        NightActions = nightActions;
        NoEliminations = noEliminations;
    }
}