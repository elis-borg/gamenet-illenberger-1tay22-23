using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime; //handles all the rooms and lobbies for

/*need way to determine if we have connected to the server, needs a callback
change MonoBehaviour to MonoBehaviourPunCallbacks; contains several override methods to determine status of connection*/
public class LaunchManager : MonoBehaviourPunCallbacks
{
    public GameObject EnterGamePanel;
    public GameObject ConnectionStatusPanel;
    public GameObject LobbyPanel;

    // Start is called before the first frame update
    void Start()
    {
      //only enter game panel should be active, enter name panel shouldnt
      EnterGamePanel.SetActive(true);
      ConnectionStatusPanel.SetActive(false); //only active when connected
      LobbyPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnConnectedToMaster()
    {
      Debug.Log(PhotonNetwork.NickName + " connected to photon servers"); //client needs to know who connected
      ConnectionStatusPanel.SetActive(false);
      LobbyPanel.SetActive(true);
    }

    //check if connected to internet
    public override void OnConnected()
    {
      Debug.Log("connected to internet");
      //safe to assume this will be called first than OnConnectedToMaster
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
      //base.OnJoinRandomFailed(returnCode, message);
      //no match found = no rooms available
      Debug.LogWarning(message);
      CreateAndJoinRoom();
    }

    //know who entered (after creating playernameinputmngr)
    //connect to photonserver using nickname
    public void ConnectToPhotonServer()
    {
      //before connecting, need to kno if not connected first
      if (!PhotonNetwork.IsConnected)
      { //if we arent connected then its the only time it connects
        //connect to servers
        PhotonNetwork.ConnectUsingSettings();
        ConnectionStatusPanel.SetActive(true);
        EnterGamePanel.SetActive(false);
      }
    }

    //need to check if there is an existing room
    public void JoinRandomRoom()
    {
      PhotonNetwork.JoinRandomRoom();
      //if try to join room but no room, then create one
      //what if fail? (see OnJoinRandomFailed())
    }

    private void CreateAndJoinRoom()
    {
      string randomRoomName = "Room" + Random.Range(0,10000);

      RoomOptions roomOptions = new RoomOptions();
      roomOptions.IsOpen = true;
      roomOptions.IsVisible = true;
      roomOptions.MaxPlayers = 20; //will be tied to CCU in photon

      PhotonNetwork.CreateRoom(randomRoomName, roomOptions);
    }

    //check if joined that particular room
    public override void OnJoinedRoom()
    {
      Debug.Log(PhotonNetwork.NickName + " has entered " + PhotonNetwork.CurrentRoom.Name);
    }
    //can test if another player has entered, test with 2 builds

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
      Debug.Log(newPlayer.NickName + " has entered room " + PhotonNetwork.CurrentRoom.Name
      + ".\n" + "Room has now " + PhotonNetwork.CurrentRoom.PlayerCount + " players.");

    }
}
