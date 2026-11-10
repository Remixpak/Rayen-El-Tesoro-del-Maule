using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public List<string> unlockedJournalIds = new List<string>();

    // Estado del jugador
    public bool statsInitialized; // false hasta que PlayerStats aplica los valores iniciales de una partida nueva
    public int health;
    public int ammo;
    public int gold;

   
}