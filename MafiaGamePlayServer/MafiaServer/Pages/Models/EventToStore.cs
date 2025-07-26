namespace MafiaServer.Pages.Models;

public class EventToStore
{
    public string ActorId { get; set; }
    public string ActionCode { get; set; }
    public string TargetId { get; set; }

    public EventToStore(string actorId, string actionCode, string targetId)
    {
        ActorId = actorId;
        ActionCode = actionCode;
        TargetId = targetId;
    }
}