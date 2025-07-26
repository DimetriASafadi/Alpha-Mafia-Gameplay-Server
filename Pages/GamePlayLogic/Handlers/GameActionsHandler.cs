using System.Diagnostics;
using System.Net.Mime;
using MafiaServer.Pages.LobbySection.Models;
using MafiaServer.Pages.Models;
using Microsoft.AspNetCore.SignalR;

namespace MafiaServer.Pages.GamePlayLogic;

public class GameActionsHandler
{
    private IHubContext<GameHub> _hubContext;
    private CryptDataHandler _cryptDataHandler;
    private SendDataHandler _sendDataHandler;
    private Room _room;


    public GameActionsHandler(Room _Room, IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
        _cryptDataHandler = new CryptDataHandler();
        _sendDataHandler = new SendDataHandler(_hubContext);
        _room = _Room;
    }

    public async Task ActionRecieve(Room roomid, string playerAction)
    {
        ReceivePlayerAction(roomid, playerAction);
        // await _sendDataHandler.ToClientsSendDayAction(_room.RoomId, playerAction);
        await _sendDataHandler.ToClientsSendAction(_room.RoomId, playerAction);

    }

    public async Task ActionDay(Room roomid, string playerAction)
    {
        ReceivePlayerAction(roomid, playerAction);
        await _sendDataHandler.ToClientsSendDayAction(_room.RoomId, playerAction);
    }

    public async Task ActionNight(Room roomid, string playerAction)
    {
        ReceivePlayerAction(roomid, playerAction);
        await _sendDataHandler.ToClientsSendNightAction(_room.RoomId, playerAction);
    }

    public async Task ActionQalsBomber(Room roomid, string playerAction, string targetid, string qalsOrbomber)
    {
        _room.RoomGameLifeCycle._QalsBomberChoice = playerAction;
        _room.RoomGameLifeCycle._QalsBomberTimer?.Cancel();
        ReceivePlayerAction(roomid, playerAction);
        await _sendDataHandler.ToClientsSendQalsBomberChoice(_room.RoomId, playerAction, targetid, qalsOrbomber);
    }

    public void ReceivePlayerAction(Room _room, string playerAction)
    {
        // string[] actionData = playerAction.Split('|');
        // DayNightEvent dayNightEvent = new DayNightEvent();
        // dayNightEvent.eventActorId = int.Parse(actionData[0]);
        // dayNightEvent.eventAction = actionData[1];
        // dayNightEvent.eventVictimId = int.Parse(actionData[2]);
        // dayNightEvent.eventIDTime = AllGameDetailsScript.Instance.CurrentTime.ToString() + AllGameDetailsScript.Instance.CurrentDayCount;
        EventAction dayNightEvent = EventAction.Parse(playerAction);
        var victimPlayerId = dayNightEvent.eventVictimId;
        var victimPlayerseat =
            _room.RoomDataCenter.AllPlayers.FirstOrDefault(aplayer => dayNightEvent.eventVictimId == aplayer.PlayerId);
        var actorPlayerId = dayNightEvent.eventActorId;
        var actorPlayerseat =
            _room.RoomDataCenter.AllPlayers.FirstOrDefault(aplayer => dayNightEvent.eventActorId == aplayer.PlayerId);
        _room.RoomDataCenter.EventsToShow.Add(dayNightEvent);

        bool processed = false;
        // UpdatePlayersDetails(); executed when result ends
        if (_room.RoomDataCenter.DayOrNight is DayNight.Day)
        {
            if (dayNightEvent.PlayerAction is PlayerAction.Vote or PlayerAction.Vote3X or PlayerAction.SkipVote)
            {
                System.Console.WriteLine($"PlayerOrBot {playerAction}");
                _room.RoomDataCenter.DayDecitionCount++;
                // ShowVoteThumbnail(this, dayNightEvent);
                // printVoteActionOnLog(this, dayNightEvent);
                // _room.RoomDataCenter.StoredEvents.Add(new EventToStore(actorPlayerId, dayNightEvent.eventAction,
                //     victimPlayerId));
                processed = true;
            }
        }
        else
        {
            if (dayNightEvent.PlayerAction == PlayerAction.SaveLife)
            {
                _room.RoomDataCenter.AllPlayers.FirstOrDefault(aplayer => aplayer.PlayerId == victimPlayerId)
                    .IsProtected = true;
                victimPlayerseat.IsProtected = true;
                // printNightActionOnLog(this, dayNightEvent);
                processed = true;
            }

            if (dayNightEvent.PlayerAction == PlayerAction.Detect)
            {
                _room.RoomDataCenter.AllPlayers.FirstOrDefault(aplayer => aplayer.PlayerId == victimPlayerId)
                    .PlayerWasDiscoveredTo.Add(actorPlayerId);
                victimPlayerseat.PlayerWasDiscoveredTo.Add(dayNightEvent.eventActorId);
                // printNightActionOnLog(this, dayNightEvent);
                processed = true;
            }

            if (dayNightEvent.PlayerAction == PlayerAction.Spy)
            {
                _room.RoomDataCenter.AllPlayers.FirstOrDefault(aplayer => aplayer.PlayerId == victimPlayerId)
                    .PlayerWasDiscoveredTo.Add(actorPlayerId);
                victimPlayerseat.PlayerWasDiscoveredTo.Add(dayNightEvent.eventActorId);
                // printNightActionOnLog(this, dayNightEvent);
                processed = true;
            }

            if (dayNightEvent.PlayerAction is PlayerAction.Kill or PlayerAction.Bomb or PlayerAction.Mute
                or PlayerAction.Sleep
                or PlayerAction.Spy)
            {
                // ShowMafiaKillThumbnail(this, dayNightEvent);
                processed = true;
            }
            // if (dayNightEvent.PlayerAction is PlayerAction.Sleep)
            // {
            //     ShowMafiaKillThumbnail(this, dayNightEvent);
            //     gameScript.eventsToShow.RemoveAll(anevent => anevent.eventActorId == dayNightEvent.eventVictimId);
            //     processed = true;
            // }
        }

        // save if player decided
        // int actorIdtoInt = int.Parse(actionData[0]);

        // actorGameCard.playerDidDecide = true;
        actorPlayerseat.DidDecide = true;
        if (dayNightEvent.PlayerAction == PlayerAction.Kill)
        {
            actorPlayerseat.DidKill = true;
            // actorGameCard.playerDidKill = true;
        }

        return;
    }

