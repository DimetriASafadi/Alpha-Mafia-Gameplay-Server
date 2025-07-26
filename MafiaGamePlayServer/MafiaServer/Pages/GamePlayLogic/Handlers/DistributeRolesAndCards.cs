using System.Diagnostics;
using MafiaServer.Pages.GamePlayLogic.BotSection;
using MafiaServer.Pages.LobbySection.Models;
using MafiaServer.Pages.Models;
using Microsoft.AspNetCore.SignalR;

namespace MafiaServer.Pages.GamePlayLogic;

public class DistributeRolesAndCards
{
    private static readonly Random _random = new Random(); // Create a single instance

    private readonly IHubContext<GameHub> _hubContext;
    private CryptDataHandler _cryptDataHandler;
    private SendDataHandler _sendDataHandler;
    Dictionary<string, bool> MatchCards = new Dictionary<string, bool>();
    List<string> MatchCardsNames = new List<string>();

    public DistributeRolesAndCards(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
        _cryptDataHandler = new CryptDataHandler();
        _sendDataHandler = new SendDataHandler(_hubContext);
    }

    public void DistributePositions(Room _room)
    {
        // Add seats to dataCenter
        for (int i = 0; i < _room.Settings.SettingPlayersCount; i++)
        {
            var anseat = new PlayerSeat();
            anseat.BotController = new BotController(_room, anseat);
            _room.RoomDataCenter.AllPlayers.Add(anseat);
        }

        foreach (var aliveplayr in _room.RoomLivePlayers)
        {
            var aemptySeat = _room.RoomDataCenter.AllPlayers.FirstOrDefault(aseat => !aseat.IsReserved);
            aemptySeat.PlayerId = aliveplayr.Value.PlayerId;
            aemptySeat.SeatPlayerData = aliveplayr.Value;
            aemptySeat.IsConnected = true;
            aemptySeat.IsBot = false;
            aemptySeat.IsReserved = true;
        }

        // foreach (var aliveplayr in _room.RoomDataCenter.LivePlayers)
        // {
        //     var aemptySeat = _room.RoomDataCenter.AllPlayers.FirstOrDefault(aseat => !aseat.IsReserved);
        //     if (aliveplayr.PlayerIsReady)
        //     {
        //         aemptySeat.PlayerId = aliveplayr.PlayerId;
        //         aemptySeat.SeatPlayerData = aliveplayr.PlayerData;
        //         aemptySeat.IsConnected = true;
        //         aemptySeat.IsBot = false;
        //         aemptySeat.IsReserved = true;
        //     }
        // }

        int botcount = 1;
        int randomBotRange = 0;
        foreach (var aSeat in _room.RoomDataCenter.AllPlayers)
        {
            if (!aSeat.IsReserved)
            {
                aSeat.PlayerId = "-" + botcount;
                aSeat.IsBot = true;
                botcount++;
                aSeat.BotId = _random.Next(randomBotRange, randomBotRange + 10);
                randomBotRange += 10;
                aSeat.IsReserved = true;
                // int selectedRandBot = GetFitRandomBotAccount();
                // if (selectedRandBot > -1)
                // {
                //     gameCard.playerBotId = selectedRandBot;
                // }
            }
        }
    }

    public List<int> ChoosenBotAccountIds = new List<int>();

    public int GetFitRandomBotAccount(Room _room)
    {
        int result = -1;
        int wantedLevel = 0;
        if (_room.RoomType is RoomType.PublicRank7)
        {
            wantedLevel = 12;
        }
        else if (_room.RoomType is RoomType.Public9)
        {
            wantedLevel = 7;
        }
        else if (_room.RoomType is RoomType.Public7)
        {
            wantedLevel = 0;
        }

        // var allBots = AllGameDetailsScript.Instance.AllBotAccounts;
        // List<BotAccount> botsAboveWantedLevel = allBots
        //     .Where(bot => bot.GetLevel() >= wantedLevel && !ChoosenBotAccountIds.Contains(bot.name))
        //     .ToList();


        // BotAccount selectedBaccount = botsAboveWantedLevel[Random.Range(0, botsAboveWantedLevel.Count)];
        // ChoosenBotAccountIds.Add(selectedBaccount.name);
        // result = allBots.IndexOf(selectedBaccount);

        return result;
    }

