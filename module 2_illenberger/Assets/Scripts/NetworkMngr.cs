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
    public Text roomInfoTxt;
    public GameObject playerListItemPrefab;
    public GameObject playerListViewParent;
    public GameObject startGameBtn; //should only be enabled if ur host

    [Header ("Room List Panel")]
    public GameObject roomListPnl;
    public GameObject roomItemPrefab;
    public GameObject roomListParent; //make the list items a child of the list parent

    private Dictionary<string, RoomInfo> cachedRoomList; //when
    private Dictionary<string, GameObject> roomListGameObjs;
    private Dictionary<int, GameObject> playerListGameObjs; 


    //#region Unity functions
    // Start is called before the first frame update
    void Start()
    {
      cachedRoomList = new Dictionary<string, RoomInfo>();
      roomListGameObjs = new Dictionary<string, GameObject>();
      ActivatePanel(loginUIPnl);

      PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Update is called once per frame
    void Update()
    {
      connectionStatusText.text= "Connection status: " + PhotonNetwork.NetworkClientState;
    }
      //#endregion

    //#region UI Callbacks - for our onclick users
    public void OnLoginBtnClicked()
    {
      string playerName = playerNameInput.text; //get name

      if (string.IsNullOrEmpty(playerName)) //verify validity of name
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

    public void OnBackBtnClicked()
    {
       if(PhotonNetwork.InLobby){
         PhotonNetwork.LeaveLobby(); //need to clear prefabs when leaving provided by PUNcallbacks
       }
       ActivatePanel(gameOptPnl);
    }

    public void OnLeaveGameBtnClicked()
    {
      PhotonNetwork.LeaveRoom();
    }

    public void OnJoinRndRoomBtnClicked()
    {
      ActivatePanel(joinRndRoomPnl);
      PhotonNetwork.JoinRandomRoom();
    }

    public void OnStartGameButtonClicked()
    {
      PhotonNetwork.LoadLevel("GameScene");
    }
    //#endregion

    //#region PUN Callbacks
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

      roomInfoTxt.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + " Current Player Count: " +
        PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

      if(playerListGameObjs == null){
        playerListGameObjs = new Dictionary<int, GameObject>();
      }

      //join room then display player List
      foreach (Player player in PhotonNetwork.PlayerList){
        GameObject playerItem = Instantiate(playerListItemPrefab);
        playerItem.transform.SetParent(playerListViewParent.transform);
        playerItem.transform.localScale = Vector3.one;

        playerItem.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;
        //to determine that player is u (the you box being indicated)
        playerItem.transform.Find("PlayerIndicator").gameObject.SetActive(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);

        playerListGameObjs.Add(player.ActorNumber, playerItem); //dont forget to initialize playerListGameObjs otherwise will throw null ref exception
      }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
      ClearRoomListGameObjects();
      Debug.Log("OnRoomListUpdate called");

      startGameBtn.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient); //what happens if host leaves the room tho? photon also reassigns
      foreach(RoomInfo info in roomList){
        Debug.Log(info.Name);

        if(!info.IsOpen || !info.IsVisible || info.RemovedFromList){ //some rooms may be private, so we'll take a lil peek
          //when certain room gets full and gets updated, need to check is removed from the roomList

          //are there duplicates? then remove so we have most up to date roomlist
          if(cachedRoomList.ContainsKey(info.Name)){
            cachedRoomList.Remove(info.Name);
          }
        }
        else{
          //update existing rooms info
          if(cachedRoomList.ContainsKey(info.Name)){
            cachedRoomList[info.Name] = info;
          }
          else{ //just add to the list then
            cachedRoomList.Add(info.Name, info);
          }
        }
      }

      //after caching, instantiate the prefab of roomlist entry
      foreach (RoomInfo info in cachedRoomList.Values){
        //cachedroomlist will have updated values
        GameObject listItem = Instantiate(roomItemPrefab); //then make it a child
        listItem.transform.SetParent(roomListParent.transform);
        listItem.transform.localScale = Vector3.one; //avoid weird transforms

        listItem.transform.Find("RoomNameText").GetComponent<Text>().text = info.Name;
        listItem.transform.Find("RoomPlayersText").GetComponent<Text>().text = "Player count: " + info.PlayerCount
          + "/" + info.MaxPlayers;
        listItem.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(() => OnJoinRoomClicked(info.Name));

        //need to also update game objects in the listview and cache it
        roomListGameObjs.Add(info.Name, listItem);
      }
    }

    public override void OnLeftLobby()
    {
      ClearRoomListGameObjects();
      cachedRoomList.Clear();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    { //need to cache the gameobjects like OnJoinedRoom
      roomInfoTxt.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + " Current Player Count: " +
        PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

      GameObject playerItem = Instantiate(playerListItemPrefab);
      playerItem.transform.SetParent(playerListViewParent.transform);
      playerItem.transform.localScale = Vector3.one;

      playerItem.transform.Find("PlayerNameText").GetComponent<Text>().text = newPlayer.NickName;
      playerItem.transform.Find("PlayerIndicator").gameObject.SetActive(newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);

      playerListGameObjs.Add(newPlayer.ActorNumber, playerItem);
    }

    public override void OnPlayerLeftRoom(Player other)
    {
      startGameBtn.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);

      roomInfoTxt.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + " Current Player Count: " +
        PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
      Destroy(playerListGameObjs[other.ActorNumber]);
      playerListGameObjs.Remove(other.ActorNumber); //after leavign should head back to lobby, read function below
    }

    public override void OnLeftRoom()
    {
      //clear gameobjs inside list prefab
      foreach (var gameObject in playerListGameObjs.Values){
        Destroy(gameObject);
      }
      playerListGameObjs.Clear();
      playerListGameObjs = null;
      ActivatePanel(gameOptPnl);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
      Debug.LogWarning(message);

      string roomName = "Room: " + Random.Range(1000, 1000);
      RoomOptions roomOptions = new RoomOptions();
      roomOptions.MaxPlayers = 20;

      PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
    //#endregion

    //#region Private Methods
    private void OnJoinRoomClicked(string roomName)
    {
      if(PhotonNetwork.InLobby){
        PhotonNetwork.LeaveLobby();
      } //make sure joinroombutton is empty for it to work

      PhotonNetwork.JoinRoom(roomName); //when u join room u technically leave the lobby
    }

    private void ClearRoomListGameObjects() //call this method before updating in roomlist so it doesn't duplicate(?)
    {
      foreach (var item in roomListGameObjs.Values){
        Destroy(item);
      }

      roomListGameObjs.Clear();
    }

    //#endregion

    //#region Public Methods
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
    //#endregion
}
