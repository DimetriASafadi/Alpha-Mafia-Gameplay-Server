namespace MafiaServer.Pages.LobbySection.Models;

public class MatchSetting
{
    public string SettingName { get; set; } = "";
    public int SettingPlayersCount { get; set; } = 7;
    public bool SettingChatEnable { get; set; } = true;
    public int SettingMafiaCount { get; set; } = 2;
    public int SettingDayTimer { get; set; } = 30;
    public int SettingNightTimer { get; set; } = 30;
    public int SettingEventTimer { get; set; } = 5;
    public bool SettingEmotesAvailable { get; set; } = true;
    public bool SettingRevealMafiaToMafia { get; set; } = true;
    public bool SettingElimenateRandomDrawVote { get; set; } = true;
    public bool SettingHistoryPanel { get; set; } = true;
    public List<string> SettingChoosenMafia { get; set; } = new List<string>();
    public List<string> SettingChoosenCivilians { get; set; } = new List<string>();

    public MatchSetting()
    {
    }

    public MatchSetting(string settingName, int settingPlayersCount, bool settingChatEnable, int settingMafiaCount,
        int settingDayTimer, int settingNightTimer, bool settingEmotesAvailable, bool settingRevealMafiaToMafia,
        bool settingElimenateRandomDrawVote, bool settingHistoryPanel, List<string> settingChoosenMafia,
        List<string> settingChoosenCivilians)
    {
        SettingName = settingName;
        SettingPlayersCount = settingPlayersCount;
        SettingChatEnable = settingChatEnable;
        SettingMafiaCount = settingMafiaCount;
        SettingDayTimer = settingDayTimer;
        SettingNightTimer = settingNightTimer;
        SettingEmotesAvailable = settingEmotesAvailable;
        SettingRevealMafiaToMafia = settingRevealMafiaToMafia;
        SettingElimenateRandomDrawVote = settingElimenateRandomDrawVote;
        SettingHistoryPanel = settingHistoryPanel;
        SettingChoosenMafia = settingChoosenMafia;
        SettingChoosenCivilians = settingChoosenCivilians;
    }
}