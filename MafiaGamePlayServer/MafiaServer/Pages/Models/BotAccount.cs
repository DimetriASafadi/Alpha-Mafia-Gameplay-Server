namespace MafiaServer.Pages.Models;

public class BotAccount
{
    public int id;
    public string name;
    public string image;
    public int level = 0;
    public int coins = 0;
    public int totalRound = 0;
    public int totalWins = 0;
    public int totalLoses = 0;
    public int mafiaWins = 0;
    public int citizenWins = 0;
    public int SniperWins = 0;
    public int doctorWins = 0;
    public int totalKills = 0;
}