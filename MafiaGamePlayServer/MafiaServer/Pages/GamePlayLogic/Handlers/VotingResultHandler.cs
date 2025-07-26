namespace MafiaServer.Pages.GamePlayLogic;

public record VoteResult
{
    private VoteResult()
    {
    }

    public record NoVotes() : VoteResult;

    public record Draw(int votes, string[] playerIds) : VoteResult;

    public record Voted(int votes, string playerId, PlayerSeat votedPlayerGameCard) : VoteResult;
};

public class VotingResultHandler
{
    /// <summary> </summary>
    /// <returns>
    /// NoVotes, <i>Draw</i>, 3P125334 (NoOfVotes[P]PlayerId), 3Qals123456, 4MafiaBomber3456543 <br/>
    /// * will be added at first if the player was selected randomly on Draw voting <br/>
    /// like : *3pQals123456 <br/>
    /// <i>Draw is currently disabled, a random player will be selected on draw</i>
    /// </returns>
    public VoteResult GetVotingResult(Room _room)
    {
        Dictionary<string, int> votesDictionary = GetVotesDictionary(_room);
        string[] playerIds = GetVotedPlayersIds(votesDictionary);
        if (playerIds.Length == 0) return new VoteResult.NoVotes(); //"NoVotes";
        if (playerIds.Length == 1)
        {
            string votedPlayerId = playerIds[0];
            PlayerSeat playerCard = _room.RoomDataCenter.AllPlayers.FirstOrDefault(ap => ap.PlayerId == votedPlayerId);
            int voteCount = votesDictionary[votedPlayerId];
            return new VoteResult.Voted(voteCount, votedPlayerId, playerCard);
        }
        else
        {
            int voteCount = votesDictionary[playerIds[0]];
            return new VoteResult.Draw(voteCount, playerIds);
        }
    }


    /// <summary>
    /// Return the players ids with highest vote count, or empty of no once voted
    /// </summary>
    private static string[] GetVotedPlayersIds(Dictionary<string, int> votesCount)
    {
        if (votesCount.Count == 0)
        {
            return Array.Empty<string>();
        }

        var voted = new List<string>();

        // The maximum number of votes on a player
        var maxVotes = votesCount.Values.Max();

        foreach (var (playerId, votes) in votesCount)
        {
            if (votes == maxVotes)
            {
                voted.Add(playerId);
            }
        }

        return voted.ToArray();
    }

    private static Dictionary<string, int> GetVotesDictionary(Room _room)
    {
        var votesCount = new Dictionary<string, int>();
        foreach (var anEvent in _room.RoomDataCenter.EventsToShow)
        {
            if (!anEvent.eventAction.Equals("V") && !anEvent.eventAction.Equals("V3"))
            {
                continue;
            }

            var voteCount = 1;
            if (anEvent.eventAction.Equals("V3")) voteCount = 3;
            var playerId = anEvent.eventVictimId;
            votesCount.TryGetValue(playerId, out var count); // counts default to 0 when not votes set
            votesCount[playerId] = count + voteCount;
        }

        return votesCount;
    }
}