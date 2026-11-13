using UnityEngine;

//script que se usa para cargar datos en una sesion

//para desbloquear entradas del diario se usa GameSession.UnlockJournal(entrada);
// este script baiscamente es la capa de persistencia donde vive el gamedata y se realizan operaciones que tienen que ver con el guardado y la carga de datos, como desbloquear entradas del diario, o crear una nueva partida
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