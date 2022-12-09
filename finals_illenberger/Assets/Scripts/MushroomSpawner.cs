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
    public int lastShroomPoint,
              shroomsNeeded,
              collectedShrooms;
    public bool firstShroomSpawn = true,
                collected = false;

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
        Debug.Log("shroom spawning conditions met");
        StartCoroutine(ShroomSpawnTimer());
        firstShroomSpawn = !firstShroomSpawn;
      }
      else if(mushroom != null && collected == false){
        Debug.Log("calling relocateshroom()");
        StartCoroutine(RelocateShroom());
      }
    }

    public void SpawnMushroom()
    {
      if(firstShroomSpawn){
        lastShroomPoint = Random.Range(0, mushroomSpawns.Length);
        //lastShroomPoint = num;

        mushroom = PhotonNetwork.Instantiate(mushroomPrefab.name, mushroomSpawns[lastShroomPoint].GetComponent<Transform>().position, Quaternion.identity);
      }
      else{
        int num = lastShroomPoint;
        while(num == lastShroomPoint) num = Random.Range(0, mushroomSpawns.Length); //will loop until the new point is not the same as the last one

        mushroom = PhotonNetwork.Instantiate(mushroomPrefab.name, mushroomSpawns[num].GetComponent<Transform>().position, Quaternion.identity);
      }
    }

    IEnumerator ShroomSpawnTimer()
    {
      float timer = 20.0f;

      while (timer > 0){
        yield return new WaitForSeconds(1.0f);
        timer--;
        Debug.Log("shroom spawns in " + timer);
      }
      SpawnMushroom();
    }

    IEnumerator RelocateShroom()
    {
      float timer = 15f; //shifter has this much time to look for the shroom before it spawns elsewhere, avoids hunter camping if they spot it first

      while (timer > 0){
        yield return new WaitForSeconds(1.0f);
        timer--;
        Debug.Log("shroom relocates in " + timer);
      }

      Destroy(mushroom);
      Debug.Log("shroom is destroyed");
      SpawnMushroom();
    }
}
