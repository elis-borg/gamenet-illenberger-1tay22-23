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

    public float timer = 120.0f; //for quickplay

    [Header ("UI")]
    public GameObject[] eliminateeTxtUI;
    public GameObject winnerTxtUI,
                      alertTxtUI;
    public Text timeTxt;
    public Image blindsImgUI;

    [Header ("Player Stuff")]
    public int playersDone;
    public int shifterMax = 3, //3 (5), 2 (4), 1 (3), 1(2)
               hunterMax = 2; //2 (5), 2(4), 2(3), 1(2)

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
            //Debug.Log(PhotonNetwork.LocalPlayer.NickName + " has chosen to be a " + rolePrefabs[(int)playerSelectionNumber].name);

            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            //Debug.Log(PhotonNetwork.LocalPlayer.NickName + "is actor#" + PhotonNetwork.LocalPlayer.ActorNumber);
            Vector3 instantiatePosition = startingPositions[actorNumber-1].position;
            PhotonNetwork.Instantiate(rolePrefabs[(int)playerSelectionNumber].name, instantiatePosition, Quaternion.identity);
          }
        }

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

    //#region PhotonNetwork Methods
    public override void OnLeftRoom()
    {
      SceneManager.LoadScene("LobbyScene");
    }

    public void LeaveRoom()
    {
      PhotonNetwork.LeaveRoom();
    }
    //#endregion

    public void Gameover()
    {
      isGameover = true;
    }

    public void BodyCount(GameObject deadPlayer, bool h)
    {
      //playerList.Remove(deadPlayer);
      if(h) deadHunters.Add(deadPlayer);
      else deadShifters.Add(deadPlayer);
    }

    public IEnumerator HunterAlert(string areaSpot)
    {
      float alertTime = 3.0f;

      while (alertTime > 0){
        yield return new WaitForSeconds(1.0f);
        alertTime--;

        alertTxtUI.GetComponent<Text>().text = areaSpot;
      }
      alertTxtUI.GetComponent<Text>().text = "";
    }
}