    public async Task UpdatePlayersDetails()
    {
        // System.Console.WriteLine("UpdatePlayersDetails started");
        List<EventAction> KiActions = _room.RoomDataCenter.EventsToShow
            .Where(anevent => anevent.eventAction.Equals(PlayerAction.Kill)).ToList();

        // Remove those cars from the original list
        _room.RoomDataCenter.EventsToShow = _room.RoomDataCenter.EventsToShow.Except(KiActions).ToList();

        // Add them to the end of the list
        _room.RoomDataCenter.EventsToShow.AddRange(KiActions);
        foreach (var anAction in _room.RoomDataCenter.EventsToShow)
        {
            var victimGameCard =
                _room.RoomDataCenter.AllPlayers.FirstOrDefault(aplayer => anAction.eventVictimId == aplayer.PlayerId);
            var actorGameCard =
                _room.RoomDataCenter.AllPlayers.FirstOrDefault(aplayer => anAction.eventActorId == aplayer.PlayerId);
            switch (anAction.PlayerAction)
            {
                case PlayerAction.Mute:
                    victimGameCard.IsMuted = true;
                    break;
                case PlayerAction.Detect:
                    if (victimGameCard.SeatCard.CardSymbol is not CardSymbols.MafiaSpy)
                    {
                        victimGameCard.IsDiscovered = true;
                    }

                    break;
                case PlayerAction.Vote3X:
                    actorGameCard.IsOneTimeAbilityUsed = true;
                    actorGameCard.IsDiscovered = true;
                    // ChallengeRecorder.instance.RecordChallenge(AllChallenges.use_mayor_feature_27);
                    break;
                case PlayerAction.SaveLife:
                    victimGameCard.IsAlive = true;
                    victimGameCard.IsProtected = true;
                    break;
                case PlayerAction.Sleep:
                    victimGameCard.IsSleeping = true;
                    break;
                case PlayerAction.Revive:
                    victimGameCard.IsAlive = true;
                    victimGameCard.IsDiscovered = true;
                    break;
                case PlayerAction.Spy:
                    victimGameCard.IsDiscoveredForSpy = true;
                    break;
                case PlayerAction.SaveSelf:
                    victimGameCard.IsAlive = true;
                    victimGameCard.IsProtected = true;
                    victimGameCard.IsOneTimeAbilityUsed = true;
                    break;
                case PlayerAction.Kill:
                    bool ItsLionHeart = victimGameCard.SeatCard.CardAbility is CardAbilities.Immune &&
                                        !victimGameCard.IsOneTimeAbilityUsed;
                    if (!victimGameCard.IsProtected && !ItsLionHeart)
                    {
                        victimGameCard.IsAlive = false;
                    }
                    else if (ItsLionHeart)
                    {
                        actorGameCard.IsAlive = false;
                        victimGameCard.IsAlive = true;
                    }

                    break;
                case PlayerAction.Qals:
                    actorGameCard.IsAlive = false;
                    if (!victimGameCard.IsProtected)
                    {
                        victimGameCard.IsAlive = false;
                    }

                    break;
                case PlayerAction.Bomb:
                    actorGameCard.IsAlive = false;
                    if (!victimGameCard.IsProtected)
                    {
                        victimGameCard.IsAlive = false;
                    }

                    break;
                case PlayerAction.Snipe:
                    if (!victimGameCard.IsProtected)
                    {
                        victimGameCard.IsAlive = false;
                    }

                    break;
            }
        }

        string PlayerInString = _cryptDataHandler.CryptPlayersDetails(_room.RoomDataCenter.AllPlayers);
        System.Console.WriteLine("lastPlayersUpdate " + PlayerInString);
        await _sendDataHandler.ToClientsSendPlayersDetails(_room.RoomId, PlayerInString);
    }

