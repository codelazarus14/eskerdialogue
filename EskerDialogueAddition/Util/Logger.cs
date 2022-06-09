using OWML.Common;

namespace EskerDialogueAddition.Util
{
    // "borrowed" from https://github.com/xen-42/outer-wilds-achievement-tracker/blob/main/AchievementTracker/Util/Logger.cs
    public static class Logger
    {
        private static string _prefix = $"[{nameof(EskerDialogueAddition)}] -- ";

        public static void Log(string msg)
        {
            if (EskerDialogueAddition.Instance == null) return;

            EskerDialogueAddition.Instance.ModHelper.Console.WriteLine($"{_prefix}{msg}", MessageType.Info);
        }

        public static void LogError(string msg)
        {
            if (EskerDialogueAddition.Instance == null) return;

            EskerDialogueAddition.Instance.ModHelper.Console.WriteLine($"{_prefix}{msg}", MessageType.Error);
        }

        public static void LogSuccess(string msg)
        {
            if (EskerDialogueAddition.Instance == null) return;

            EskerDialogueAddition.Instance.ModHelper.Console.WriteLine($"{_prefix}{msg}", MessageType.Success);
        }
    }

}
