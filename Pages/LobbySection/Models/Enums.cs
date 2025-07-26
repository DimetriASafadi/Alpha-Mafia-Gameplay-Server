public static class RoomType
{
    
    public const string Public9 = "Public9";
    public const string PublicRank7 = "PublicRank7";
    public const string Public7 = "Public7";
    public const string Custom = "Custom";
    // Public7,
    // Public9,
    // PublicRank7,
    // Custom,
}

public static class RoomJoinResponse
{
    public const string RoomNotExist = "RoomNotExist";
    public const string RoomError = "RoomError";
}

public static class RoomState
{
    public const string Waiting = "Waiting";
    public const string SearchingPlayers = "SearchingPlayers";
    public const string Playing = "Playing";
    public const string Finished = "Finished";
}

public enum ReconnectStatus
{
    RejoinGame,
    NewGame
}

public static class ReconnectResponse
{
    public const string Successful = "Successful";
    public const string GameNotFound = "GameNotFound";
    public const string Failed = "Failed";
    public const string Finished = "Finished";
}

public enum DayNight
{
    Day,
    Night,
}

public enum PlayerAction
{
    /// <summary> Code: V </summary>
    Vote,

    /// <summary> Code: V3 </summary>"
    Vote3X,

    /// <summary> Code: SK </summary>"
    SkipVote,

    /// <summary> Code: S </summary>
    Snipe,

    /// <summary> Code: SL </summary>"
    SaveLife,

    /// <summary> Code: K </summary>
    Kill,

    /// <summary> Code: D </summary>
    Detect,

    /// <summary> Code: M </summary>
    Mute,

    /// <summary> Code: B </summary>
    Bomb,

    /// <summary> Code: Q </summary>
    Qals,

    /// <summary> Code: SLP </summary>
    Sleep,

    /// <summary> Code: REV </summary>
    Revive,

    /// <summary> Code: SPY </summary>
    Spy,

    /// <summary> Code: AVO </summary>
    AntiVote,

    /// <summary> Code: SS </summary>
    SaveSelf,

    /// <summary> Code: IM </summary>
    Immune,
}

public static class ActionStep
{
    /// <summary> Change Step to <see cref="StepCode.WaitingTime_Wait"/> </summary>
    public const int WaitingTime_0 = 0;

    /// <summary> Change Step to <see cref="StepCode.DayVotingTime_DVT"/> </summary>
    public const int DayVotingTime_1 = 1;

    /// <summary> Change Step to <see cref="StepCode.DayVotingResult_DVR"/> </summary>
    public const int DayVotingResult_2 = 2;

    /// <summary> Change Step to <see cref="StepCode.DayToNightChange_DTN"/> </summary>
    public const int DayToNightChange_3 = 3;

    /// <summary> Change Step to <see cref="StepCode.NightActionsTime_NAT"/> </summary>
    public const int NightActionsTime_4 = 4;

    /// <summary> Change Step to <see cref="StepCode.NightActionsResult_NAR"/> </summary>
    public const int NightActionsResult_5 = 5;

    /// <summary> Change Step to <see cref="StepCode.NightToDayChange_NTD"/> </summary>
    public const int NightToDayChange_6 = 6;
}

public static class StepCode
{
    /// <summary> next step is <see cref="NightToDayChange_NTD"/> </summary>
    public const string WaitingTime_Wait = "Wait";

    /// <summary> next step is <see cref="DayVotingResult_DVR"/> </summary>
    public const string DayVotingTime_DVT = "DVT";

    /// <summary> next step is <see cref="DayToNightChange_DTN"/> </summary>
    public const string DayVotingResult_DVR = "DVR";

    /// <summary> next step is <see cref="NightActionsTime_NAT"/> </summary>
    public const string DayToNightChange_DTN = "DTN";

    /// <summary> next step is <see cref="NightActionsResult_NAR"/> </summary>
    public const string NightActionsTime_NAT = "NAT";

    /// <summary> next step is <see cref="NightToDayChange_NTD"/> </summary>
    public const string NightActionsResult_NAR = "NAR";

    /// <summary> next step is <see cref="DayVotingTime_DVT"/> </summary>
    public const string NightToDayChange_NTD = "NTD";

    public const string GameResult = "GameResult";
}

public static class DayResult
{
    public const string Result_Voted = "Vot";
    public const string Result_VotedDraw = "DVot";
    public const string Result_Draw = "Drw";
    public const string Result_NoVotes = "NoV";
    public const string Result_AntiVote = "AntV";
}