    public async Task AllSizeDistributation(Room _room)
    {
        #region ForceAssignCardToMeForTesting

        // For Test set the role first
        // For Test set the role first
        // mafia boss 10 assign 
        // GiveMafiaBoss(allPlayers);
        // // mayor 12 or detective 11
        // GiveMayorDetective(allPlayers);
        // // Qals 14 or sniper 15 assign
        // GiveQalsSniper(allPlayers);
        // // doctor 13 assign
        // GiveDoctor(allPlayers);
        // // silent 9 or bomber 8  assign
        // GiveMafiaSilentBomber(allPlayers);
        // // 2 civilians assign
        // GiveCivilians(allPlayers);

        #endregion

        if (!string.IsNullOrEmpty(_room.Settings.SettingName))
        {
            FillCustomMatchCards(MatchCards, _room);
        }
        else
        {
            if (_room.RoomType is RoomType.Public9)
            {
                FillMatchCards9(_room);
            }
            else
            {
                FillMatchCards7(null, _room);
            }
        }

        // Give Bots Flags
        _cryptDataHandler.GiveRandomFlaggedBots(_room);
        // Sending Seats Data to all players ******************
        // string playerInString = _cryptDataHandler.CryptPlayersDetails(_room.RoomDataCenter.AllPlayers);
        // await _sendDataHandler.ToClientsSendPlayersDetailsBegin(_room.RoomId, playerInString);
        // await Task.Delay(3000);

    }

    private void FillCustomMatchCards(Dictionary<string, bool> AvaiCards, Room _room)
    {
        // to fill randomly cards 
        List<string> availablecivils = new List<string>()
        {
            CardSymbols.Mayor_My, CardSymbols.Detective_De, CardSymbols.Doctor_Do, CardSymbols.Sniper_Sn,
            CardSymbols.Qals_Qa, CardSymbols.Civilian_C1,
            CardSymbols.Civilian_C2, CardSymbols.Civilian_C3
        };
        List<string> availablemafias = new List<string>()
        {
            CardSymbols.MafiaBoss_Mb, CardSymbols.MafiaSilent_Si, CardSymbols.MafiaBomber_Bo, CardSymbols.MafiaNormal_Mn
        };

        List<string> civilsCards = new List<string>();
        List<string> mafiasCards = new List<string>();


        // get and fill characters choosen

        foreach (var asetting in _room.Settings.SettingChoosenCivilians)
        {
            civilsCards.Add(asetting);
            availablecivils.RemoveAll(thecard => thecard == asetting);
        }

        foreach (var asetting in _room.Settings.SettingChoosenMafia)
        {
            mafiasCards.Add(asetting);
            availablemafias.RemoveAll(thecard => thecard == asetting);
        }

        int remainingcivilCards = _room.Settings.SettingPlayersCount -
                                  _room.Settings.SettingMafiaCount -
                                  civilsCards.Count;
        int remainingmafiaCards =
            _room.Settings.SettingMafiaCount - civilsCards.Count;


        // fill if need more cards for both mafia and civils
        for (int i = 0; i < remainingcivilCards; i++)
        {
            System.Random rng = new System.Random();
            int randomIndex = rng.Next(availablecivils.Count);
            string randomString = availablecivils[randomIndex];
            civilsCards.Add(randomString);
            availablecivils.RemoveAll(thecard => thecard == randomString);
        }

        for (int i = 0; i < remainingmafiaCards; i++)
        {
            System.Random rng = new System.Random();
            int randomIndex = rng.Next(availablemafias.Count);
            string randomString = availablecivils[randomIndex];
            mafiasCards.Add(randomString);
            availablemafias.RemoveAll(thecard => thecard == randomString);
        }

        int civilRange = _room.Settings.SettingPlayersCount -
                         _room.Settings.SettingMafiaCount;
        int mafiaRange = _room.Settings.SettingMafiaCount;

        civilsCards = civilsCards.GetRange(0, civilRange);
        mafiasCards = mafiasCards.GetRange(0, mafiaRange);


        foreach (var asetting in civilsCards)
        {
            AvaiCards.Add(asetting, false);
            MatchCardsNames.Add(asetting);
        }

        foreach (var asetting in mafiasCards)
        {
            AvaiCards.Add(asetting, false);
            MatchCardsNames.Add(asetting);
        }


        DistributeToPlayersWithChance(_room);
        DistributeToNormalPlayers(_room);
        CheckIfAllPlayerHaveCards(_room);
    }

