namespace MafiaServer.Pages.LobbySection.Models;

public class ResponseDay
{
    public string VotedPlayerId { get; set; }
    public bool VotedWasDraw { get; set; }
    public int VotesCount { get; set; }

    public ResponseDay(string votedPlayerId, bool votedWasDraw, int votesCount)
    {
        VotedPlayerId = votedPlayerId;
        VotedWasDraw = votedWasDraw;
        VotesCount = votesCount;
    }
}