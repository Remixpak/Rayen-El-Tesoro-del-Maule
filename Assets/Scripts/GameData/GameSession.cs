using UnityEngine;

//script que se usa para cargar datos en una sesion

//para desbloquear entradas del diario se usa GameSession.UnlockJournal(entrada);

public static class GameSession
{
    public static GameData Data { get; private set; } = new GameData();

    // Por si el editor tiene desactivado el Domain Reload :p
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay() => Data = new GameData();

    public static void NewGame() => Data = new GameData();
    public static void SetData(GameData data) => Data = data ?? new GameData(); // para cargar una partida

    public static bool IsJournalUnlocked(JournalEntry entry)
    {
        if (entry == null) return false;
        return entry.UnlockedFromStart || Data.unlockedJournalIds.Contains(entry.Id);
    }

    public static void UnlockJournal(JournalEntry entry)
    {
        if (entry == null || IsJournalUnlocked(entry)) return;
        Data.unlockedJournalIds.Add(entry.Id);
    }
}