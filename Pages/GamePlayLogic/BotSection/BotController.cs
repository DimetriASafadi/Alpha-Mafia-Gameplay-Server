using System.Diagnostics;
using MafiaServer.Pages.LobbySection.Models;
using MafiaServer.Pages.Models;

namespace MafiaServer.Pages.GamePlayLogic.BotSection;

public class BotController
{
    private static readonly Random _random = new Random(); // Create a single instance

    private readonly List<int> _discoveredPlayerIdsCard = new();

    public Room _room;
    public PlayerSeat thisPlayer;

    public BotController(Room selfroom, PlayerSeat playerseat)
    {
        _room = selfroom;
        thisPlayer = playerseat;
    }

    public async Task TakeDayDecition(int waitThose)
    {
        await Task.Delay(waitThose * 1000);
        if (_room.RoomDataCenter.LastActionInStep != StepCode.DayVotingTime_DVT) return; // too late, don't decide
        if (thisPlayer.IsConnected) return;
        if (!thisPlayer.IsAlive) return;
        if (thisPlayer.IsMuted) return;
        if (thisPlayer.DidDecide) return;


        // 30% chance of not vote on first day
        if (_room.RoomDataCenter.CurrentDayCount == 1 && _random.Next(0, 100) <= 30)
        {
            // Bot Skipped Voting
            var actiontoSend =
                PlayerActionEx.CreatePlayerAction(thisPlayer.PlayerId, PlayerAction.SkipVote, thisPlayer.PlayerId);
            await _room.RoomGameLifeCycle._gameActionsHandler.ActionRecieve(_room, actiontoSend);
            // _room.RoomDataCenter.DayDecitionCount++;
            return;
        }

        var randomTarget = GetVoteTargetFromList();
        if (_room.RoomDataCenter.CurrentDayCount == 2 && thisPlayer.SeatCard.CardAbility == CardAbilities.V3 &&
            !thisPlayer.IsOneTimeAbilityUsed)
        {
            var actiontoSend =
                PlayerActionEx.CreatePlayerAction(thisPlayer.PlayerId, PlayerAction.Vote3X, randomTarget);
            System.Console.WriteLine(actiontoSend);
            await _room.RoomGameLifeCycle._gameActionsHandler.ActionRecieve(_room, actiontoSend);
            // System.Console.WriteLine($"Bot {thisPlayer.PlayerId} V3");
            // _room.RoomDataCenter.DayDecitionCount++;
            // await _room.RoomGameLifeCycle._gameActionsHandler.ActionDay(_room, actiontoSend);
            // SendActionToOthers(actiontoSend);
        }
        else
        {
            var actiontoSend = PlayerActionEx.CreatePlayerAction(thisPlayer.PlayerId, PlayerAction.Vote, randomTarget);
            System.Console.WriteLine(actiontoSend);
            await _room.RoomGameLifeCycle._gameActionsHandler.ActionRecieve(_room, actiontoSend);
            // System.Console.WriteLine($"Bot {thisPlayer.PlayerId} Voted");
            // _room.RoomDataCenter.DayDecitionCount++;
            // await _room.RoomGameLifeCycle._gameActionsHandler.ActionDay(_room, actiontoSend);
            // SendActionToOthers(actiontoSend);
        }

        randomTarget = GetVoteTargetFromList();
        if (thisPlayer.SeatCard.CardAbility == CardAbilities.Sleep)
        {
            var actiontoSend = PlayerActionEx.CreatePlayerAction(thisPlayer.PlayerId, PlayerAction.Sleep, randomTarget);
            await _room.RoomGameLifeCycle._gameActionsHandler.ActionRecieve(_room, actiontoSend);
            // await _room.RoomGameLifeCycle._gameActionsHandler.ActionDay(_room, actiontoSend);
            // SendActionToOthers(actiontoSend);
        }
    }

    public List<string> availiableAbilitiesForJoker = new
        List<string>()
        {
            CardAbilities.Revive, CardAbilities.SaveSelf, CardAbilities.SaveLife, CardAbilities.ShowCard,
            CardAbilities.Snipe
        };

    public void GiveJokerAbility(string randomAbility) // Joker Ability
    {
        if (_room.RoomDataCenter.AllPlayers.Any(aplayer =>
                aplayer.SeatCard.CardAbility is CardAbilities.RandomButton))
        {
            _room.RoomDataCenter.AllPlayers.FirstOrDefault(aplayer =>
                    aplayer.SeatCard.CardAbility is CardAbilities.RandomButton)
                .SeatCard.CardAbility = randomAbility;
        }
    }

