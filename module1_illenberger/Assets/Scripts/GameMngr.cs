using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameMngr : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject playerPrefab;

    public static GameMngr instance;

    private void Awake()
    {
      if(instance != null){
        Destroy(this.gameObject);
      }
      else{
        instance = this;
      }
    }

    // Start is called before the first frame update
    void Start()
    {
      if(PhotonNetwork.IsConnected) //are we really connected?
      {
        if(playerPrefab != null)
        {
          //if not null, then spawn player in random origin
          int xRndPt = Random.Range(-20,20);
          int zRndPt = Random.Range(-20,20);

          //cant use instantiate() of unity bc it aint server lvl code, photon has own instantiate() :default:
          PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(xRndPt, 0, zRndPt), Quaternion.identity);
          //not using the GameObj but the name instead bc thats what PNs parameters are


        }
      }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnJoinedRoom()
    {
       //base.OnJoinedRoom();
       //log the player who joined the room
       Debug.Log(PhotonNetwork.NickName + " has joined the room!");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
       //base.OnPlayerEnteredRoom(newPlayer);
       Debug.Log(newPlayer.NickName + " has joined the room " + PhotonNetwork.CurrentRoom.Name);
       Debug.Log("Room has now " + PhotonNetwork.CurrentRoom.PlayerCount + "/20 players" );

    }

    public override void OnLeftRoom()
    {
      SceneManager.LoadScene("GameLauncherScene");
    }

    public void LeaveRoom()
    {
      //similar to joining rooms
      PhotonNetwork.LeaveRoom();
    }
}
