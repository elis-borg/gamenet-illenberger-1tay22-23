using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RacingGameManager : MonoBehaviourPunCallbacks
{
    public GameObject[] vehiclePrefabs;
    public Transform[] startingPositions;
    public GameObject[] finisherTxtUI;
    public GameObject[] eliminateeTxtUI;
    public GameObject winnerTxtUI;

    public static RacingGameManager instance = null;

    public Text timeTxt;

    public int playersDone;

    public List<GameObject> lapTriggers = new List<GameObject>();
    public List<GameObject> playerList = new List<GameObject>();
    public List<GameObject> deadPlayerList = new List<GameObject>();


    public bool isGameover, //win conditions are either all players make it to the last lap or only one player left in game
                cdTurnedOff;

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

          //instantiate players vehicle prefab
          if(PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(Constants.PLAYER_SELECTION_NUMBER, out playerSelectionNumber)){
            Debug.Log(playerSelectionNumber);

            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            Vector3 instantiatePosition = startingPositions[actorNumber-1].position;
            //dont forget to set a starting position in gamescene becos parameters need a position ^
            PhotonNetwork.Instantiate(vehiclePrefabs[(int)playerSelectionNumber].name, instantiatePosition, Quaternion.identity);
          }
        }

        foreach (GameObject go in finisherTxtUI) go.SetActive(false);
        foreach (GameObject go in eliminateeTxtUI) go.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
      //Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount);
      //if(playersDone == PhotonNetwork.CurrentRoom.PlayerCount || deadPlayerList.Count == PhotonNetwork.CurrentRoom.PlayerCount-1) isGameover = true;

      if(isGameover){
        foreach(GameObject p in playerList) LeaveRoom();
        foreach(GameObject p in deadPlayerList) LeaveRoom();
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
      deadPlayerList.Add(deadPlayer);
    }
}
