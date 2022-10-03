using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;

public class NetworkMngr : MonoBehaviourPunCallbacks
{
    [Header ("Connection Status Panel")]
    public Text connectionStatusText;

    [Header ("Login UI Panel")]
    public InputField playerNameInput;
    public GameObject loginUIPnl;

    [Header ("Game Options Panel")]
    public GameObject gameOptPnl;

    [Header ("Create Room Panel")]
    public GameObject createRoomPnl;
    public InputField roomNameInputFld;
    public InputField playerCntInputFld;

    [Header ("Join Random Room Panel")]
    public GameObject joinRndRoomPnl;

    [Header ("Show Room List Panel")]
    public GameObject showRoomListPnl;

    [Header ("Inside Room List Panel")]
    public GameObject insideRoomPnl;

    [Header ("Room List Panel")]
    public GameObject roomListPnl;

    //region Unity Functions
    // Start is called before the first frame update
    void Start()
    {
      ActivatePanel(loginUIPnl);
    }

    // Update is called once per frame
    void Update()
    {
      connectionStatusText.text= "Connection status: " + PhotonNetwork.NetworkClientState;
    }
    //endregion Unity Functions

    //region UI Callbacks //for our onclick users
    public void OnLoginButtonClicked()
    {
      string playerName = playerNameInput.text;

      if (string.IsNullOrEmpty(playerName))
      {
        Debug.Log("Player name is invalid!");
      }
      else
      {
        PhotonNetwork.LocalPlayer.NickName = playerName;
        PhotonNetwork.ConnectUsingSettings();
      }
    }

    public void OnCreateRoomBtnClicked()
    {
      string roomName = roomNameInputFld.text;

      if (string.IsNullOrEmpty(roomName))
      {
        roomName = "Room " + Random.Range(1000, 10000);
      }

      RoomOptions roomOptions = new RoomOptions();
      roomOptions.MaxPlayers = (byte)int.Parse(playerCntInputFld.text); //will throw an error bcos of int to byte, need to typecast

      PhotonNetwork.CreateRoom(roomName, roomOptions);
      /*notice we're only creating rooms and not lobbies but in Photon, technically all rooms belong to lobbies, we didnt create one since = using default lobby for now
      default lobby type = most suited for synchronous random matchmaking*/
    }

    public void OnCancelBtnClicked()
    {
      ActivatePanel(gameOptPnl);
    }

    public void OnShowRoomListBtnClicked()
    {
      //since we're going to retrieve the info of all rooms, need to make sure ur actually inside lobby
      if(!PhotonNetwork.InLobby){
        PhotonNetwork.JoinLobby();
        Debug.Log("Joined a lobby.");
      }

      ActivatePanel(showRoomListPnl);
    }
    //endregion

    //region PUN Callbacks
    public override void OnConnected()
    {
      Debug.Log("Connected to the Internet");
    }

    public override void OnConnectedToMaster()
    {
      Debug.Log(PhotonNetwork.LocalPlayer.NickName + " has connected to Photon servers ");
      ActivatePanel(gameOptPnl);
    }

    public override void OnCreatedRoom()
    {
      Debug.Log(PhotonNetwork.CurrentRoom.Name + " created!");
    }

    public override void OnJoinedRoom()
    {
      Debug.Log(PhotonNetwork.LocalPlayer.NickName + " has joined " + PhotonNetwork.CurrentRoom.Name);
      ActivatePanel(insideRoomPnl);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
      foreach(RoomInfo info in roomList){
        Debug.Log(info.Name);
      }
    }
    //endregion

    //region Public Methods
    public void ActivatePanel(GameObject panelToBeAbled) //activate the panel u want to appear and disable to hide
    {
      loginUIPnl.SetActive(panelToBeAbled.Equals(loginUIPnl));
      gameOptPnl.SetActive(panelToBeAbled.Equals(gameOptPnl));
      createRoomPnl.SetActive(panelToBeAbled.Equals(createRoomPnl));
      joinRndRoomPnl.SetActive(panelToBeAbled.Equals(joinRndRoomPnl));
      showRoomListPnl.SetActive(panelToBeAbled.Equals(showRoomListPnl));
      insideRoomPnl.SetActive(panelToBeAbled.Equals(insideRoomPnl));
      roomListPnl.SetActive(panelToBeAbled.Equals(roomListPnl));
    }
    //endregion
}