    int QalsBomberTimer = 0;
    bool someOneKilled = false;

    public bool TryGetPlayerByCardSymbol(string cardSymbol, out PlayerSeat playerset)
    {
        foreach (var player in _room.RoomDataCenter.AllPlayers)
        {
            var aseat = player;
            if (player.SeatCard.CardSymbol == cardSymbol)
            {
                playerset = aseat;
                return true;
            }

            ;
        }

        playerset = null;
        return false;
    }

    public async Task ShowNightEventPanel(EventsHandler eventsHandler)
    {
        ResponseNight responseNight = new ResponseNight(_room.RoomDataCenter.EventsToShow, false);
        someOneKilled = false;

        if (_room.RoomDataCenter.EventsToShow.Count == 0)
        {
            // OsamaAction Zero Action at Night
            // NightEventPanel.transform.Find("VotedPlayerCard/CardNameContainer/CardName").GetComponent<RTLTextMeshPro>().text = "ليلة هادئة";
            // NightEventPanel.transform.FindRecursively("#PanelMessage").GetComponent<RTLTextMeshPro>().text = "لم يحدث شيئ أثناء هذه الليلة";
            System.Console.WriteLine("Night Was Calm");
            // await _sendDataHandler.ToClientsSendNightResult(_room.RoomId, "Calm");
        }
        else
        {
            // put save life on last actions
            if (_room.RoomDataCenter.EventsToShow.Any(anevent => anevent.eventAction.Equals(PlayerAction.SaveLife)) ||
                _room.RoomDataCenter.EventsToShow.Any(anevent => anevent.eventAction.Equals(PlayerAction.SaveSelf)))
            {
                List<EventAction> SLActions = _room.RoomDataCenter.EventsToShow
                    .Where(anevent => anevent.eventAction.Equals(PlayerAction.SaveLife)).ToList();
                List<EventAction> SSActions = _room.RoomDataCenter.EventsToShow
                    .Where(anevent => anevent.eventAction.Equals(PlayerAction.SaveSelf)).ToList();
                List<EventAction> IMActions = _room.RoomDataCenter.EventsToShow
                    .Where(anevent => anevent.eventAction.Equals(PlayerAction.Immune)).ToList();

                // Remove those cars from the original list
                _room.RoomDataCenter.EventsToShow = _room.RoomDataCenter.EventsToShow.Except(SLActions).ToList();
                _room.RoomDataCenter.EventsToShow = _room.RoomDataCenter.EventsToShow.Except(SSActions).ToList();
                _room.RoomDataCenter.EventsToShow = _room.RoomDataCenter.EventsToShow.Except(IMActions).ToList();

                // Add them to the end of the list
                _room.RoomDataCenter.EventsToShow.AddRange(SLActions);
                _room.RoomDataCenter.EventsToShow.AddRange(SSActions);
                _room.RoomDataCenter.EventsToShow.AddRange(IMActions);
            }

            // delete the sleeped player action
            if (_room.RoomDataCenter.EventsToShow.Any(anevent => anevent.eventAction.Equals(PlayerAction.Sleep)))
            {
                EventAction sleepyevent =
                    _room.RoomDataCenter.EventsToShow.FirstOrDefault(anevent =>
                        anevent.eventAction.Equals(PlayerAction.Sleep));
                if (sleepyevent.eventActorId != sleepyevent.eventVictimId) // to avoid self action delete
                {
                    _room.RoomDataCenter.EventsToShow.RemoveAll(anevent =>
                        anevent.eventActorId == sleepyevent.eventVictimId);
                }
            }


            foreach (var anEvent in _room.RoomDataCenter.EventsToShow)
            {
                var playerAction = anEvent.PlayerAction;
                if (playerAction is PlayerAction.Detect or PlayerAction.SaveLife or PlayerAction.Spy) continue;
                if (playerAction is PlayerAction.Kill or PlayerAction.Snipe)
                {
                    someOneKilled = true;
                }

                await NightEventShow(anEvent, _room.RoomDataCenter.PanelTimeMedium);
            }
        }

        if (TryGetPlayerByCardSymbol(CardSymbols.Qals_Qa, out var qalsGameCard) && !qalsGameCard.IsAlive &&
            !qalsGameCard.IsOneTimeAbilityUsed)
        {
            // OsamaAction Waiting for KilledQals to choose

            await QalsProcedures(eventsHandler);
        }

        if (TryGetPlayerByCardSymbol(CardSymbols.MafiaBomber_Bo, out var bomberGameCard) && !bomberGameCard.IsAlive &&
            !bomberGameCard.IsOneTimeAbilityUsed)
        {
            // OsamaAction Waiting for KilledBomber to choose
            await BomberProcedures(eventsHandler);
        }
    }

