namespace MafiaServer.Pages.LobbySection.Models;

public class SRPlayerData
{
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public string PlayerConnectionId { get; set; }
    

    public SRPlayerData(string playerId, string playerName, string playerConnectionId)
    {
        PlayerId = playerId;
        PlayerName = playerName;
        PlayerConnectionId = playerConnectionId;
    }
}