using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance = null;

    public GameObject[] rolePrefabs,
                        shifterAnimalPrefabs;
    public Transform[] startingPositions;

    [Header ("UI")]
    public GameObject[] finisherTxtUI; //to remove
    public GameObject[] eliminateeTxtUI;
    public GameObject winnerTxtUI,
                      alertTxtUI;
    public Text timeTxt;
    public Image blindsImgUI;

    [Header ("Player Stuff")]
    public int playersDone;
    public int shifterMax = 3,
               hunterMax = 2;

    public List<GameObject> playerList = new List<GameObject>(); //add tags
    public List<GameObject> deadHunters = new List<GameObject>();
    public List<GameObject> deadShifters = new List<GameObject>();


    public bool isGameover, //win conditions are either all players make it to the last lap or only one player left in game
                cdTurnedOff = false;

    void Awake()
    {
      if(instance == null) instance = this;
      else if(instance != this) Destroy(gameObject);

      DontDestroyOnLoad(gameObject);
      PhotonNetwork.AutomaticallySyncScene = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        isGameover = false;
        cdTurnedOff = false;

        if(PhotonNetwork.IsConnectedAndReady){
          object playerSelectionNumber;

          //instantiate players role
            if(PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(Constants.PLAYER_SELECTION_NUMBER, out playerSelectionNumber)){
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " has chosen to be a " + rolePrefabs[(int)playerSelectionNumber].name);

            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            //Debug.Log(PhotonNetwork.LocalPlayer.NickName + "is actor#" + PhotonNetwork.LocalPlayer.ActorNumber);
            Vector3 instantiatePosition = startingPositions[actorNumber-1].position;
            PhotonNetwork.Instantiate(rolePrefabs[(int)playerSelectionNumber].name, instantiatePosition, Quaternion.identity);
          }
        }

        foreach (GameObject go in finisherTxtUI) go.SetActive(false);
        foreach (GameObject go in eliminateeTxtUI) go.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
      //Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount);
      //if(playersDone == PhotonNetwork.CurrentRoom.PlayerCount || deadHunters.Count == PhotonNetwork.CurrentRoom.PlayerCount-1) isGameover = true;

      if(isGameover){
        foreach(GameObject p in playerList) LeaveRoom();
        foreach(GameObject p in deadHunters) LeaveRoom();
        foreach(GameObject p in deadShifters) LeaveRoom();

      }
    }

    public override void OnLeftRoom()
    {
      SceneManager.LoadScene("LobbyScene");
    }

    public void LeaveRoom()
    {
      PhotonNetwork.LeaveRoom();
    }

    public void Gameover()
    {
      isGameover = true;
    }

    public void RemovePlayer(GameObject deadPlayer)
    {
      playerList.Remove(deadPlayer);
      deadHunters.Add(deadPlayer);
    }

    IEnumerator HunterAlert()
    {
      float alertTime = 3.0f;

      while (alertTime > 0){
        yield return new WaitForSeconds(1.0f);
        alertTime--;

        //alertTxtUI.GetComponent<Text>().text = "A shifter has collected a mushroom! " + collectedShrooms + "/" + shroomsNeeded + " left.";
      }
    }

    void OnEvent(EventData photonEvent)
    {
      if(photonEvent.Code == (byte)Constants.MushroomCollectedEventCode){ //maybe try implementing as an RPC instead?
        //retrieve data that has been passed
        object[] data = (object[]) photonEvent.CustomData;

        string nickNameOfFinishedPlayer = (string)data[0];
        //finishOrder = (int)data[1];
        int viewId = (int)data[2];

        //Debug.Log("A shifter has collected a mushroom! " + collectedShrooms + "/" + shroomsNeeded + " left.");

        //GameObject orderUiTxt = GameManager.instance.finisherTxtUI[finishOrder-1];
        //orderUiTxt.SetActive(true);

        if(viewId == photonView.ViewID) {//this is you
          //orderUiTxt.GetComponent<Text>().text = finishOrder + " " + nickNameOfFinishedPlayer + "(YOU)";
        //  orderUiTxt.GetComponent<Text>().color = Color.red;
        }
        else{
          //orderUiTxt.GetComponent<Text>().text = finishOrder + " " + nickNameOfFinishedPlayer;
        }
      }
    }
}
