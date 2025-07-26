using MafiaServer.Pages.LobbySection.Models;

namespace MafiaServer.Pages.Models;

public class EventAction
{
    /// <summary>for example : (Day 1, Night 2, ...) </summary>

    public readonly string eventActorId;

    public readonly string eventVictimId;
    public readonly string eventAction = "";

    public readonly PlayerAction PlayerAction;
    // public DayNightEvent()
    // {
    // }

    private EventAction(string eventActorId, string eventVictimId, string eventAction,
        PlayerAction playerAction)
    {
        this.eventActorId = eventActorId;
        this.eventVictimId = eventVictimId;
        this.eventAction = eventAction;
        PlayerAction = playerAction;
    }


    public static EventAction Parse(string actionMessage)
    {
        string[] actionData = actionMessage.Split('|');
        EventAction dayNightEvent = new EventAction
        (
            eventActorId: actionData[0],
            eventAction: actionData[1],
            eventVictimId: actionData[2],
            playerAction: PlayerActionEx.FromCode(actionData[1])
        );
        return dayNightEvent;
    }

    public override string ToString()
    {
        return
            $"[eventActorId:{eventActorId}, eventVictimId:{eventVictimId}, eventAction:{eventAction}, PlayerAction:{PlayerAction}]";
    }
}



public static class PlayerActionEx
{
    public static string ToCode(this PlayerAction playerAction)
    {
        return playerAction switch
        {
            PlayerAction.Vote => "V",
            PlayerAction.Vote3X => "V3",
            PlayerAction.SkipVote => "SK",
            PlayerAction.Snipe => "S",
            PlayerAction.SaveLife => "SL",
            PlayerAction.Kill => "K",
            PlayerAction.Detect => "D",
            PlayerAction.Mute => "M",
            PlayerAction.Bomb => "B",
            PlayerAction.Qals => "Q",
            PlayerAction.Sleep => "SLP",
            PlayerAction.Revive => "REV",
            PlayerAction.Spy => "SPY",
            PlayerAction.AntiVote => "AVO",
            PlayerAction.SaveSelf => "SS",
            PlayerAction.Immune => "IM",
            _ => throw new ArgumentOutOfRangeException(nameof(playerAction), playerAction, null)
        };
    }

    public static PlayerAction FromCode(string playerAction)
    {
        return playerAction switch
        {
            "V" => PlayerAction.Vote,
            "V3" => PlayerAction.Vote3X,
            "SK" => PlayerAction.SkipVote,
            "S" => PlayerAction.Snipe,
            "SL" => PlayerAction.SaveLife,
            "K" => PlayerAction.Kill,
            "D" => PlayerAction.Detect,
            "M" => PlayerAction.Mute,
            "B" => PlayerAction.Bomb,
            "Q" => PlayerAction.Qals,
            "SLP" => PlayerAction.Sleep,
            "REV" => PlayerAction.Revive,
            "SPY" => PlayerAction.Spy,
            "AVO" => PlayerAction.AntiVote,
            "SS" => PlayerAction.SaveSelf,
            "IM" => PlayerAction.Immune,
            _ => throw new ArgumentOutOfRangeException(nameof(playerAction), playerAction, null)
        };
    }

    public static string CreatePlayerAction(string playerId, PlayerAction action, string targetPlayerId)
    {
        return playerId + "|" + action.ToCode() + "|" + targetPlayerId;
    }
    
}