    private void FillMatchCards7(string forceMyCardSymbol, Room _room)
    {
        System.Random random = new System.Random();

        // Static Characters = MafiaBoss + Doctor
        MatchCards.Add(CardSymbols.MafiaBoss_Mb, false);
        MatchCardsNames.Add(CardSymbols.MafiaBoss_Mb);
        MatchCards.Add(CardSymbols.Doctor_Do, false);
        MatchCardsNames.Add(CardSymbols.Doctor_Do);

        // Randomly Picked Characters =
        // (Mayor|Detective|Wiseman)
        // (Qals|Sniper|Warrior)
        // (LiomHeart|Joker|Princess)
        // (MafiaNormal|MafiaSpy|MafiaSleeper|MafiaBomber|MafiaSilent)

        string[] CivTeamCha1 = new string[] { CardSymbols.Mayor_My, CardSymbols.Detective_De, CardSymbols.WiseMan };
        string[] CivTeamCha2 = new string[] { CardSymbols.Qals_Qa, CardSymbols.Sniper_Sn, CardSymbols.Warrior };
        string[] CivTeamCha3 = new string[] { CardSymbols.LionHeart, CardSymbols.Joker, CardSymbols.Princess };
        string[] MafTeamCha1 = new string[]
        {
            CardSymbols.MafiaBomber_Bo, CardSymbols.MafiaSilent_Si, CardSymbols.MafiaNormal_Mn, CardSymbols.MafiaSpy,
            CardSymbols.MafiaSleeper_Slp
        };
        List<string> CivilliansList = new List<string>
            { CardSymbols.Civilian_C1, CardSymbols.Civilian_C2, CardSymbols.Civilian_C3 };


        string CivTeamCha1R = CivTeamCha1[_random.Next(0, 3)];
        MatchCards.Add(CivTeamCha1R, false);
        MatchCardsNames.Add(CivTeamCha1R);
        string CivTeamCha2R = CivTeamCha2[_random.Next(0, 3)];
        MatchCards.Add(CivTeamCha2R, false);
        MatchCardsNames.Add(CivTeamCha2R);
        string CivTeamCha3R = CivTeamCha3[_random.Next(0, 3)];
        MatchCards.Add(CivTeamCha3R, false);
        MatchCardsNames.Add(CivTeamCha3R);
        string MafTeamCha1R = MafTeamCha1[_random.Next(0, 5)];
        MatchCards.Add(MafTeamCha1R, false);
        MatchCardsNames.Add(MafTeamCha1R);


        int randomFIndex = random.Next(CivilliansList.Count);
        MatchCards.Add(CivilliansList[randomFIndex], false);
        MatchCardsNames.Add(CivilliansList[randomFIndex]);
        CivilliansList.RemoveAt(randomFIndex);


        DistributeToPlayersWithChance(_room);
        DistributeToNormalPlayers(_room);
        CheckIfAllPlayerHaveCards(_room);
    }

