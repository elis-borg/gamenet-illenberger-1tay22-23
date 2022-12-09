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
    public int currentPoint,
              shroomsNeeded,
              collectedShrooms;
    public bool firstShroomSpawn = true,
                collected = false,
                relocating = false;

    void Awake()
    {
      if(instance == null) instance = this;
      else if(instance != this) Destroy(gameObject);

      //DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
      if(firstShroomSpawn == true && GameManager.instance.cdTurnedOff == true){
        //Debug.Log("shroom spawning conditions met");
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
        //PickShroomSpawnPoint(currentPoint);
        Debug.Log("first mushroom spawned!");
        currentPoint = Random.Range(0, mushroomSpawns.Length);
        Debug.Log(currentPoint + ": " + mushroomSpawns[currentPoint].name);

        mushroom = PhotonNetwork.Instantiate(mushroomPrefab.name, mushroomSpawns[currentPoint].GetComponent<Transform>().position, Quaternion.identity);
      }
      else{
        int newPoint = currentPoint;

        //will loop until the new point isnt the same as current
        /*while(newPoint == currentPoint){
          PickShroomSpawnPoint(newPoint);
        }*/

        mushroom = PhotonNetwork.Instantiate(mushroomPrefab.name, mushroomSpawns[newPoint].GetComponent<Transform>().position, Quaternion.identity);
      }
      Debug.Log("mushroom is at (" + currentPoint + ") " + mushroomSpawns[currentPoint].name);
    }

    private int PickShroomSpawnPoint(int num)
    {
      return num = Random.Range(0, mushroomSpawns.Length);
    }

    IEnumerator ShroomSpawnTimer()
    {
      float timer = 10f; //20

      while (timer > 0){
        yield return new WaitForSeconds(1.0f);
        timer--;
      }
      SpawnMushroom();
    }

    IEnumerator RelocateShroom()
    {
      float timer = 5f; //15  //shifter has this much time to look for the shroom before it spawns elsewhere, avoids hunter camping if they spot it first

      while (timer > 0){
        yield return new WaitForSeconds(1.0f);
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
      if(collectedShrooms >= shroomsNeeded) GameManager.instance.Gameover();
      Debug.Log("shrooms: " + collectedShrooms + "/" + shroomsNeeded);
    }
}