    private async Task QalsProcedures(EventsHandler eventsHandler)
    {
        _room.RoomGameLifeCycle.IsQalsBomberChoosing = true;
        _room.RoomGameLifeCycle.WhichQalsBomberChoosing = CardSymbols.Qals_Qa;
        QalsBomberTimer += _room.RoomDataCenter.PanelTimeLong;
        _room.RoomGameLifeCycle.BotQalsDecision(_room.RoomDataCenter.PanelTimeLong);
        await eventsHandler.ShowQalsBomberPanel(CardSymbols.Qals_Qa, _room,
            _room.RoomGameLifeCycle._QalsBomberTimer); // send to players********* // 3333333333333
        await eventsHandler.ShowQalsBomberResultPanel(_room.RoomGameLifeCycle._QalsBomberChoice, _room,
            _room.RoomGameLifeCycle._QalsBomberTimer); // send to players********* // 4444444444444
    }

    /// <summary>
    /// Bomber has been killed and have to choose who to kill (Shows a panel, allow player to choose)
    /// </summary>
    private async Task BomberProcedures(EventsHandler eventsHandler)
    {
        _room.RoomGameLifeCycle.IsQalsBomberChoosing = true;
        _room.RoomGameLifeCycle.WhichQalsBomberChoosing = CardSymbols.MafiaBomber_Bo;
        QalsBomberTimer += _room.RoomDataCenter.PanelTimeLong;
        _room.RoomGameLifeCycle.BotBomberDecision(_room.RoomDataCenter.PanelTimeLong);
        await eventsHandler.ShowQalsBomberPanel(CardSymbols.MafiaBomber_Bo, _room,
            _room.RoomGameLifeCycle._QalsBomberTimer); // send to players********* // 3333333333333
        await eventsHandler.ShowQalsBomberResultPanel(_room.RoomGameLifeCycle._QalsBomberChoice, _room,
            _room.RoomGameLifeCycle._QalsBomberTimer); // send to players********* // 4444444444444
    }

