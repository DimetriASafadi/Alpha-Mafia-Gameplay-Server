using System.Diagnostics;
using MafiaServer.Pages.LobbySection.Models;
using Microsoft.AspNetCore.SignalR;

namespace MafiaServer.Pages.GamePlayLogic;

public class GameLifeCycle
{
    // Get mode Type public or custom
    // if custom check if there is custom settings set / if not put the default settings
    // Getting Players to Seats and close the Server // update the Players count and send it to Game Lobby
    // Make Sure the what is the mode and bind it 

    private static readonly Random _random = new Random(); // Create a single instance

    private readonly IHubContext<GameHub> _hubContext;
    public GameActionsHandler _gameActionsHandler;
    private VotingResultHandler _votingResultHandler;
    private EventsHandler _eventsHandler;
    public GameResultHandler _gameResultHandler;
    private SendDataHandler _sendDataHandler;
    private CryptDataHandler _cryptDataHandler;

    public bool IsQalsBomberChoosing = false;
    public string WhichQalsBomberChoosing = "Q";


    private int FixedDuration = 2000;
    private CancellationTokenSource _cts;
    public CancellationTokenSource _QalsBomberTimer;
    public string _QalsBomberChoice = "";


    private Room _room;
    private Stopwatch _stopwatch = new Stopwatch();

    public GameLifeCycle(Room selfRoom, IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
        _room = selfRoom;
        _cryptDataHandler = new CryptDataHandler();
        _gameActionsHandler = new GameActionsHandler(_room, _hubContext);
        _votingResultHandler = new VotingResultHandler();
        _sendDataHandler = new SendDataHandler(_hubContext);
        _eventsHandler = new EventsHandler(_sendDataHandler);
        _gameResultHandler = new GameResultHandler(_hubContext, _gameActionsHandler, _room);
    }

