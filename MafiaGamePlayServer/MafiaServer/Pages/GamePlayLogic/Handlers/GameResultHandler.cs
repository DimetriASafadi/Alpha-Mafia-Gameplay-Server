using MafiaServer.Pages.LobbySection.Models;
using Microsoft.AspNetCore.SignalR;

namespace MafiaServer.Pages.GamePlayLogic;

public class GameResultHandler
{
    private IHubContext<GameHub> _hubContext;
    private GameActionsHandler _gameActionsHandler;
    private Room _room;


    public GameResultHandler(IHubContext<GameHub> hubContext, GameActionsHandler gameActionsHandler, Room room)
    {
        _hubContext = hubContext;
        _gameActionsHandler = gameActionsHandler;
        _room = room;
    }


    public void CheckWinLoseCondition()
    {
        System.Console.WriteLine("CheckWinLoseCondition");

        // if mafia more than or equals Civilians -- Mafia Win
        // if Civilians kill all mafias -- Civilians Win
        int MafiaCount = 0;
        int CiviliansCount = 0;
        foreach (var aPlayerseat in _room.RoomDataCenter.AllPlayers)
        {
            if (aPlayerseat.IsAlive &&
                aPlayerseat.SeatCard.CardTeam.Equals("M"))
            {
                MafiaCount++;
            }
            else if (aPlayerseat.IsAlive &&
                     aPlayerseat.SeatCard.CardTeam.Equals("C"))
            {
                CiviliansCount++;
            }
        }

        // [ahmed] todo: check if this is correct, it may give false win to the mafia
        if (MafiaCount >= CiviliansCount)
        {
            _room.RoomDataCenter.LastActionInStep = "GameResult";
            // if (MyGameCard.playerCard.CardTeam is CardTeams.Mafia)
            // {
            //     if (MyGameCard.playerIsAlive)
            //     {
            //         XPActions[ExpPointsTypes.MafiaStayedAlive]++;
            //     }
            //
            //     XPActions[ExpPointsTypes.MafiaWonGame]++;
            // }
            // else
            // {
            //     XPActions[ExpPointsTypes.CitizenLostGame]++;
            // }
            System.Console.WriteLine("Mafia Won the game");
            _room.RoomDataCenter.GameFinished = true;
            _room.CRoomState = RoomState.Finished;
            // gameObject.GetComponent<WinProcedure>().StartCoroutine("ShowGameResult", 0);
        }
        else if (MafiaCount == 0)
        {
            _room.RoomDataCenter.LastActionInStep = "GameResult";
            // if (MyGameCard.playerCard.CardTeam is CardTeams.Civilian)
            // {
            //     if (MyGameCard.playerIsAlive)
            //     {
            //         XPActions[ExpPointsTypes.CitizenStayedAlive]++;
            //     }
            //
            //     XPActions[ExpPointsTypes.CitizenWonGame]++;
            // }
            // else
            // {
            //     XPActions[ExpPointsTypes.MafiaLostGame]++;
            // }
            System.Console.WriteLine("Citizens Won the game");
            _room.RoomDataCenter.GameFinished = true;
            _room.CRoomState = RoomState.Finished;
            // gameObject.GetComponent<WinProcedure>().StartCoroutine("ShowGameResult", 1);
        }
    }
}