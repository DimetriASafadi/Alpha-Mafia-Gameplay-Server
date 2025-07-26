namespace MafiaServer.Pages.LobbySection.Models;

public class SRRoomUpdate
{
    public string RStatus { get; set; }
    public string RPlayerCount { get; set; }

    public SRRoomUpdate(string rStatus, string rPlayerCount)
    {
        RStatus = rStatus;
        RPlayerCount = rPlayerCount;
    }
}