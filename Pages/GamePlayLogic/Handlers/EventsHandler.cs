using MafiaServer.Pages.LobbySection.Models;

namespace MafiaServer.Pages.GamePlayLogic;

public class EventsHandler
{
    private SendDataHandler _sendDataHandler;

    public EventsHandler(SendDataHandler sendDataHandler)
    {
        _sendDataHandler = sendDataHandler;
    }

    public async Task ShowVotedPlayerPanel(VoteResult.Voted voteResult, bool isDraw, Room _room)
    {
        // yield return new WaitForSeconds(2);
        PlayerSeat gameCard = voteResult.votedPlayerGameCard;
        var playerCard = gameCard.SeatCard;
        var votesCount = voteResult.votes;
        string votedPlayerId = voteResult.playerId;
        gameCard.IsAlive = false;

        if (playerCard.CardAbility is CardAbilities.AntiVote) // Princess Ability
        {
            // panelDescription.text = "لا يمكن إقصاء الأميرة بالتصويت";
            // panelInTitle.text = "إنها الأميرة !";
            gameCard.IsAlive = true;
            gameCard.IsDiscovered = true;
            ResponseDay responseDay = new ResponseDay(votedPlayerId,false,votesCount);
            await _sendDataHandler.ToClientsSendDayResult(_room.RoomId,
                responseDay);
            System.Console.WriteLine("Result is Princess");
        }
        else
        {
            if (!isDraw)
            {
                // panelDescription.text = "عدد الأصوات التي اجمعت على قتل اللاعب " + "???";
                // panelInTitle.text = "لقد صوت المواطنون على إقصاء";
                ResponseDay responseDay = new ResponseDay(votedPlayerId,false,votesCount);
                await _sendDataHandler.ToClientsSendDayResult(_room.RoomId,
                    responseDay);
                System.Console.WriteLine("Result is Voted On " + votedPlayerId);
            }
            else
            {
                // panelDescription.text = "سيتم إقصاء أحد اللاعبين عشوائيا";
                // panelInTitle.text = "النتيجة هي <color=#466BB7>التعادل </color> في التصويت";
                ResponseDay responseDay = new ResponseDay(votedPlayerId,true,votesCount);
                await _sendDataHandler.ToClientsSendDayResult(_room.RoomId,
                    responseDay);
                System.Console.WriteLine("Result is DrawVoted On " + votedPlayerId);
            }
        }

        await Task.Delay(_room.RoomDataCenter.PanelTimeMedium * 1000);
    }

    public async Task ShowNoVotePanel(bool isDraw, Room _room)
    {
        if (!isDraw)
        {
            ResponseDay responseDay = new ResponseDay(DayResult.Result_NoVotes,false,0);
            await _sendDataHandler.ToClientsSendDayResult(_room.RoomId,
                responseDay);
            System.Console.WriteLine("Result is No Votes");
        }
        else
        {
            ResponseDay responseDay = new ResponseDay(DayResult.Result_Draw,true,0);
            await _sendDataHandler.ToClientsSendDayResult(_room.RoomId,
                responseDay);
            System.Console.WriteLine("Result is Draw");
        }

        await Task.Delay(_room.RoomDataCenter.PanelTimeShort * 1000);
    }

    public async Task ShowQalsBomberPanel(string cardSymbol, Room _room, CancellationTokenSource QalsBomberTimer)
    {
        // if (cardSymbol == CardSymbols.Qals_Qa)
        // {
        //     // panelInTitle.text = LocalizationSettings.StringDatabase.GetLocalizedString("GameTexts", "PleaseWait");
        //     // panelDescription.text = LocalizationSettings.StringDatabase.GetLocalizedString("GameTexts", "QalsWarnning");
        //     // panelInTitle.text = "يرجى الإنتظار";
        //     // panelDescription.text = "يقوم القلص بمحاولة سحب لاعب اخر معه ";
        // }
        // else if (cardSymbol == CardSymbols.MafiaBomber_Bo)
        // {
        //     // panelInTitle.text = LocalizationSettings.StringDatabase.GetLocalizedString("GameTexts", "PleaseWait");
        //     // panelDescription.text = LocalizationSettings.StringDatabase.GetLocalizedString("GameTexts", "BomberWarnning");
        //     // panelInTitle.text = "يرجى الإنتظار";
        //     // panelDescription.text = "يقوم المافيا المفجر بتفجير نفسه بأحد اللاعبين وإقصاء كلاهما";
        // }
        Console.WriteLine("Waiting for " + cardSymbol + " to choose");
        await _sendDataHandler.ToClientsSendCurrentPanel(_room.RoomId,
            cardSymbol);

        QalsBomberTimer?.Cancel();
        QalsBomberTimer = new CancellationTokenSource();
        try
        {
            // Use Task.Delay with a CancellationToken to simulate a timer
            await Task.Delay(_room.RoomDataCenter.PanelTimeLong * 1000, QalsBomberTimer.Token);
        }
        catch (OperationCanceledException)
        {
            // Console.WriteLine("Night Timer was canceled.");
        }
    }

    public async Task ShowQalsBomberResultPanel(string playeraction, Room _room,
        CancellationTokenSource QalsBomberTimer)
    {
        // await _sendDataHandler.ToClientsSendQalsBomberChoice(_room.RoomId,
        //     playeraction);
        System.Console.WriteLine("Qals Bomber Result is " + playeraction);

        QalsBomberTimer?.Cancel();
        QalsBomberTimer = new CancellationTokenSource();
        try
        {
            // Use Task.Delay with a CancellationToken to simulate a timer
            await Task.Delay(_room.RoomDataCenter.PanelTimeMedium * 1000, QalsBomberTimer.Token);
        }
        catch (OperationCanceledException)
        {
            // Console.WriteLine("Night Timer was canceled.");
        }
    }
}