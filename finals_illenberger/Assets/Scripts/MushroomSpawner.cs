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
      if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("qp")){
        shroomsNeeded = 3;
      }
    }

    // Update is called once per frame
    void Update()
    {
      if(PhotonNetwork.IsMasterClient){ //so that only one is calling mushrooms
        if(mushroom == null && GameManager.instance.cdTurnedOff == true){
          //if mushroom is empty and its the firsttime spawn and countdown managers are off for all players, then spawn the mushroom and turn off firstspawn bool
          if(firstShroomSpawn){
            StartCoroutine(ShroomSpawnTimer());
            firstShroomSpawn = !firstShroomSpawn;
          }
        }
        //else if mushroom isnt empty, hasnt been collected yet and relocation is off then turn on relocation bool and relocate it
        else if(mushroom != null && collected == false && relocating == false){
          relocating = !relocating; //turning this on allowes for only 1 call of relocation
          StartCoroutine(RelocateShroom());
          Debug.Log("uncollected mushroom about to be relocated");
        }
      }
    }

    public void SpawnMushroom()
    {
      if(firstShroomSpawn){
        //the first value of currentpoint has been called at start
        mushroom = PhotonNetwork.Instantiate(mushroomPrefab.name, mushroomSpawns[currentPoint].GetComponent<Transform>().position, Quaternion.identity);
        Debug.Log("first mushroom spawned!");
      }
      else{
        int newPoint = currentPoint;
        while(newPoint == currentPoint){
          newPoint = Random.Range(0, mushroomSpawns.Length);
        }

        mushroom = PhotonNetwork.Instantiate(mushroomPrefab.name, mushroomSpawns[newPoint].GetComponent<Transform>().position, Quaternion.identity);
        currentPoint = newPoint;
      }
      Debug.Log(mushroomSpawns[currentPoint].name);
    }

    IEnumerator ShroomSpawnTimer()
    {
      float timer;
      if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("qp")) {
        timer = 10f;
      }
      else {
        timer = 20f; //20 normal
      }

      while (timer > 0){
        yield return new WaitForSeconds(1.0f);
        //Debug.Log("spawning in " + timer);
        timer--;
      }
      SpawnMushroom();
    }

    IEnumerator RelocateShroom()
    {
      float timer = 30f; //normal  //shifter has this much time to look for the shroom before it spawns elsewhere, avoids hunter camping if they spot it first

      while (timer > 0){
        yield return new WaitForSeconds(1.0f);
        //Debug.Log("relocating in " + timer);
        timer--;
      }

      //Destroy(mushroom);
      int newPoint = currentPoint;
      while(newPoint == currentPoint) newPoint = Random.Range(0, mushroomSpawns.Length);

      mushroom.transform.position = mushroomSpawns[newPoint].GetComponent<Transform>().position;
      Debug.Log("shroom is relocated to " + mushroomSpawns[newPoint].name);
      relocating = !relocating;
    }

    public void CancelRelocation()
    {
      //breaks out of coroutine
      StopCoroutine(RelocateShroom());
      Debug.Log("relocation cancelled");
    }

    [PunRPC]
    public void ShroomCollected(GameObject fae)
    {
      if(relocating == true) CancelRelocation();
      collected = true;
      collectedShrooms++;

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

      fae.GetComponent<PhotonView>().RPC("HunterAlert", RpcTarget.All, areaName);

      if(collectedShrooms == shroomsNeeded){
        fae.GetComponent<PhotonView>().RPC("ShroomsComplete", RpcTarget.All, "shifter");
      }
      else StartCoroutine(ShroomSpawnTimer());
    }
}
