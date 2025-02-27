﻿namespace GTAIVDowngrader
{

    public enum Steps
    {
        S0_Welcome = 0,
        S1_SelectIVExe,
        S2_MD5FilesChecker,
        S3_MoveGameFilesQuestion,
        S4_MoveGameFiles,
        S5_SelectDwngrdVersion,
        S6_Multiplayer,
        S7_SelectRadioDwngrd,
        S7_1_SelectVladivostokType,
        S8_SelectComponents,
        S9_Confirm,
        S10_Downgrade,
        S11_SavefileDowngrade,
        S11_SavefileDowngrade_2,
        S11_SavefileDowngrade_3,
        S12_Commandline,
        S13_Finish,

        MessageDialog,
        StandaloneWarning,
        Error
    }

    public enum LogType
    {
        Info,
        Warning,
        Error
    }
    public enum NotificationType
    {
        Info,
        Warning,
        Error,
        Success
    }
    public enum SlideDirection
    {
        TopToBottom,
        BottomToTop
    }

}
