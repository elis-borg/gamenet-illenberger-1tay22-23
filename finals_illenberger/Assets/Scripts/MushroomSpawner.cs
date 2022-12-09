using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MushroomSpawner : MonoBehaviour
{
    public static MushroomSpawner instance = null;

    [Header ("Mushroom Stuff")]
    public Transform[] mushroomSpawns;
    public GameObject mushroomPrefab,
                      mushroom;
    public int shroomsNeeded,
              collectedShrooms;
    public bool firstShroomSpawn = true,
                collected = false,
                relocating = false;

    [SerializeField]
    private int currentPoint;

    void Awake()
    {
      if(instance == null) instance = this;
      else if(instance != this) Destroy(gameObject);

      //DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
      currentPoint =  Random.Range(0, mushroomSpawns.Length);
      Debug.Log(mushroomSpawns[currentPoint].name);
    }

    // Update is called once per frame
    void Update()
    {
      if(firstShroomSpawn == true && GameManager.instance.cdTurnedOff == true){
        StartCoroutine(ShroomSpawnTimer());
        firstShroomSpawn = !firstShroomSpawn;
      }
      else if(mushroom != null && collected == false && relocating == false){
        relocating = !relocating;
        StartCoroutine(RelocateShroom());
      }
      else if(collected == true && mushroom == null){
        StartCoroutine(ShroomSpawnTimer());
        collected = !collected;
      }
    }

    public void SpawnMushroom()
    {
      if(firstShroomSpawn){
        Debug.Log("first mushroom spawned!");
        mushroom = PhotonNetwork.Instantiate(mushroomPrefab.name, mushroomSpawns[currentPoint].GetComponent<Transform>().position, Quaternion.identity);
      }
      else{
        int newPoint = currentPoint;
        while(newPoint == currentPoint){
          newPoint = Random.Range(0, mushroomSpawns.Length);
        }
        Debug.Log(mushroomSpawns[newPoint].name);

        mushroom = PhotonNetwork.Instantiate(mushroomPrefab.name, mushroomSpawns[newPoint].GetComponent<Transform>().position, Quaternion.identity);
        currentPoint = newPoint;
      }
      Debug.Log(mushroomSpawns[currentPoint].name);
    }

    IEnumerator ShroomSpawnTimer()
    {
      float timer = 10f; //20

      while (timer > 0){
        yield return new WaitForSeconds(1.0f);
        Debug.Log("spawning in " + timer);
        timer--;
      }
      SpawnMushroom();
    }

    IEnumerator RelocateShroom()
    {
      float timer = 5f; //15  //shifter has this much time to look for the shroom before it spawns elsewhere, avoids hunter camping if they spot it first

      while (timer > 0){
        yield return new WaitForSeconds(1.0f);
        Debug.Log("relocating in " + timer);
        timer--;
      }

      Destroy(mushroom);
      Debug.Log("shroom is relocated");
      SpawnMushroom();
      relocating = !relocating;
    }

    public void CancelRelocation()
    {
      //breaks out of coroutine
      StopCoroutine(RelocateShroom());
      Debug.Log("relocation cancelled");
    }

    public void ShroomCollected()
    {
      collected = true;
      collectedShrooms++;

      if(relocating == true) CancelRelocation();

      //try to call for rpc too

      //#region RaiseEvent
      string areaName = "";

      switch (currentPoint){
        case 0:
          areaName = "A shifter has been spotted around the perpetual autumn giants."; break;
        case 1:
          areaName = "A shifter has been spotted around the area of thriving decay."; break;
        case 2:
          areaName = "A shifter has been spotted around the fragrant fields."; break;
        case 3:
          areaName = "A shifter has been spotted around craggy towering cliffs."; break;
        case 4:
          areaName = "A shifter has been spotted around grassy plains."; break;
      }

      object[] data = new object[] {areaName}; //need to pass mushroom's last position in the map to alert hunters

      RaiseEventOptions raiseEventOpts = new RaiseEventOptions
      {
        Receivers = ReceiverGroup.All,
        CachingOption = EventCaching.AddToRoomCache
      };

      SendOptions sendOption = new SendOptions
      {
        Reliability = false
      };

      PhotonNetwork.RaiseEvent((byte) Constants.MushroomCollectedEventCode, data, raiseEventOpts, sendOption);
      //#endregion


      if(collectedShrooms >= shroomsNeeded) GameManager.instance.Gameover();
      Debug.Log("shrooms: " + collectedShrooms + "/" + shroomsNeeded);
    }
}
