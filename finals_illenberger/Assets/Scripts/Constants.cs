using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants : MonoBehaviour
{
  //constants class can be access anywhere within the project
    public const string PLAYER_READY = "isPlayerReady";
    public const string PLAYER_ROLE = "playerRole";
    public const string PLAYER_SELECTION_NUMBER = "playerSelectionNumber";
    public const byte EliminatedWhoEventCode = 1;
    public const byte WhoWonEventCode = 2;
    public const byte MushroomCollectedEventCode = 0;
}
