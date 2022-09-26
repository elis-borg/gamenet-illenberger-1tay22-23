using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerNameInputMngr : MonoBehaviour
{
    public void SetPlayerName(string playerName) //make sure its public else inspector wont access
    {
      if(string.IsNullOrEmpty(playerName))
      {
        Debug.LogWarning("Player name is empty!");
        return;
      }

      PhotonNetwork.NickName = playerName; //set the player's name in ur network
    }
}