    public async Task TakeNightDecition(int waitThose)
    {
        await Task.Delay(waitThose * 1000);

        if (_room.RoomDataCenter.LastActionInStep != StepCode.NightActionsTime_NAT) return; // too late, don't decide

        if (thisPlayer.IsConnected) return; // not a bot!
        if (thisPlayer.IsAlive is false) return; // is dead!
        if (thisPlayer.DidDecide) return; // not again!
        if (thisPlayer.IsSleeping) return; // player is sleeping!
        if (thisPlayer.IsOneTimeAbilityUsed) return; // player ability used one time!


        string RandomAbilityForJoker = availiableAbilitiesForJoker[_random.Next(0, 5)];
        GiveJokerAbility(RandomAbilityForJoker);

        var myCardAbility = thisPlayer.SeatCard.CardAbility;
        var hasNightAbilities = myCardAbility
            is CardAbilities.Snipe or CardAbilities.Mute
            or CardAbilities.ShowCard or CardAbilities.SaveLife
            // or CardAbilities.Sleep Turned to Day
            or CardAbilities.Revive
            or CardAbilities.Spy
            or CardAbilities.SaveSelf;


        if (!hasNightAbilities) return; // good night, nothing to do here

        int randomDayRange = _random.Next(1, 6);
        if (myCardAbility is CardAbilities.SaveSelf) // Warrior Ability
        {
            if (!thisPlayer.IsOneTimeAbilityUsed && _room.RoomDataCenter.CurrentDayCount >= randomDayRange)
            {
                var playerId = thisPlayer.PlayerId;
                var actionCode = GetActionSymbol(myCardAbility);
                var action = PlayerActionEx.FromCode(actionCode);
                var randomTargetId = playerId;
                // string ActiontoSend = playerId + "|" + actionCode + "|" + randomTargetId;
                var actiontoSend = PlayerActionEx.CreatePlayerAction(playerId, action, playerId);
                System.Console.WriteLine(actiontoSend);
                await _room.RoomGameLifeCycle._gameActionsHandler.ActionRecieve(_room, actiontoSend);
                // await _room.RoomGameLifeCycle._gameActionsHandler.ActionNight(_room, actiontoSend);

                
            }
        }
        else
        {
            var playerId = thisPlayer.PlayerId;
            var actionCode = GetActionSymbol(myCardAbility);
            var action = PlayerActionEx.FromCode(actionCode);
            var randomTargetId = GetTargetFromList(ActionType.NightActon);
            // string ActiontoSend = playerId + "|" + actionCode + "|" + randomTargetId;
            var actiontoSend = PlayerActionEx.CreatePlayerAction(playerId, action, randomTargetId);
            if (randomTargetId == "-77997799") return;
            System.Console.WriteLine(actiontoSend);
            
            await _room.RoomGameLifeCycle._gameActionsHandler.ActionRecieve(_room, actiontoSend);
            // await _room.RoomGameLifeCycle._gameActionsHandler.ActionNight(_room, actiontoSend);

        }
        // var playerId = myGameCard.playerId;
        // var actionCode = GetActionSymbol(myCardAbility);
        // var action = PlayerActionEx.FromCode(actionCode);
        // var randomTargetId = GetTargetFromList(myGameCard, ActionType.NightActon);
        // // string ActiontoSend = playerId + "|" + actionCode + "|" + randomTargetId;
        // var actiontoSend = PlayerActionEx.CreatePlayerAction(playerId, action, randomTargetId);
        // SendActionToOthers(actiontoSend);
    }

    public async Task TakeMafiaKill(int waitThose)
    {
        if (!thisPlayer.IsMafiaInhereted) return;
        await Task.Delay(waitThose * 1000);
        if (_room.RoomDataCenter.LastActionInStep != StepCode.NightActionsTime_NAT) return; // too late, don't decide
        if (thisPlayer.IsConnected) return;
        if (!thisPlayer.IsAlive) return;
        if (thisPlayer.DidKill) return;

        var randomTargetId = GetTargetFromList(ActionType.NightKill);
        var actionToSend = PlayerActionEx.CreatePlayerAction(thisPlayer.PlayerId, PlayerAction.Kill, randomTargetId);
        System.Console.WriteLine(actionToSend);
        await _room.RoomGameLifeCycle._gameActionsHandler.ActionRecieve(_room, actionToSend);
        // await _room.RoomGameLifeCycle._gameActionsHandler.ActionNight(_room, actionToSend);
    }

