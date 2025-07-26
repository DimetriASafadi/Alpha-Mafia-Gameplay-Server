using MafiaServer.Pages.LobbySection.Models;
using Microsoft.AspNetCore.SignalR;

namespace MafiaServer.Pages.GamePlayLogic;

public class SendDataHandler
{
    private IHubContext<GameHub> _hubContext;

    public SendDataHandler(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task ToClientsSendPlayersDetailsBegin(string roomid, string Psdetails)
    {
        // PlayerID|PlayerIsAlive|PlayerCardSymbol|PlayerUsedOneTimeAbility|PlayerMafiaInhereted|PlayerWasDiscovered|PlayerIsConnected
        // 845|1|Ma|1|1|1|1,845|1|Ma|1|1|1|1,845|1|Ma|1|1|1|1,845|1|Ma|1|1|1|1,845|1|Ma|1|1|1|1,845|1|Ma|1|1|1|1
        System.Console.WriteLine("SRGePlayersDetailsBegin Executed");
        await _hubContext.Clients.Group(roomid).SendAsync("SRGePlayersDetailsBegin", Psdetails);
    }
    public async Task ToClientsSendPlayersDetails(string roomid, string Psdetails)
    {
        // PlayerID|PlayerIsAlive|PlayerCardSymbol|PlayerUsedOneTimeAbility|PlayerMafiaInhereted|PlayerWasDiscovered|PlayerIsConnected
        // 845|1|Ma|1|1|1|1,845|1|Ma|1|1|1|1,845|1|Ma|1|1|1|1,845|1|Ma|1|1|1|1,845|1|Ma|1|1|1|1,845|1|Ma|1|1|1|1
        await _hubContext.Clients.Group(roomid).SendAsync("SRGePlayersDetails", Psdetails);
    }
    public async Task ToClientsSendAction(string roomid, string playerAction)
    {
        await _hubContext.Clients.Group(roomid).SendAsync("SRGetAction", playerAction);
    }
    public async Task ToClientsSendDayAction(string roomid, string playerAction)
    {
        await _hubContext.Clients.Group(roomid).SendAsync("SRGetDayAction", playerAction);
    }
    public async Task ToClientsSendNightAction(string roomid, string playerAction)
    {
        await _hubContext.Clients.Group(roomid).SendAsync("SRGetNightAction", playerAction);
    }
    public async Task ToClientsSendQalsBomberChoice(string roomid, string playerAction,string targetid, string qalsOrbomber)
    {
        await _hubContext.Clients.Group(roomid).SendAsync("SRGetQalsBomberAction", playerAction,targetid,qalsOrbomber);
    }
    public async Task ToClientsSendChangeTime(string roomid,int changeSeconds,int changeTo)
    {
        await _hubContext.Clients.Group(roomid).SendAsync("SRChangeTimeTo",changeSeconds, changeTo);
    }
    public async Task ToClientsSendDayResult(string roomid, ResponseDay dayresult)
    { // the Value Would be Vo|P4758 or Drw| or NoV|
        await _hubContext.Clients.Group(roomid).SendAsync("SRGetDayResult", dayresult);
    }
    public async Task ToClientsSendNightResult(string roomid, ResponseNight nightresult)
    {
        await _hubContext.Clients.Group(roomid).SendAsync("SRGetNightResult", nightresult);
    }
    public async Task ToClientsSendGameResult(string roomid, string gameresult)
    {
        await _hubContext.Clients.Group(roomid).SendAsync("SRGetGameResult", gameresult);
    }
    public async Task ToClientsSendTauntFromTo(string roomid, string playerTaunt)
    {
        await _hubContext.Clients.Group(roomid).SendAsync("SRShowPlayerTaunt", playerTaunt);
    }
    public async Task ToClientsSendCurrentPanel(string roomid, string eventAndpanel)
    {
        await _hubContext.Clients.Group(roomid).SendAsync("SRShowCurrentPanel", eventAndpanel);
    }
    public async Task ToClientsSendNewBoss(string roomid, string newmafia)
    {
        await _hubContext.Clients.Group(roomid).SendAsync("SRGetNewMafia", newmafia);
    }
}