namespace MafiaServer.Pages.LobbySection.Models;

public class Card
{
    public int CardId;
    public string CardName;
    public string CardDetails;

    // public string CardArabicName()
    // {
    //     var CardNametrans = LocalizationSettings.StringDatabase.GetLocalizedString("GameTexts", CardLangKey + "Name");
    //     return CardNametrans;
    // }
    public string CardImagePath;

    public string CardAbility;

    // public string CardDetails()
    // {
    //     var CardDetailstrans = LocalizationSettings.StringDatabase.GetLocalizedString("GameTexts", CardLangKey + "Desc");
    //     return CardDetailstrans;
    // }
    public string CardTeam;
    public string CardSymbol;
    public bool CardAbilityIsOneTime;
    public string CardAbilityTime;
    public string CardLangKey;
    public string ColorBasedOnTeam;
    public int ChanceItem;

    public Card()
    {
    }

    public Card(int cardId, string cardName, string cardAbility, string cardTeam, string cardSymbol,
        bool cardAbilityIsOneTime, string cardAbilityTime)
    {
        CardId = cardId;
        CardName = cardName;
        CardAbility = cardAbility;
        CardTeam = cardTeam;
        CardSymbol = cardSymbol;
        CardAbilityIsOneTime = cardAbilityIsOneTime;
        CardAbilityTime = cardAbilityTime;
    }

    public Card(int cardId, string cardName, string cardDetails, string cardImagePath, string cardAbility,
        string cardTeam, string cardSymbol, bool cardAbilityIsOneTime, string cardAbilityTime, string cardLangKey,
        string colorBasedOnTeam, int chanceItem)
    {
        CardId = cardId;
        CardName = cardName;
        CardDetails = cardDetails;
        CardImagePath = cardImagePath;
        CardAbility = cardAbility;
        CardTeam = cardTeam;
        CardSymbol = cardSymbol;
        CardAbilityIsOneTime = cardAbilityIsOneTime;
        CardAbilityTime = cardAbilityTime;
        CardLangKey = cardLangKey;
        ColorBasedOnTeam = colorBasedOnTeam;
        ChanceItem = chanceItem;
    }
}

public static class CardSymbols
{
    public const string Civilian_C1 = "C1";
    public const string Civilian_C2 = "C2";
    public const string Civilian_C3 = "C3";
    public const string Detective_De = "De";
    public const string Doctor_Do = "Do";
    public const string Qals_Qa = "Qa";
    public const string Mayor_My = "My";
    public const string Sniper_Sn = "Sn";
    public const string MafiaBomber_Bo = "Bo";
    public const string MafiaBoss_Mb = "Mb";
    public const string MafiaNormal_Mn = "Mn";
    public const string MafiaSilent_Si = "Si";
    public const string MafiaSleeper_Slp = "Slp";
    public const string WiseMan = "WM";
    public const string MafiaSpy = "Spy";
    public const string Princess = "Pri";
    public const string Warrior = "War";
    public const string LionHeart = "Li";
    public const string Joker = "Jo";
}

public static class CardTeams
{
    public const string Civilian = "C";
    public const string Mafia = "M";
}

public static class CardAbilityTimes
{
    public const string DayOnly_D = "D";
    public const string NightOnly_N = "N";
    public const string Both_DN = "DN";
}

public static class CardAbilities
{
    /// <summary> for C1,C2,C3 </summary>
    public const string Vote = "Vote";

    /// <summary> for card symbol <see cref="CardSymbols.Qals_Qa"/> only </summary>
    public const string Pull = "Pull";

    /// <summary> for card symbol <see cref="CardSymbols.Sniper_Sn"/> only </summary>
    public const string Snipe = "Snipe";

    /// <summary> for card symbol <see cref="CardSymbols.Detective_De"/> only </summary>
    public const string ShowCard = "ShowCard";

    /// <summary> for card symbol <see cref="CardSymbols.Doctor_Do"/> only </summary>
    public const string SaveLife = "SaveLife";

    /// <summary> for card symbol <see cref="CardSymbols.MafiaBomber_Bo"/> only </summary>
    public const string Bomb = "Bomb";

    /// <summary> for card symbol <see cref="CardSymbols.MafiaBoss_Mb"/> only </summary>
    public const string Kill = "Kill";

    /// <summary> for card symbol <see cref="CardSymbols.MafiaNormal_Mn"/> </summary>
    public const string Non = "Non";

    /// <summary> for card symbol <see cref="CardSymbols.Mayor_My"/> </summary>
    public const string V3 = "V3";

    /// <summary> for card symbol <see cref="CardSymbols.MafiaSilent_Si"/> </summary>
    public const string Mute = "Mute";

    /// <summary> for card symbol <see cref="CardSymbols.MafiaSleeper_Slp"/> </summary>
    public const string Sleep = "Sleep";

    /// <summary> for card symbol <see cref="CardSymbols.WiseMan"/> </summary>
    public const string Revive = "Revive";

    /// <summary> for card symbol <see cref="CardSymbols.MafiaSpy"/> </summary>
    public const string Spy = "Spy";

    /// <summary> for card symbol <see cref="CardSymbols.Princess"/> </summary>
    public const string AntiVote = "AntiVote";

    /// <summary> for card symbol <see cref="CardSymbols.Warrior"/> </summary>
    public const string SaveSelf = "SaveSelf";

    /// <summary> for card symbol <see cref="CardSymbols.LionHeart"/> </summary>
    public const string Immune = "Immune";

    /// <summary> for card symbol <see cref="CardSymbols.Joker"/> </summary>
    public const string RandomButton = "RandomButton";

    /// <summary> for card symbol <see cref="CardSymbols.SkipVote"/> </summary>
    public const string SkipVote = "SkipVote";
}