    private void FillMatchCards9(Room _room)
    {
        System.Random random = new System.Random();

        // Static Characters = MafiaBoss + Doctor
        MatchCards.Add(CardSymbols.MafiaBoss_Mb, false);
        MatchCardsNames.Add(CardSymbols.MafiaBoss_Mb);
        MatchCards.Add(CardSymbols.Doctor_Do, false);
        MatchCardsNames.Add(CardSymbols.Doctor_Do);

        // Randomly Picked Characters =
        // (Mayor|Detective|Wiseman)
        // (Qals|Sniper|Warrior)
        // (LiomHeart|Joker|Princess)
        // (MafiaNormal|MafiaSpy|MafiaSleeper)
        // (MafiaBomber|MafiaSilent)

        string[] CivTeamCha1 = new string[] { CardSymbols.Mayor_My, CardSymbols.Detective_De, CardSymbols.WiseMan };
        string[] CivTeamCha2 = new string[] { CardSymbols.Qals_Qa, CardSymbols.Sniper_Sn, CardSymbols.Warrior };
        string[] CivTeamCha3 = new string[] { CardSymbols.LionHeart, CardSymbols.Joker, CardSymbols.Princess };
        string[] MafTeamCha1 = new string[]
            { CardSymbols.MafiaNormal_Mn, CardSymbols.MafiaSpy, CardSymbols.MafiaSleeper_Slp };
        string[] MafTeamCha2 = new string[] { CardSymbols.MafiaBomber_Bo, CardSymbols.MafiaSilent_Si };
        List<string> CivilliansList = new List<string>
            { CardSymbols.Civilian_C1, CardSymbols.Civilian_C2, CardSymbols.Civilian_C3 };


        string CivTeamCha1R = CivTeamCha1[_random.Next(0, 3)];
        MatchCards.Add(CivTeamCha1R, false);
        MatchCardsNames.Add(CivTeamCha1R);
        string CivTeamCha2R = CivTeamCha2[_random.Next(0, 3)];
        MatchCards.Add(CivTeamCha2R, false);
        MatchCardsNames.Add(CivTeamCha2R);
        string CivTeamCha3R = CivTeamCha3[_random.Next(0, 3)];
        MatchCards.Add(CivTeamCha3R, false);
        MatchCardsNames.Add(CivTeamCha3R);
        string MafTeamCha1R = MafTeamCha1[_random.Next(0, 3)];
        MatchCards.Add(MafTeamCha1R, false);
        MatchCardsNames.Add(MafTeamCha1R);
        string MafTeamCha2R = MafTeamCha2[_random.Next(0, 2)];
        MatchCards.Add(MafTeamCha2R, false);
        MatchCardsNames.Add(MafTeamCha2R);


        int randomFIndex = random.Next(CivilliansList.Count);
        MatchCards.Add(CivilliansList[randomFIndex], false);
        MatchCardsNames.Add(CivilliansList[randomFIndex]);
        CivilliansList.RemoveAt(randomFIndex);

        int randomSIndex = random.Next(CivilliansList.Count);
        MatchCards.Add(CivilliansList[randomSIndex], false);
        MatchCardsNames.Add(CivilliansList[randomSIndex]);
        CivilliansList.RemoveAt(randomSIndex);

        DistributeToPlayersWithChance(_room);
        DistributeToNormalPlayers(_room);
        CheckIfAllPlayerHaveCards(_room);
    }

    private void DistributeToPlayersWithChance(Room _room)
    {
        foreach (var cardSymbol in MatchCardsNames)
        {
            if (!MatchCards[cardSymbol])
            {
                CheckGiveChanceWinner(cardSymbol, _room);
            }
        }
    }

