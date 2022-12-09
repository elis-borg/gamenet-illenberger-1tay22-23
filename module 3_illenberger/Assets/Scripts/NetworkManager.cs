using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    [Header("Login UI")]
    public GameObject LoginUIPanel;
    public InputField PlayerNameInput;

    [Header("Connecting Info Panel")]
    public GameObject ConnectingInfoUIPanel;

    [Header("Creating Room Info Panel")]
    public GameObject CreatingRoomInfoUIPanel;

    [Header("GameOptions  Panel")]
    public GameObject GameOptionsUIPanel;

    [Header("Create Room Panel")]
    public GameObject CreateRoomUIPanel;
    public InputField RoomNameInputField;
    public string GameMode;

    [Header("Inside Room Panel")]
    public GameObject InsideRoomUIPanel;
    public Text RoomInfoTxt, GameModeTxt;
    public GameObject PlayerListPrefab, PlayerListParent,
          StartGameBtn;

    [Header("Join Random Room Panel")]
    public GameObject JoinRandomRoomUIPanel;

    private Dictionary<int, GameObject> playerListGameObjs;

    //#region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        ActivatePanel(LoginUIPanel.name);
        PhotonNetwork.AutomaticallySyncScene = true; //all the other players in that room will also load that specific scene
    }

    // Update is called once per frame
    void Update()
    {

    }
    //#endregion

    //#region UI Callback Methods
    public void OnLoginButtonClicked()
    {
        string playerName = PlayerNameInput.text;

        if (!string.IsNullOrEmpty(playerName))
        {
            ActivatePanel(ConnectingInfoUIPanel.name);

            if (!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.LocalPlayer.NickName = playerName;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        else
        {
            Debug.Log("PlayerName is invalid!");
        }
    }

    public void OnCancelButtonClicked()
    {
        ActivatePanel(GameOptionsUIPanel.name);
    }

    public void OnCreateRoomButtonClicked()
    {
        ActivatePanel(CreatingRoomInfoUIPanel.name);
        if(GameMode != null){ //null guard

          string roomName = RoomNameInputField.text;

          if (string.IsNullOrEmpty(roomName))
          {
              roomName = "Room " + Random.Range(1000, 10000);
          }

          RoomOptions roomOptions = new RoomOptions(); //photon.realtime
          string[] roomPropertiesInLobby = {"gm"}; //gm = game mode
          roomOptions.MaxPlayers = 3;
          /*custom property for room, tho gm doesn't contain anything for now will need a hash table
          similar to dictionary and contains a key (gm), value would be whatever gamemode selected*/

          //gm is set to rc by default, rc = racing, dr = death race, previous parameter {gm, rc}
          ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable() {{"gm", GameMode}};

          roomOptions.CustomRoomPropertiesForLobby = roomPropertiesInLobby;
          roomOptions.CustomRoomProperties = customRoomProperties;
          PhotonNetwork.CreateRoom(roomName, roomOptions); //new (2nd) parameter for room options, usually just roomName
        }
    }

    public void OnJoinRandomRoomClicked(string gameMode)
    {
      GameMode = gameMode;

      //when create room expecting customproperties embedded in it
      ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() {{"gm", gameMode}};
      //joining random room based on mode selected,
      PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 0); //will have an error since u need another overload method, just set to 0 if we dont to put max. players
      //if no room yet just override with OnJoinRandomFailed
    }

    public void OnBackButtonClicked()
    {
      ActivatePanel(GameOptionsUIPanel.name);
    }

    public void OnLeaveGameButtonClicked()
    {
      PhotonNetwork.LeaveRoom();
    }

    public void OnStartGameButtonClicked()
    {
      if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("gm")){
        if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("rc")){ //initialize racing gm
          PhotonNetwork.LoadLevel("RacingScene");
        }
        else if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("dr")){ //initialize deathracing gm
          PhotonNetwork.LoadLevel("DeathRaceScene");
        }
      }
    }
    //#endregion

    //#region Photon Callbacks
    public override void OnConnected()
    {
        Debug.Log("Connected to Internet");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName+ " is connected to Photon");
        ActivatePanel(GameOptionsUIPanel.name);
    }

    public override void OnCreatedRoom()
    {
      Debug.Log(PhotonNetwork.CurrentRoom + "has been created");
    }

    public override void OnJoinedRoom()
    {
      Debug.Log(PhotonNetwork.LocalPlayer.NickName + "has joined the " + PhotonNetwork.CurrentRoom.Name);
      Debug.Log("Playercount: " + PhotonNetwork.CurrentRoom.PlayerCount);

      ActivatePanel(InsideRoomUIPanel.name);
      //which gm has been set
      //2nd paramenter of TryGetValue is an object type so need to make its type
      object gameModeName;
      if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gm", out gameModeName)){
        Debug.Log(gameModeName.ToString());
        RoomInfoTxt.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " + PhotonNetwork.CurrentRoom.PlayerCount + "/"
          + PhotonNetwork.CurrentRoom.MaxPlayers;

        if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("rc")){
          GameModeTxt.text = "Racing Mode";
        }
        else if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("dr")){
          GameModeTxt.text = "Death Race Mode";
        }
      }

      if(playerListGameObjs == null){
        playerListGameObjs = new Dictionary<int, GameObject>();
      }

      foreach(Player player in PhotonNetwork.PlayerList){
        GameObject playerListItem = Instantiate(PlayerListPrefab);
        playerListItem.transform.SetParent(PlayerListParent.transform);
        playerListItem.transform.localScale = Vector3.one;

        playerListItem.GetComponent<PlayerListItemInitializer>().Initialize(player.ActorNumber, player.NickName);

        object isPlayerReady;
        if(player.CustomProperties.TryGetValue(Constants.PLAYER_READY, out isPlayerReady)){
          playerListItem.GetComponent<PlayerListItemInitializer>().SetPlayerReady((bool) isPlayerReady);
        }
        playerListGameObjs.Add(player.ActorNumber, playerListItem);
      }

      StartGameBtn.SetActive(false); //initially off anw before checking if anyone is ready

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
      GameObject playerListItem = Instantiate(PlayerListPrefab);
      playerListItem.transform.SetParent(PlayerListParent.transform);
      playerListItem.transform.localScale = Vector3.one;

      playerListItem.GetComponent<PlayerListItemInitializer>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);

      playerListGameObjs.Add(newPlayer.ActorNumber, playerListItem);

      RoomInfoTxt.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " + PhotonNetwork.CurrentRoom.PlayerCount + "/"
        + PhotonNetwork.CurrentRoom.MaxPlayers; //updates playercount for both clients

      StartGameBtn.SetActive(CheckAllPlayersReady()); //takes players readiness as boolean, activating it, if not viceversa
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
      Destroy(playerListGameObjs[otherPlayer.ActorNumber].gameObject);
      playerListGameObjs.Remove(otherPlayer.ActorNumber);

      RoomInfoTxt.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " + PhotonNetwork.CurrentRoom.PlayerCount + "/"
        + PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    public override void OnLeftRoom()
    {
      ActivatePanel(GameOptionsUIPanel.name);

      foreach(GameObject playerListGameObj in playerListGameObjs.Values){
        Destroy(playerListGameObj);
      }

      playerListGameObjs.Clear();
      playerListGameObjs = null;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
      Debug.Log(message);

      if(GameMode != null){ //null guard

        string roomName = RoomNameInputField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(1000, 10000);
        }

        RoomOptions roomOptions = new RoomOptions(); //photon.realtime
        roomOptions.MaxPlayers = 3;
        string[] roomPropertiesInLobby = {"gm"}; //gm = game mode
        /*custom property for room, tho gm doesn't contain anything for now will need a hash table
        similar to dictionary and contains a key (gm), value would be whatever gamemode selected*/

        //gm is set to rc by default, rc = racing, dr = death race, previous parameter {gm, rc}
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable() {{"gm", GameMode}};

        roomOptions.CustomRoomPropertiesForLobby = roomPropertiesInLobby;
        roomOptions.CustomRoomProperties = customRoomProperties;
        PhotonNetwork.CreateRoom(roomName, roomOptions); //new (2nd) parameter for room options, usually just roomName
      }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    { //called whenever property is changed
      GameObject playerListGameObj;
      if(playerListGameObjs.TryGetValue(targetPlayer.ActorNumber, out playerListGameObj)){
        //this will update the ready button for the other player
        object isPlayerReady;
        if(changedProps.TryGetValue(Constants.PLAYER_READY, out isPlayerReady)){
          playerListGameObj.GetComponent<PlayerListItemInitializer>().SetPlayerReady((bool) isPlayerReady);
        }
      }
      StartGameBtn.SetActive(CheckAllPlayersReady()); //also pasted here just so incase any player changes their mind, its also updated
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
      if(PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber){
        StartGameBtn.SetActive(CheckAllPlayersReady());
      }
    }
    //#endregion

    //#region Public Methods
    public void ActivatePanel(string panelNameToBeActivated)
    {
        LoginUIPanel.SetActive(LoginUIPanel.name.Equals(panelNameToBeActivated));
        ConnectingInfoUIPanel.SetActive(ConnectingInfoUIPanel.name.Equals(panelNameToBeActivated));
        CreatingRoomInfoUIPanel.SetActive(CreatingRoomInfoUIPanel.name.Equals(panelNameToBeActivated));
        CreateRoomUIPanel.SetActive(CreateRoomUIPanel.name.Equals(panelNameToBeActivated));
        GameOptionsUIPanel.SetActive(GameOptionsUIPanel.name.Equals(panelNameToBeActivated));
        JoinRandomRoomUIPanel.SetActive(JoinRandomRoomUIPanel.name.Equals(panelNameToBeActivated));
        InsideRoomUIPanel.SetActive(InsideRoomUIPanel.name.Equals(panelNameToBeActivated));
    }

    public void SetGameMode(string gameMode)
    {
      GameMode = gameMode;
    }
    //#endregion

    //#region Private Methods
    private bool CheckAllPlayersReady()
    {
      if(!PhotonNetwork.IsMasterClient) return false; //only the owner of the room can start the game

      foreach(Player p in PhotonNetwork.PlayerList){
        object isPlayerReady;

        if(p.CustomProperties.TryGetValue(Constants.PLAYER_READY, out isPlayerReady)){
          if(!(bool) isPlayerReady) return false;
        }
        else return false;
      }
    return true; //if any of the ifs arent met
    }
    //#endregion
}