    /// <summary>
    /// Decide who to kill when bot was killed/Voted and is a Qals
    /// </summary>
    /// <param name="waitThose"></param>
    /// <returns></returns>
    public async Task TakeQalsDecision(int waitThose)
    {
        await Task.Delay(waitThose * 1000);
        // only the master can decide for bots

        if (thisPlayer.IsConnected) return; // not a bot
        if (thisPlayer.DidDecide) return; // not again
        if (thisPlayer.SeatCard.CardSymbol != CardSymbols.Qals_Qa) return; // qals only

        string theTarget = GetTargetFromList(ActionType.Qals);
        if (theTarget == "-77997799") return;

        var playerId = thisPlayer.PlayerId;
        var actionCode = GetActionSymbol(thisPlayer.SeatCard.CardAbility);
        var action = PlayerActionEx.FromCode(actionCode);
        var actiontoSend = PlayerActionEx.CreatePlayerAction(playerId, action, theTarget);
        await _room.RoomGameLifeCycle._gameActionsHandler.ActionQalsBomber(_room, actiontoSend,theTarget,CardSymbols.Qals_Qa);
    }

    public async Task TakeBomberDecision(int waitThose)
    {
        await Task.Delay(waitThose * 1000);

        if (thisPlayer.IsConnected || thisPlayer.DidDecide) return;

        if (thisPlayer.SeatCard.CardAbility is not CardAbilities.Bomb) return;


        string theTarget = GetTargetFromList(ActionType.Bomb);
        if (theTarget == "-77997799") return;

        var playerId = thisPlayer.PlayerId;
        var actionCode = GetActionSymbol(thisPlayer.SeatCard.CardAbility);
        var action = PlayerActionEx.FromCode(actionCode);
        var actionToSend = PlayerActionEx.CreatePlayerAction(playerId, action, theTarget);
        await _room.RoomGameLifeCycle._gameActionsHandler.ActionQalsBomber(_room, actionToSend,theTarget,CardSymbols.MafiaBomber_Bo);
    }


    /// <summary>
    /// Randomly return a player for the bot to take action on
    /// </summary>
    /// <param name="myGameCard"></param>
    /// <param name="actionType"></param>
    /// <returns></returns>
    private string GetTargetFromList(ActionType actionType)
    {
        List<string> TargetList = new List<string>();
        var myCardAbility = thisPlayer.SeatCard.CardAbility;
        var myCardSymbol = thisPlayer.SeatCard.CardSymbol;
        var myTeam = thisPlayer.SeatCard.CardTeam;
        bool piriotiryTakenWM = false; // Special for WiseMan did take a piriotry or not
        foreach (var aPlayer in _room.RoomDataCenter.AllPlayers)
        {
            var othersCard = aPlayer;
            var othersTeam = othersCard.SeatCard.CardTeam;
            var isSelf = thisPlayer.PlayerId == othersCard.PlayerId;
            var otherDiscovered = othersCard.IsDiscovered ||
                                  othersCard.PlayerWasDiscoveredTo.Contains(thisPlayer.PlayerId);

            if (myCardAbility is CardAbilities.Revive && !piriotiryTakenWM)
            {
                if (othersTeam is CardTeams.Mafia)
                    continue;
                if (othersCard.IsAlive)
                    continue;
                // Added Piriotiry for doctor and sniper
                if (othersCard.SeatCard.CardAbility is CardAbilities.SaveLife or CardAbilities.Snipe)
                {
                    TargetList.Clear();
                    TargetList.Add(othersCard.PlayerId);
                    piriotiryTakenWM = true;
                }
                else
                {
                    TargetList.Add(othersCard.PlayerId);
                }
            }
            else
            {
                // can't act on dead
                if (othersCard.IsAlive is false) continue;

                // mafia can't act on mafia, or himself

                if (myTeam == othersTeam && myTeam is CardTeams.Mafia)
                {
                    if (string.IsNullOrEmpty(_room.Settings.SettingName) ||
                        !string.IsNullOrEmpty(_room.Settings.SettingName) && _room.Settings.SettingRevealMafiaToMafia)
                    {
                        continue;
                    }
                }
                // if (myTeam == othersTeam && myTeam is CardTeams.Mafia && )
                //     continue;


                // Except save and mute, bot can't act on him self
                if (isSelf && myCardAbility is not (CardAbilities.SaveLife or CardAbilities.Mute))
                    continue;

                // Don't show card for already known (Discovered) cards
                if (otherDiscovered && actionType == ActionType.NightActon && myCardAbility is CardAbilities.ShowCard)
                    continue;


                // when a civilian card is discovered civilian (me) can't act on him Except for Save
                var isCivilian = otherDiscovered && othersCard.SeatCard.CardTeam == CardTeams.Civilian;
                if (isCivilian && myTeam == CardTeams.Civilian && myCardAbility is not CardAbilities.SaveLife)
                    continue;
                TargetList.Add(othersCard.PlayerId);
            }
        }

        int randomIndex = _random.Next(TargetList.Count);
        if (randomIndex < TargetList.Count)
        {
            return TargetList[randomIndex];
        }
        else
        {
            return "-77997799";
        }
    }

