using MafiaServer.Pages.LobbySection.Models;

namespace MafiaServer.Pages.GamePlayLogic;

public class CryptDataHandler
{
    private static readonly Random _random = new Random(); // Create a single instance
    

    public string CryptPlayersDetails(List<PlayerSeat> AllPlayersSeats)
    {
        // PlayerID|PlayerIsAlive|PlayerCardSymbol|PlayerUsedOneTimeAbility|PlayerMafiaInhereted|PlayerWasDiscovered|PlayerIsConnected|PlayerIsMuted|PlayerIsProtected|playerIsBot|playerBotId+new(|FlagId)
        // 845|1|Ma|1|1|1|1|0|0|15,845|1|Ma|1|1|1|1|0|0|15,845|1|Ma|1|1|1|1|0|0|15,845|1|Ma|1|1|1|1|0|0|15,845|1|Ma|1|1|1|1|0|0|15

        string result = "";
        foreach (var aPlayer in AllPlayersSeats)
        {
            string playerInString = "";
            playerInString += (aPlayer.PlayerId) + "|";
            playerInString += (aPlayer.IsAlive ? 1 : 0) + "|";
            playerInString += (aPlayer.SeatCard.CardSymbol) + "|";
            playerInString += (aPlayer.IsOneTimeAbilityUsed ? 1 : 0) + "|";
            playerInString += (aPlayer.IsMafiaInhereted ? 1 : 0) + "|";
            playerInString += (aPlayer.IsDiscovered ? 1 : 0) + "|";
            playerInString += (aPlayer.IsConnected ? 1 : 0) + "|";
            playerInString += (aPlayer.IsMuted ? 1 : 0) + "|";
            playerInString += (aPlayer.IsProtected ? 1 : 0) + "|";
            playerInString += (aPlayer.IsBot ? 1 : 0) + "|";
            playerInString += (aPlayer.BotId) + "|";
            playerInString += (aPlayer.FlagId) + "";


            result += playerInString + ",";
        }

        result = result.Remove(result.Length - 1);
        return result;
    }

    public void GiveRandomFlaggedBots(Room _room)
    {
        // no flags for bot when playing with friends
        if (_room.RoomType is RoomType.Custom)
            return;


        List<string> BotsIds = GetPlayersIdsOfBots(_room.RoomDataCenter.AllPlayers);

        // no bots in game!
        if (BotsIds.Count == 0)
            return;

        // // one bot will always get a flag
        // if (BotsIds.Count == 1)
        // {
        //     // only 5% percent change of getting a flag
        //     if (Random.Range(0, 100f) > 5f) 
        //         return string.Empty;
        //     
        //     var singeBotId = BotsIds[0];
        //     var flagId = GiveThisBotAFlag(gameScript,  singeBotId);
        //     cryptedString += "," + singeBotId + "|" + flagId;
        //     return cryptedString;
        // }

        foreach (var anBotId in BotsIds)
        {
            // only 5% percent change of getting a flag
            if (_random.Next(0, 100) > 5)
                continue;

            GiveThisBotAFlag(_room.RoomDataCenter.StoreFlags, anBotId, _room.RoomDataCenter.AllPlayers);
        }

        static List<string> GetPlayersIdsOfBots(List<PlayerSeat> allPlayersSeats2)
        {
            var botsIds = new List<string>();
            foreach (var aPlayer in allPlayersSeats2)
            {
                if (aPlayer.IsBot)
                {
                    botsIds.Add(aPlayer.PlayerId);
                }
            }

            return botsIds;
        }

        // Give bot the flag, return the flag id
        static void GiveThisBotAFlag(List<int> FlagsList, string botId, List<PlayerSeat> allPlayersSeats2)
        {
            if (FlagsList.Count > 0)
            {
                var countOfFlagsOnStore = FlagsList.Count;
                var randomFlagIndex = _random.Next(0, countOfFlagsOnStore);
                int oneFlagOnlyRandom = FlagsList[randomFlagIndex];
                allPlayersSeats2.FirstOrDefault(aplayer => aplayer.PlayerId == botId).FlagId = oneFlagOnlyRandom;
            }
        }
    }
}