    private async Task NightEventShow(EventAction anEvent, int NEventDuration)
    {
        // -- show result
        var victimCardObject =
            _room.RoomDataCenter.AllPlayers.FirstOrDefault(aplayer => anEvent.eventVictimId == aplayer.PlayerId);
        var actorCardObject =
            _room.RoomDataCenter.AllPlayers.FirstOrDefault(aplayer => anEvent.eventActorId == aplayer.PlayerId);


        // EventLog.AddLog(eventMessage + victimPlayerName);

        var victimIsProtected = victimCardObject.IsProtected;
        if (anEvent.PlayerAction == PlayerAction.Kill && !victimIsProtected)
        {
            if (victimCardObject.SeatCard.CardAbility is CardAbilities.Immune &&
                !victimCardObject.IsOneTimeAbilityUsed) // LionHeart Ability 
            {
                // panelMessage.text = "أنه قلب الأسد !!";
                // playerName.text = "حاول المافيا إغتيال قلب الأسد لكنه فشل";
                // var message = "حاول المافيا إغتيال قلب الأسد لكنه فشل";
                actorCardObject.IsAlive = false;
                actorCardObject.IsProtected = false;
                victimCardObject.IsAlive = true;
                victimCardObject.IsOneTimeAbilityUsed = true;
                victimCardObject.IsProtected = true;
                victimCardObject.IsDiscovered = true;
            }
            else
            {
                victimCardObject.IsAlive = false;
            }
        }
        else if (anEvent.PlayerAction == PlayerAction.Snipe && !victimIsProtected)
        {
            victimCardObject.IsAlive = false;
        }
        else if (anEvent.PlayerAction == PlayerAction.Mute)
        {
            // OsamaAction Mafia Silent Muted Someone
        }
        else if (anEvent.PlayerAction == PlayerAction.Spy)
        {
            victimCardObject.IsDiscoveredForSpy = true;
        }
        else if (anEvent.PlayerAction == PlayerAction.Revive)
        {
            victimCardObject.IsAlive = true;
            actorCardObject.IsOneTimeAbilityUsed = true;

            // if (myGameCard.playerCard.CardSymbol == CardSymbols.MafiaSilent_Si &&
            //     victimPlayerCard.CardSymbol == CardSymbols.MafiaSilent_Si &&
            //     Prefs.JoinType.Contains("JoinRandomRoom"))
            // {
            //     ChallengeRecorder.instance.RecordChallenge(AllChallenges.self_muted_public_game_18);
            // }
            // cardImage.sprite = UnknownPlayerImage;
            // cardName.text = "?????";
            // cardNameContainer.color = new Color(0f, 0f, 0f);

            // OsamaAction Mafia Silent Muted Someone
            // IbrahimAction Mafia Sleeper sleep Someone

            /// make the procedures to relife a player and remove the red blood from him
            /// RemoveDead()
            victimCardObject.IsDiscovered = true;
        }
        // else if (anEvent.PlayerAction == PlayerAction.SaveLife)
        // {

        // }
        else if (anEvent.PlayerAction == PlayerAction.SaveSelf)
        {
            victimCardObject.IsAlive = true;
            victimCardObject.IsProtected = true;
        }

        if (victimIsProtected)
        {
            victimCardObject.IsAlive = true;
            victimCardObject.IsProtected = true;

            // difference between doctor and warrior
            bool isWarriorProtection = victimCardObject.SeatCard.CardAbility is CardAbilities.SaveSelf &&
                                       _room.RoomDataCenter.EventsToShow.Any(aevent =>
                                           aevent.eventAction == CardAbilities.SaveSelf);


            if (anEvent.PlayerAction == PlayerAction.Kill)
            {
                // panelMessage.text = "عملية إغتيال فاشلة";
                if (isWarriorProtection)
                {
                    // playerName.text = "قام المحارب بحماية نفسه من الإغتيال";
                }
                else
                {
                    // playerName.text = "قام الدكتور بحماية الضحية من الإغتيال";
                }

                // var message = "قام الدكتور بحماية الضحية من الإغتيال";
            }
            else if (anEvent.PlayerAction == PlayerAction.Snipe)
            {
                // panelMessage.text = "عملية قنص فاشلة";
                if (isWarriorProtection)
                {
                    // playerName.text = "قام االمحارب بحماية نفسه من القنص";
                }
                else
                {
                    // playerName.text = "قام الدكتور بحماية الضحية من القنص";
                }

                // var message = "قام الدكتور بحماية الضحية من القنص";
            }
            else
            {
                // panelMessage.text = "عملية إقصاء فاشلة";
                // playerName.text = "قام الدكتور بحماية الضحية من الإقصاء";
                // var message = "قام الدكتور بحماية الضحية من الإقصاء";
            }
        }

        await Task.Delay(NEventDuration * 1000);
    }
}