    private string GetVoteTargetFromList()
    {
        List<string> TargetList = new List<string>();
        var myTeam = thisPlayer.SeatCard.CardTeam;
        foreach (var aPlayer in _room.RoomDataCenter.AllPlayers)
        {
            var otherPlayer = aPlayer;
            var otherDiscovered = otherPlayer.IsDiscovered ||
                                  otherPlayer.PlayerWasDiscoveredTo.Contains(thisPlayer.PlayerId);
            var otherDiscoveredSpy = otherPlayer.IsDiscoveredForSpy;

            var otherTeam = otherPlayer.SeatCard.CardTeam;

            // don't vote on self
            if (thisPlayer.PlayerId == otherPlayer.PlayerId) continue;

            // don't vote on dead players
            if (!otherPlayer.IsAlive) continue;

            // civilian don't vote on discovered civilian 
            if (otherDiscovered && myTeam is CardTeams.Civilian && otherTeam is CardTeams.Civilian)
                continue;


            // mafia don't vote on mafia
            // if (myTeam is CardTeams.Mafia && otherTeam is CardTeams.Mafia)
            //     continue;
            if (myTeam is CardTeams.Mafia && otherTeam is CardTeams.Mafia)
            {
                if (string.IsNullOrEmpty(_room.Settings.SettingName) ||
                    !string.IsNullOrEmpty(_room.Settings.SettingName) &&
                    _room.Settings.SettingRevealMafiaToMafia)
                {
                    continue;
                }
            }


            // if we know a mafia, only vote on him (Discovered by detective (ShowCard ability))
            if (otherDiscovered && myTeam is CardTeams.Civilian && otherTeam is CardTeams.Mafia)
                return otherPlayer.PlayerId;
            if (otherDiscoveredSpy && myTeam is CardTeams.Mafia && otherTeam is CardTeams.Civilian)
                return otherPlayer.PlayerId;

            TargetList.Add(otherPlayer.PlayerId);
        }

        System.Random random = new System.Random();
        int randomIndex = random.Next(TargetList.Count);
        string randomTargetIndex = TargetList[randomIndex];
        return randomTargetIndex;
    }

    // private void SendActionToOthers(string actionCrypted)
    // {
    //     AllGameDetailsScript.Instance.PPortal.RPC(RPC.ReceivePlayerAction, RpcTarget.AllBuffered, actionCrypted);
    // }

    private string GetActionSymbol(string botAbility)
    {
        switch (botAbility)
        {
            case "Kill":
                return "K";
            case "Snipe":
                return "S";
            case "Mute":
                return "M";
            case "ShowCard":
                return "D";
            case "V3":
                return "V3";
            case "Bomb":
                return "B";
            case "Pull":
                return "Q";
            case "SaveLife":
                return "SL";
            case "Sleep":
                return "SLP";
            case "Revive":
                return "REV";
            case "Spy":
                return "SPY";
            case "AntiVote":
                return "AVO";
            case "SaveSelf":
                return "SS";
            case "Immune":
                return "IM";
        }

        return "";
    }


    private enum ActionType
    {
        /// <summary> Save or Snipe </summary>
        NightActon,
        NightKill,
        Qals,
        Bomb,
    }
}