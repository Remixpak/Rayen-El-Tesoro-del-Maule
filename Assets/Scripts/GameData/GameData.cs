using System;
using System.Collections.Generic;

//aunque estos datos sean estaticos y se guarden en la memoria, seria bueno que lo pusieramos en el gamemanager para la version final
//el objetivo de este script junto con el gamesession era mantener los datos para guardar la partida
//ya que estos dos scripts son solo datos que persisten y no estan presentes en la escena pero ahi te lo encargo la mision de unificar 
//basicamente este es el contenedor de datos que se guardan y cargan -3-
[Serializable]
public class GameData
{
    public List<string> unlockedJournalIds = new List<string>();

    // Estado del jugador
    public bool statsInitialized; // false hasta que PlayerStats aplica los valores iniciales de una partida nueva
    public int health;
    public int ammo;
    public int gold;

    //falta como dato poder guardar a futuro el estado de los cofres abierto o del estado del ciclo dia/noche 

   
}