    public async Task StartTheGame()
    {
        while (!_room.RoomDataCenter.GameFinished)
        {
            // Night To Day
            if (_room.RoomDataCenter.GameFinished)
            {
                break;
            }
            Console.WriteLine("Change Night to Day "+_room.RoomDataCenter.DayCount+" RoomId " + _room.RoomId);
            _room.RoomDataCenter.LastActionInStep = StepCode.NightToDayChange_NTD;
            await ChangeFromNightToDay(); // Wait for 7 seconds
            if (_room.RoomDataCenter.GameFinished)
            {
                break;
            }
            // Day Voting Timer
            Console.WriteLine("Day Voting started. "+_room.RoomDataCenter.DayCount+" RoomId " + _room.RoomId);
            _room.RoomDataCenter.LastActionInStep = StepCode.DayVotingTime_DVT;
            await StartDayTimer();
            if (_room.RoomDataCenter.GameFinished)
            {
                break;
            }
            // Day Voting Result
            Console.WriteLine("Viewing Day Result. "+_room.RoomDataCenter.DayCount+" RoomId " + _room.RoomId);
            _room.RoomDataCenter.LastActionInStep = StepCode.DayVotingResult_DVR;
            await ViewDayResult();
            if (_room.RoomDataCenter.GameFinished)
            {
                break;
            }
            // Day To Night
            Console.WriteLine("Change Day to Night "+_room.RoomDataCenter.DayCount+" RoomId " + _room.RoomId);
            _room.RoomDataCenter.LastActionInStep = StepCode.DayToNightChange_DTN;
            await ChangeFromDayToNight(); // Wait for 7 seconds
            if (_room.RoomDataCenter.GameFinished)
            {
                break;
            }
            // Night Action Time
            Console.WriteLine("Night Actions started. "+_room.RoomDataCenter.DayCount+" RoomId " + _room.RoomId);
            _room.RoomDataCenter.LastActionInStep = StepCode.NightActionsTime_NAT;
            await StartNightTimer();
            if (_room.RoomDataCenter.GameFinished)
            {
                break;
            }
            // Night Action Result
            Console.WriteLine("Viewing Night Result. "+_room.RoomDataCenter.DayCount+" RoomId " + _room.RoomId);
            _room.RoomDataCenter.LastActionInStep = StepCode.NightActionsResult_NAR;
            await ViewNightResult();
        }
        Console.WriteLine("Game Finished" + _room.RoomId);
    }
    private async Task StartDayTimer()
    {
        await _sendDataHandler.ToClientsSendChangeTime(_room.RoomId,_room.Settings.SettingDayTimer, ActionStep.DayVotingTime_1);
        _stopwatch.Restart();
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        try
        {
            // Use Task.Delay with a CancellationToken to simulate a timer
            // BotsDayAction(_room.Settings.SettingDayTimer);
            // await Task.Delay(_room.RoomDataCenter.matchSetting.SettingDayTimer, _cts.Token);
            RefreshBotsDecision();
            _=BotsDayAction(_room.Settings.SettingDayTimer);
            // await Task.WhenAny(
            //     Task.Delay(_room.Settings.SettingDayTimer*1000, _cts.Token));
            await Task.WhenAny(CheckDayDecisionsIsDone(_room.Settings.SettingDayTimer),
                Task.Delay(_room.Settings.SettingDayTimer*1000, _cts.Token));
            // await Task.WhenAll(BotsDayAction(_room.Settings.SettingDayTimer),
            //     Task.Delay(_room.RoomDataCenter.matchSetting.SettingDayTimer, _cts.Token));
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Day Timer was canceled.");
        }
        _stopwatch.Stop();
    }
    private async Task ViewDayResult()
    {
        await _sendDataHandler.ToClientsSendChangeTime(_room.RoomId,_room.Settings.SettingDayTimer, ActionStep.DayVotingResult_2);
        _stopwatch.Restart();
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        try
        {
            _room.RoomDataCenter.DayDecitionCount = 0;
            RefreshBotsDecision();
            ClearMutes();
            // Use Task.Delay with a CancellationToken to simulate a timer
            var voteResult = _votingResultHandler.GetVotingResult(_room);
            bool isDraw = false;
            // when draw, master will randomly select a player to eliminate, other player will wait for master to decide
            if (voteResult is VoteResult.Draw draw &&
                ((!string.IsNullOrEmpty(_room.Settings.SettingName) &&
                  _room.Settings.SettingElimenateRandomDrawVote) ||
                 string.IsNullOrEmpty(_room.Settings.SettingName)))
            {
                isDraw = true;

                //select random player from draw
                var drawPlayerIds = draw.playerIds;
                var randomIndex = _random.Next(0, drawPlayerIds.Length);
                var randomPlayerId = draw.playerIds[randomIndex];
                System.Console.WriteLine($"Draw, i (the server) will eliminate [{randomPlayerId}]");
                var chosenexecutedPlyaer =
                    _room.RoomDataCenter.AllPlayers.FirstOrDefault(ap => ap.PlayerId == randomPlayerId);
                voteResult = new VoteResult.Voted(draw.votes, randomPlayerId, chosenexecutedPlyaer);
            }


            FixedDuration = 0;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (voteResult is VoteResult.Draw) // expected to never run
            {
                FixedDuration += _room.RoomDataCenter.PanelTimeShort;
                // StartCoroutine(panelsHandlerFuncs.ShowNoVotePanel(isDraw: true)); // send to players********
                await _eventsHandler.ShowNoVotePanel(true, _room); // 1111111111111
            }
            else if (voteResult is VoteResult.NoVotes)
            {
                FixedDuration += _room.RoomDataCenter.PanelTimeShort;
                // StartCoroutine(panelsHandlerFuncs.ShowNoVotePanel(isDraw: false)); // send to players********
                await _eventsHandler.ShowNoVotePanel(false, _room); // 1111111111111
            }
            else if (voteResult is VoteResult.Voted voted)
            {
                var cardSymbol = voted.votedPlayerGameCard.SeatCard.CardSymbol;
                // AllGameDetailsScript.Instance.CheckMyVoteResult(voted.votedPlayerGameCard);
                if (cardSymbol is CardSymbols.Qals_Qa or CardSymbols.MafiaBomber_Bo)
                {
                    // panelsHandlerFuncs.HideAllPanels();
                    IsQalsBomberChoosing = true;
                    WhichQalsBomberChoosing = cardSymbol;
                    RefreshBotsDecision();
                    // await StartCoroutine(panelsHandlerFuncs.ShowVotedPlayerPanel(voted, isDraw));
                    await _eventsHandler.ShowVotedPlayerPanel(voted, isDraw, _room); // 2222222222222
                    if (cardSymbol == CardSymbols.Qals_Qa) BotQalsDecision(_room.RoomDataCenter.PanelTimeLong);
                    if (cardSymbol == CardSymbols.MafiaBomber_Bo) BotBomberDecision(_room.RoomDataCenter.PanelTimeLong);
                    await _eventsHandler.ShowQalsBomberPanel(cardSymbol, _room,
                        _QalsBomberTimer); // send to players********* // 3333333333333
                    await _eventsHandler.ShowQalsBomberResultPanel(_QalsBomberChoice, _room,
                        _QalsBomberTimer); // send to players********* // 4444444444444
                    voted.votedPlayerGameCard.IsAlive = false;
                    voted.votedPlayerGameCard.IsOneTimeAbilityUsed = true;
                }
                else
                {
                    // Debug.Log("WTF " + voteResultStr);
                    // StartCoroutine(panelsHandlerFuncs.ShowVotedPlayerPanel(voted, isDraw)); // send to players************
                    await _eventsHandler.ShowVotedPlayerPanel(voted, isDraw, _room); // 22222222222222
                    // OsamaAction Voted On Player
                }
            }

            await Task.Delay(FixedDuration, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Console.WriteLine("Night Timer was canceled.");
        }
        _stopwatch.Stop();
        EliminatePlayersCheck();
        await MafiaBossDeathCheck();
        await _gameActionsHandler.UpdatePlayersDetails();
        _gameResultHandler.CheckWinLoseCondition();
        _room.RoomDataCenter.EventsToShow.Clear();
    }
    private async Task ChangeFromDayToNight()
    {
        await _sendDataHandler.ToClientsSendChangeTime(_room.RoomId,7000, ActionStep.DayToNightChange_3);
        RefreshBotsDecision();
        ClearMutes();
        await MafiaBossDeathCheck();
        ClearJokerAbilityBlock();
        _room.RoomDataCenter.EventsToShow.Clear();
        _stopwatch.Restart();
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        try
        {
            // Use Task.Delay with a CancellationToken to simulate a timer
            await Task.Delay(7000, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Console.WriteLine("Night Timer was canceled.");
        }
        _stopwatch.Stop();
        _room.RoomDataCenter.DayOrNight = DayNight.Night;
        await _gameActionsHandler.UpdatePlayersDetails();
    }

    private async Task StartNightTimer()
    {
        await _sendDataHandler.ToClientsSendChangeTime(_room.RoomId,_room.Settings.SettingNightTimer, ActionStep.NightActionsTime_4);
        RefreshBotsDecision();
        ClearMutes();
        _room.RoomDataCenter.EventsToShow.Clear();
        _stopwatch.Restart();
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        try
        {
            // // Use Task.Delay with a CancellationToken to simulate a timer
            // BotsNightAction(_room.Settings.SettingNightTimer);
            // await Task.Delay(_room.RoomDataCenter.matchSetting.SettingNightTimer, _cts.Token);

            await Task.WhenAll(BotsNightAction(_room.Settings.SettingNightTimer),
                Task.Delay(_room.Settings.SettingNightTimer, _cts.Token));
        }
        catch (OperationCanceledException)
        {
            // Console.WriteLine("Night Timer was canceled.");
        }
        _stopwatch.Stop();
    }

    private async Task ViewNightResult()
    {
        await _sendDataHandler.ToClientsSendChangeTime(_room.RoomId,_room.Settings.SettingNightTimer, ActionStep.NightActionsResult_5);
        FixedDuration = 2000;
        // if (_room.RoomDataCenter.EventsToShow)
        // {
        //     
        // }
        RefreshBotsDecision();
        _stopwatch.Restart();
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        try
        {
            await _gameActionsHandler.ShowNightEventPanel(_eventsHandler);

            // Use Task.Delay with a CancellationToken to simulate a timer
            await Task.Delay(FixedDuration, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Console.WriteLine("Night Timer was canceled.");
        }
        _stopwatch.Stop();
        EliminatePlayersCheck();
        await MafiaBossDeathCheck();
        await _gameActionsHandler.UpdatePlayersDetails();
        _gameResultHandler.CheckWinLoseCondition();
        _room.RoomDataCenter.EventsToShow.Clear();
    }

    private async Task ChangeFromNightToDay()
    {
        RefreshBotsDecision();
        ClearProtection();
        ClearSleeps();
        await MafiaBossDeathCheck();
        ClearJokerAbilityBlock();
        _room.RoomDataCenter.EventsToShow.Clear();
        await _sendDataHandler.ToClientsSendChangeTime(_room.RoomId,7000, ActionStep.NightToDayChange_6);
        _stopwatch.Restart();
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        try
        {
            _room.RoomDataCenter.DayDecitionCount = 0;
            // Use Task.Delay with a CancellationToken to simulate a timer
            await Task.Delay(7000, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Console.WriteLine("Night Timer was canceled.");
        }
        _stopwatch.Stop();
        _room.RoomDataCenter.CurrentDayCount++;
        _room.RoomDataCenter.DayCount++;
        _room.RoomDataCenter.DayOrNight = DayNight.Day;
        await _gameActionsHandler.UpdatePlayersDetails();
    }


    public void StopCurrentTimer()
    {
        // Cancel the ongoing timer
        _cts?.Cancel();
        Console.WriteLine("Timer canceled.");
    }
    private async Task BotsDayAction(int MaxTimeToWait)
    {
        List<Task> botsdecisions = new List<Task>();
        foreach (var aPlayerseat in _room.RoomDataCenter.AllPlayers)
        {
            var waitTime = MaxTimeToWait > 0 ? _random.Next(4, MaxTimeToWait) : _random.Next(1, 3);
            // var waitTime = MaxTimeToWait > 0 ? Random.Range(4, PanelTimeHalf) : Random.Range(1, 3);
            botsdecisions.Add(aPlayerseat.BotController.TakeDayDecition(waitTime));
        }

        await Task.WhenAll(botsdecisions);
        // BotMaySendTaunt();
    }
    private async Task BotsNightAction(int MaxTimeToWait)
    {
        List<Task> botsdecisions = new List<Task>();
        foreach (var aPlayerseat in _room.RoomDataCenter.AllPlayers)
        {
            var waitTime = MaxTimeToWait > 0 ? _random.Next(4, MaxTimeToWait) : _random.Next(1, 3);
            // var waitTime = MaxTimeToWait > 0 ? Random.Range(4, PanelTimeHalf) : Random.Range(1, 3);

            // hint: mafia can mute and kill others in the same round
            botsdecisions.Add(aPlayerseat.BotController.TakeNightDecition(waitTime));
            botsdecisions.Add(aPlayerseat.BotController.TakeMafiaKill(waitTime));
        }

        await Task.WhenAll(botsdecisions);
        // BotMaySendTaunt();
    }
    // private async Task BotsDayAction(int MaxTimeToWait)
    // {
    //     foreach (var aPlayerseat in _room.RoomDataCenter.AllPlayers)
    //     {
    //         var waitTime = MaxTimeToWait > 0 ? _random.Next(4, MaxTimeToWait) : _random.Next(1, 3);
    //         // var waitTime = MaxTimeToWait > 0 ? Random.Range(4, PanelTimeHalf) : Random.Range(1, 3);
    //         _ = aPlayerseat.BotController.TakeDayDecition(waitTime);
    //     }
    //     // BotMaySendTaunt();
    // }
    
    // private void BotsNightAction(int MaxTimeToWait)
    // {
    //     foreach (var aPlayerseat in _room.RoomDataCenter.AllPlayers)
    //     {
    //         var waitTime = MaxTimeToWait > 0 ? _random.Next(4, MaxTimeToWait) : _random.Next(1, 3);
    //         // var waitTime = MaxTimeToWait > 0 ? Random.Range(4, PanelTimeHalf) : Random.Range(1, 3);
    //
    //         // hint: mafia can mute and kill others in the same round
    //         _ = aPlayerseat.BotController.TakeNightDecition(waitTime);
    //         _ = aPlayerseat.BotController.TakeMafiaKill(waitTime);
    //     }
    //
    //     // BotMaySendTaunt();
    // }
    public void BotQalsDecision(int MaxTimeToWait)
    {
        foreach (var aPlayerseat in _room.RoomDataCenter.AllPlayers)
        {
            var randomValue = MaxTimeToWait <= 0 ? 1 : _random.Next(4, MaxTimeToWait);
            _ = aPlayerseat.BotController.TakeQalsDecision(randomValue);
        }
    }
    public void BotBomberDecision(int MaxTimeToWait)
    {
        foreach (var aPlayerseat in _room.RoomDataCenter.AllPlayers)
        {
            var randomValue = MaxTimeToWait > 0 ? _random.Next(4, MaxTimeToWait) : 1;
            _ = aPlayerseat.BotController.TakeBomberDecision(randomValue);
        }
    }
    public void RefreshBotsDecision()
    {
        IsQalsBomberChoosing = false;
        WhichQalsBomberChoosing = "";
        _QalsBomberChoice = "";
        foreach (var aPlayerseat in _room.RoomDataCenter.AllPlayers)
        {
            aPlayerseat.DidDecide = false;
            aPlayerseat.DidKill = false;
        }
    }
    public void ClearMutes()
    {
        foreach (var aPlayerseat in _room.RoomDataCenter.AllPlayers)
        {
            aPlayerseat.IsMuted = false;
        }
    }
    public void ClearProtection()
    {
        foreach (var aPlayerseat in _room.RoomDataCenter.AllPlayers)
        {
            aPlayerseat.IsProtected = false;
        }
    }
    public void ClearSleeps()
    {
        foreach (var aPlayerseat in _room.RoomDataCenter.AllPlayers)
        {
            aPlayerseat.IsSleeping = false;
        }
    }
    public async Task MafiaBossDeathCheck()
    {
        var IsThereKiller = true;
        foreach (var aPlayerseat in _room.RoomDataCenter.AllPlayers)
        {
            if (aPlayerseat.SeatCard.CardTeam.Equals("M") &&
                aPlayerseat.IsMafiaInhereted &&
                !aPlayerseat.IsAlive)
            {
                aPlayerseat.IsMafiaInhereted = false;
                IsThereKiller = false;
                break;
            }
        }

        if (!IsThereKiller)
        {
            foreach (var aPlayerseat in _room.RoomDataCenter.AllPlayers)
            {
                if (aPlayerseat.SeatCard.CardTeam.Equals("M") &&
                    !aPlayerseat.IsMafiaInhereted &&
                    aPlayerseat.IsAlive)
                {
                    aPlayerseat.IsMafiaInhereted = true;
                    await _sendDataHandler.ToClientsSendNewBoss(_room.RoomId, aPlayerseat.PlayerId);
                    break;
                }
            }
        }
    }
    public void ClearJokerAbilityBlock()
    {
        foreach (var aPlayerseat in _room.RoomDataCenter.AllPlayers)
        {
            if (aPlayerseat.SeatCard.CardSymbol is CardSymbols.Joker && aPlayerseat.IsAlive)
            {
                aPlayerseat.IsOneTimeAbilityUsed = false;
            }

            ;
        }
    }
    public void EliminatePlayersCheck()
    {
        foreach (var aPlayerseat in _room.RoomDataCenter.AllPlayers)
        {
            if (aPlayerseat.IsAlive) continue;
            // aPlayer.transform.Find("DeathHolder").gameObject.SetActive(true);
            aPlayerseat.IsDiscovered = true;
        }
    }
    private int GetAlivePlayersCount()
    {
        return _room.RoomDataCenter.AllPlayers.Count(p => p.IsAlive);
    }
    private async Task CheckDayDecisionsIsDone(int dayTimeSeconds)
    {
        for (int i = 0; i < dayTimeSeconds; i++) // 30 seconds
        {
            // if (_room.RoomDataCenter.LastActionInStep != StepCode.DayVotingTime_DVT) return;
            if (_room.RoomDataCenter.DayDecitionCount >= GetAlivePlayersCount())
            {
                Console.WriteLine("✅ Counter reached "+_room.RoomDataCenter.DayDecitionCount+". Exiting early.");
                return;
            }
            Console.WriteLine($"⏳ GetAlivePlayersCount {GetAlivePlayersCount()} Decisions Taken {_room.RoomDataCenter.DayDecitionCount}");

            Console.WriteLine($"⏳ Second {i + 1}");
            await Task.Delay(1000); // wait 1 second
        }

        Console.WriteLine("⏰ 30 seconds passed. Counter never reached full decisions.");
    }
}