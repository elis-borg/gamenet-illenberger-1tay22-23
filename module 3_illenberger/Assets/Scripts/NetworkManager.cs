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

    [Header("Join Random Room Panel")]
    public GameObject JoinRandomRoomUIPanel;

    //#region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        ActivatePanel(LoginUIPanel.name);
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
      }
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
}
