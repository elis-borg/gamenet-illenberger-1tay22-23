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
    public GameObject[] eliminateeTxtUI;
    public GameObject winnerTxtUI;

    public Text timeTxt,
                countdownTxt,
                alertTxt;
    public Image blindsImgUI;

    [Header ("Player Stuff")]
    public int playersDone;
    public int shifterMax = 3, //3 (5), 2 (4), 1 (3), 1(2)
               hunterMax = 2; //2 (5), 2(4), 2(3), 1(2)

    public List<GameObject> playerList = new List<GameObject>(); //add tags
    public List<GameObject> deadHunters = new List<GameObject>();
    public List<GameObject> deadShifters = new List<GameObject>();

    public bool isGameover,
                cdTurnedOff = false,
                isThereWinner; //off for freeplay

    [Header ("Quickplay Mode")]
    public bool timedGM; //on for quickplay off for freeplay
    public bool timerIsRunning = false;
    public float gameDuration;
    private float timeRemaining;


    void Awake()
    {
      if(instance == null) instance = this;
      else if(instance != this) Destroy(gameObject);

      //DontDestroyOnLoad(gameObject);
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
            //Debug.Log(PhotonNetwork.LocalPlayer.NickName + "instantiated at pos#" + instantiatePosition);
          }
        }

        if(timedGM){
              timeRemaining = gameDuration;
        }

        foreach (GameObject go in eliminateeTxtUI) go.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
      //Debug.Log(PhotonNetwork.CurrentRoom.PlayerCount);
      //if(playersDone == PhotonNetwork.CurrentRoom.PlayerCount || deadHunters.Count == PhotonNetwork.CurrentRoom.PlayerCount-1) isGameover = true;

      if(timedGM){
        if(timerIsRunning){
          if(timeRemaining > 0){
            timeRemaining -= Time.deltaTime;
            float minutes = Mathf.FloorToInt(timeRemaining / 60);
            float seconds = Mathf.FloorToInt(timeRemaining % 60);

            countdownTxt.text = "0" + minutes + ":" + seconds + " left";
          }
          else{
            timeRemaining = 0;
            timerIsRunning = false;
            countdownTxt.text = "00:00 Time's up!";

            StartCoroutine(Wait(5.0f));
            Gameover(false);
          }
        }
      }

      if(isGameover){
        foreach(GameObject p in playerList){
          Destroy(p);
          LeaveRoom();
        }
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

    public void Gameover(bool answer)
    {
      isThereWinner = answer;
      if(timerIsRunning) timerIsRunning = !timerIsRunning;
      isGameover = true;
    }

    public void BodyCount(GameObject deadPlayer, bool h)
    {
      //playerList.Remove(deadPlayer);
      if(h) deadHunters.Add(deadPlayer);
      else deadShifters.Add(deadPlayer);
    }

    IEnumerator Wait(float time)
    {
      winnerTxtUI.transform.GetChild(0).GetComponent<Text>().text = "Neither hunter or fae returned home.";
      blindsImgUI.enabled = true;
      yield return new WaitForSeconds(time);
    }

}
