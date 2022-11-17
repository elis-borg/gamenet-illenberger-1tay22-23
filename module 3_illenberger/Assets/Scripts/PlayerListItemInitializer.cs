using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerListItemInitializer : MonoBehaviour
{
    [Header("UI References")]
    public Text PlayerNameText;
    public Button PlayerReadyButton;
    public Image PlayerReadyImage;

    private bool isPlayerReady = false;

    public void Initialize(int playerId, string playerName)
    {
      PlayerNameText.text = playerName;

      if(PhotonNetwork.LocalPlayer.ActorNumber != playerId){
        PlayerReadyButton.gameObject.SetActive(false);
      }
      else{
        //sets custom property for each player "isPlayerReady"
        //first parameter was "isPlayerReady" but since its going to be called alot, a constants class was created for it instead
        ExitGames.Client.Photon.Hashtable initializeProperties = new ExitGames.Client.Photon.Hashtable() {{Constants.PLAYER_READY, isPlayerReady}};
        PhotonNetwork.LocalPlayer.SetCustomProperties(initializeProperties);

        PlayerReadyButton.onClick.AddListener(() =>
        //custom function below within ()
        {
          //when we click the button, the isPlayerReady shouldve the opposite value
          isPlayerReady = !isPlayerReady;
          SetPlayerReady(isPlayerReady);

          ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable() {{Constants.PLAYER_READY, isPlayerReady}};

          PhotonNetwork.LocalPlayer.SetCustomProperties(newProperties);
        });
      }
    }

    public void SetPlayerReady(bool playerReady)
    {
      PlayerReadyImage.enabled = playerReady;

      if(playerReady){
        PlayerReadyButton.GetComponentInChildren<Text>().text = "Ready!";
      }
      else{
        PlayerReadyButton.GetComponentInChildren<Text>().text = "Ready?";

      }
    }
}
