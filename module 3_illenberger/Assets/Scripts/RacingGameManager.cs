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

    public static RacingGameManager instance = null;

    public Text timeTxt;

    public List<GameObject> lapTriggers = new List<GameObject>();
    public List<GameObject> playerList = new List<GameObject>();

    public bool isGameover; //win conditions are either all players make it to the last lap or only one player left in game

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

        if(PhotonNetwork.IsConnectedAndReady){
          object playerSelectionNumber;

          //instantiate players vehicle prefab
          if(PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(Constants.PLAYER_SELECTION_NUMBER, out playerSelectionNumber)){
            Debug.Log(playerSelectionNumber);

            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            Vector3 instantiatePosition = startingPositions[actorNumber -1].position;
            //dont forget to set a starting position in gamescene becos parameters need a position ^
            PhotonNetwork.Instantiate(vehiclePrefabs[(int)playerSelectionNumber].name, instantiatePosition, Quaternion.identity);
          }
        }

        foreach (GameObject go in finisherTxtUI) go.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
      foreach(GameObject p in GameObject.FindGameObjectsWithTag("Player")) playerList.Add(p);

      if(isGameover){
        foreach(GameObject p in playerList) LeaveRoom();
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
}
