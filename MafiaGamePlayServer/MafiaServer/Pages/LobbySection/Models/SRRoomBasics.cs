namespace MafiaServer.Pages.LobbySection.Models;

public class SRRoomBasics
{
    public string RoomId { get; set; }
    public string RoomMode { get; set; }
    public string RoomCode { get; set; }
    public MatchSetting RoomSettings { get; set; }

    public SRRoomBasics(string roomId, string roomMode, string roomCode, MatchSetting roomSettings)
    {
        RoomId = roomId;
        RoomMode = roomMode;
        RoomCode = roomCode;
        RoomSettings = roomSettings;
    }
}