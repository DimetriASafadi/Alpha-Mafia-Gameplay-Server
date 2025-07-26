namespace MafiaServer.Pages.LobbySection.Models;

public class ReconnectionData
{
    public string ReconnectionResponse { get; set; }
    public SRRoomBasics? RoomBasics { get; set; }
    public string RoomPlayersData { get; set; }

    public int RoomCurrentDay { get; set; } = 0;
    public string DayOrNight { get; set; } = "";
    public string RoomLastStep { get; set; }  = "";

    
    public ReconnectionData(string reconnectionResponse,SRRoomBasics? roomBasics, string roomPlayersData)
    {
        ReconnectionResponse = reconnectionResponse;
        RoomBasics = roomBasics;
        RoomPlayersData = roomPlayersData;
    }

    public ReconnectionData(string reconnectionResponse,SRRoomBasics? roomBasics, string roomPlayersData,int roomCurrentDay, string dayOrNight,string roomLastStep)
    {
        ReconnectionResponse = reconnectionResponse;
        RoomBasics = roomBasics;
        RoomPlayersData = roomPlayersData;
        RoomCurrentDay = roomCurrentDay;
        DayOrNight = dayOrNight;
        RoomLastStep = roomLastStep;
    }
}