    private void CheckGiveChanceWinner(string cardSymbol, Room _room)
    {
        Card cardObj = _room.CharactersCards.FirstOrDefault(p => p.CardSymbol == cardSymbol);
        string winnerGameCard = null;
        int lastPlayerWinRatio = 101;
        var chanceItem = cardObj.ChanceItem;
        if (chanceItem == null) return;

        foreach (var player in _room.RoomDataCenter.AllPlayers)
        {
            foreach (var anItem in player.PlayerItems)
            {
                if (player.HasPlayCard) continue;
                if (anItem != chanceItem) continue;
                int cardChanceRatio = _random.Next(0, 101);
                if (cardChanceRatio <= 70 && cardChanceRatio < lastPlayerWinRatio)
                {
                    winnerGameCard = player.PlayerId;
                    lastPlayerWinRatio = cardChanceRatio;
                }

                // Debug.Log("ChanceWinner" + winnerGameCard.playerId + " Ratio " + lastPlayerWinRatio + " Card " +
                //           cardSymbol);
            }
        }

        if (winnerGameCard == null) return;
        _room.RoomDataCenter.AllPlayers.FirstOrDefault(aseat => aseat.PlayerId.Equals(winnerGameCard)).HasPlayCard =
            true;
        _room.RoomDataCenter.AllPlayers.FirstOrDefault(aseat => aseat.PlayerId.Equals(winnerGameCard)).SeatCard =
            cardObj;
        MatchCards[cardSymbol] = true;
        if (cardObj.CardSymbol == CardSymbols.MafiaBoss_Mb)
        {
            _room.RoomDataCenter.AllPlayers.FirstOrDefault(aseat => aseat.PlayerId.Equals(winnerGameCard))
                .IsMafiaInhereted = true;
        }
    }

    private void DistributeToNormalPlayers(Room _room)
    {
        foreach (var cardSymbol in MatchCardsNames)
        {
            if (!MatchCards[cardSymbol])
            {
                if (!GiveNormalWinner(cardSymbol, _room)) System.Console.WriteLine("Failed to assign player");
            }
            else
            {
                System.Console.WriteLine("MatchCard Taken");
            }

            ;
        }
    }

    private bool GiveNormalWinner(string cardSymbol, Room _room)
    {
        Card cardObj = _room.CharactersCards.FirstOrDefault(p => p.CardSymbol == cardSymbol);
        string winnerGameCard = null;
        int lastPlayerWinRatio = 101;
        foreach (var player in _room.RoomDataCenter.AllPlayers)
        {
            if (player.HasPlayCard) continue;
            int cardChanceRatio = _random.Next(0, 101);
            if (cardChanceRatio < lastPlayerWinRatio)
            {
                // Debug.Log("player chosen");
                winnerGameCard = player.PlayerId;
                lastPlayerWinRatio = cardChanceRatio;
            }
        }

        if (winnerGameCard == null) return false;
        _room.RoomDataCenter.AllPlayers.FirstOrDefault(aseat => aseat.PlayerId.Equals(winnerGameCard)).HasPlayCard =
            true;
        _room.RoomDataCenter.AllPlayers.FirstOrDefault(aseat => aseat.PlayerId.Equals(winnerGameCard)).SeatCard =
            cardObj;
        MatchCards[cardSymbol] = true;
        if (cardObj.CardSymbol == CardSymbols.MafiaBoss_Mb)
        {
            _room.RoomDataCenter.AllPlayers.FirstOrDefault(aseat => aseat.PlayerId.Equals(winnerGameCard))
                .IsMafiaInhereted = true;
        }

        return true;
    }

    private bool CheckIfAllPlayerHaveCards(Room _room)
    {
        List<string> playerIds = new();
        foreach (var player in _room.RoomDataCenter.AllPlayers)
        {
            if (player.SeatCard == null)
            {
                playerIds.Add(player.PlayerId);
            }
        }

        List<string> unusedCards = new();

        foreach (var card in MatchCards)
        {
            if (card.Value == false)
            {
                unusedCards.Add(card.Key);
            }
        }

        if (playerIds.Count > 0 || unusedCards.Count > 0)
        {
            System.Console.WriteLine(
                $"incomplete distribution players: [{string.Join(',', playerIds)}], cards: [{string.Join(',', unusedCards)}]");
            return true;
        }

        return false;
    }
}