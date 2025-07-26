namespace MafiaServer.Pages.LobbySection.Models;

public class LivePlayer
{
    public string PlayerId { get; set; }
    public SRPlayerData PlayerData { get; set; }
    public bool PlayerIsReady { get; set; } = false;
    public bool PlayerGotSeat { get; set; } = false;

    public LivePlayer()
    {
    }

    public LivePlayer(string playerId, SRPlayerData playerData, bool playerIsReady, bool playerGotSeat)
    {
        PlayerId = playerId;
        PlayerData = playerData;
        PlayerIsReady = playerIsReady;
        PlayerGotSeat = playerGotSeat;
    }
}