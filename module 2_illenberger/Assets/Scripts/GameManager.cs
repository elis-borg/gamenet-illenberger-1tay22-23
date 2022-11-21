using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject playerPrefab;

    public static GameManager instance;

    public GameObject[] spawnPoints;
    public GameObject[] players;

    public bool isGameover;

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
      spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
      isGameover = false;

      if(PhotonNetwork.IsConnectedAndReady){
        Vector3 spawn = PickRespawnPoint();
        PhotonNetwork.Instantiate(playerPrefab.name, spawn, Quaternion.identity);
      }

      PhotonNetwork.AutomaticallySyncScene = true; //hopefully fixes the player being left in the room thingy after game finishes
    }

    // Update is called once per frame
    void Update()
    {
      players = GameObject.FindGameObjectsWithTag("Player");

      if (isGameover){
        foreach(GameObject p in players) LeaveRoom();
      }
    }

    public override void OnLeftRoom()
    {
      SceneManager.LoadScene("LobbyScene");
    }

    public void LeaveRoom()
    {
      //this will be called by GameManager.instance
      PhotonNetwork.LeaveRoom();
    }

    public Vector3 PickRespawnPoint()
    {
      return spawnPoints[Random.Range(0, spawnPoints.Length)].GetComponent<Transform>().position;
    }

    public void WinnerDetermined()
    {
      Debug.Log("This function has been called.");
      foreach(GameObject p in players){
        LeaveRoom();
      }
    }

    public bool Gameover()
    {
      return isGameover = true;
    }
}
