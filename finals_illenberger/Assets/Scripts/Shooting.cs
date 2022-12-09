using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime; //need to import this to have raiseevent variables
using ExitGames.Client.Photon; //for send options

public class Shooting : MonoBehaviourPunCallbacks
{
    public Camera camera;
    public GameObject hitFXPrefab;

    //public Transform turretOrigin;

    [Header ("Gun Settings")]
    public float gunRange = 200f;
    public float fireRate = 1.0f;
    public float fireCooldown;
    private float currentFireCooldown = 0;
    public float projectileSpeed;
    public bool isShootEnabled;

    [Header ("Player Stats")]
    public float startHealth;
    private float health;
    private int eliminationOrder;
    public Image healthbar;
    public bool isHunter = true; //determine if can shift or just darts
    public string mimicName;

    private void OnEnable()
    {
      PhotonNetwork.NetworkingClient.EventReceived += OnEvent; //this is how to add listeners to all listeners in event
    }

    private void OnDisable()
    {
      PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    // Start is called before the first frame update
    void Start()
    {
      //camera = GetComponent<PlayerSetup>().camera;
      health = startHealth;
      healthbar.fillAmount = health / startHealth;
      isShootEnabled = false;
      if(!isHunter) mimicName = "ShifterTemp";
    }

    void Update()
    {
      Debug.DrawRay(camera.transform.position, camera.transform.forward * gunRange, Color.yellow);
      if(currentFireCooldown > 0) {
        currentFireCooldown -= Time.deltaTime;
        GetComponent<PlayerSetup>().playerUi.transform.Find("FireBtn").GetComponent<Button>().interactable = false;
      }
      else if(currentFireCooldown <= 0 && GameManager.instance.cdTurnedOff == true){
          GetComponent<PlayerSetup>().playerUi.transform.Find("FireBtn").GetComponent<Button>().interactable = true;
          isShootEnabled = true;
        }

      if(Input.GetKey(KeyCode.Mouse0) && isShootEnabled && photonView.IsMine){
        Fire();
      }
    }

    public void Fire()
    {
      if(currentFireCooldown <= 0){ //not on cooldown
        if(isHunter){
          photonView.RPC("ShootDart", RpcTarget.All);
        }
        else{
          photonView.RPC("Mimic", RpcTarget.All);
        }
      }
      currentFireCooldown = fireCooldown;
      isShootEnabled = false; 
    }

    [PunRPC]
    public void TakeDamage(int damage, PhotonMessageInfo info)
    {
      this.health -= damage;
      this.healthbar.fillAmount = health/startHealth; //when copying dont forget to put image source for ur fillbar
      //Debug.Log(gameObject.name + " took damage!");


      if(health <= 0){
        string victimName = photonView.Owner.NickName,
               killerName = info.Sender.NickName;
        Player victim = photonView.Owner;
        Player killer = info.photonView.Owner;

        Die(victimName, killerName, victim, killer);
        gameObject.tag = "Dead";
      }
    }

    [PunRPC]
    public void CreateHitFX (Vector3 position)
    {
      GameObject hitFXGameObj = Instantiate(hitFXPrefab, position, Quaternion.identity); //destry it after duration
        Destroy(hitFXGameObj, 0.2f);
    }

    public void Die(string vName, string kName, Player victim, Player killer){
      eliminationOrder++;

      if(photonView.IsMine){
        int viewId = this.photonView.ViewID;
        GameObject deadTxt = GameObject.Find("DeadTxt");

        transform.GetComponent<PlayerMovement>().enabled = false;
        GetComponent<PlayerSetup>().playerUi.transform.Find("FireBtn").GetComponent<Button>().enabled = false;
        if(gameObject.GetComponent<PlayerSetup>().roleTag == "hunter") {
          bool h = true;
          deadTxt.GetComponent<Text>().text = "You were too irresponsible and therefore was sent back to town.";
          GameManager.instance.BodyCount(this.gameObject, h);
        }
        else {
          bool h = false;
          deadTxt.GetComponent<Text>().text = "You were careless. It's too late now.";
          GameManager.instance.BodyCount(this.gameObject, h);
        }

        //event data for elimination
        object[] data = new object[] {vName, kName, eliminationOrder, viewId};

        string winningRole = "";
        if(GameManager.instance.deadShifters.Count == GameManager.instance.shifterMax) winningRole = "hunter";
        else if(GameManager.instance.deadHunters.Count == GameManager.instance.hunterMax) winningRole = "shifter";

        //event data for which roles won
        object[] windata = new object[] {winningRole};

        RaiseEventOptions raiseEventOpts = new RaiseEventOptions
        {
          Receivers = ReceiverGroup.All,
          CachingOption = EventCaching.AddToRoomCache
        };

        SendOptions sendOption = new SendOptions
        {
          Reliability = false
        };

        PhotonNetwork.RaiseEvent((byte) Constants.EliminatedWhoEventCode, data, raiseEventOpts, sendOption);

        Debug.Log(GameManager.instance.deadShifters.Count + "/" + GameManager.instance.shifterMax);
        Debug.Log(GameManager.instance.deadHunters.Count + "/" + GameManager.instance.hunterMax);
        if(GameManager.instance.deadShifters.Count == GameManager.instance.shifterMax || GameManager.instance.deadHunters.Count == GameManager.instance.hunterMax){
          PhotonNetwork.RaiseEvent((byte) Constants.WhoWonEventCode, windata, raiseEventOpts, sendOption);
        }

        //insert spectatorcode here
        //Destroy(this.gameObject); //delete car
      }
    }

    [PunRPC]
    public void Mimic()
    {
      RaycastHit hit;
      Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

      if (Physics.Raycast(ray, out hit, 200)){
        string turnInto = this.GetComponent<MimicHighlight>().animal.GetComponent<Mimic>().mName;
        GameObject childAnimal = null;

        if (hit.collider.gameObject.CompareTag("Mimic")){
          //insert instatiate prefabs of mimicable animal models

          //rifle thru gamemanager animal prefabs, find which matches then retrieve the index
          for(int i = 0; i < GameManager.instance.shifterAnimalPrefabs.Length; i++){
            if(GameManager.instance.shifterAnimalPrefabs[i].GetComponent<Mimic>().mName == turnInto){
                childAnimal = PhotonNetwork.Instantiate(GameManager.instance.shifterAnimalPrefabs[i].name, this.transform.position, this.transform.rotation * Quaternion.identity);
                break;
              }
          }

          //destroy current prefab model then attach new model
          Transform removeTemp = this.gameObject.transform.Find(mimicName);
          removeTemp.parent = null;
          Destroy(removeTemp.gameObject);
          mimicName = childAnimal.name;
          childAnimal.transform.parent = this.gameObject.transform;

        }
      }
    }

    [PunRPC]
    public void ShootDart()
    {
      Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
      RaycastHit hit;
      //Ray ray = new Ray (turretOrigin.transform.position, turretOrigin.transform.forward);

      if (Physics.Raycast(ray, out hit, gunRange)){
        //Debug.Log(hit.collider.gameObject.name + " tag: " + hit.collider.gameObject.tag);
        Debug.DrawRay(camera.transform.position, camera.transform.forward * hit.distance, Color.yellow);

        if (hit.collider.gameObject.CompareTag("Player") && hit.collider.gameObject.GetComponent<PlayerSetup>().roleTag != "hunter" && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine){
          hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 1);
        }
        else if(hit.collider.gameObject.CompareTag("Mimic")){ //if hits normal animal, get damaged
          GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 1);
          //Debug.Log("this isnt what your hunting for!");
          GameObject warningTxt = GameObject.Find("DeadTxt");
          StartCoroutine(WarningText(warningTxt));
        }
      }

      photonView.RPC("CreateHitFX", RpcTarget.All, hit.point);
    }

    IEnumerator ReturnPlayersToLobby(GameObject wtext)
    {
      float leaveRoomTime = 5.0f;

      while (leaveRoomTime > 0){
        yield return new WaitForSeconds(1.0f);
        leaveRoomTime--;

        wtext.transform.GetChild(1).GetComponent<Text>().text = "Leaving room in " + leaveRoomTime.ToString(".00") ;
      }
      GameManager.instance.Gameover();
    }

    IEnumerator WarningText(GameObject warning)
    {
      float warningTime = 7f;

      while(warningTime > 0){
        yield return new WaitForSeconds(0.3f);
        warning.GetComponent<Text>().text = "This isn't your prey, hunter.";
        warningTime--;
      }
      warning.GetComponent<Text>().text = "";
    }

    void OnEvent(EventData photonEvent)
    {
      if(photonEvent.Code == (byte)Constants.EliminatedWhoEventCode){
        object[] data = (object[]) photonEvent.CustomData;

        string vName = (string)data[0],
              kName = (string)data[1];
        eliminationOrder = (int)data[2];
        int viewId = (int)data[3];

        GameObject elimOrderUiTxt = GameManager.instance.eliminateeTxtUI[eliminationOrder-1]; //moves the textbox
        elimOrderUiTxt.SetActive(true);

        if(vName == kName) elimOrderUiTxt.GetComponent<Text>().text = vName +  " had too many offenses.";
        else elimOrderUiTxt.GetComponent<Text>().text = vName +  " was caught by " + kName;
      }

      if(photonEvent.Code == (byte)Constants.WhoWonEventCode){
        object[] data = (object[]) photonEvent.CustomData;

        string winner = (string)data[0];

        Debug.Log(winner + "s are the winner!");

        GameObject winnertxt = GameManager.instance.winnerTxtUI; //moves the textbox
        winnertxt.GetComponent<Image>().enabled = true;

        if(winner == "shifter") winnertxt.transform.GetChild(0).GetComponent<Text>().text = "The fae escaped into the feywilds."; //win fae
        else winnertxt.transform.GetChild(0).GetComponent<Text>().text = "The hunters spared no fae."; //win hunters

        foreach(GameObject go in GameManager.instance.playerList){
          GetComponent<PlayerMovement>().enabled = false;
          GetComponent<PlayerSetup>().playerUi.transform.Find("FireBtn").GetComponent<Button>().interactable = false;
        }
        StartCoroutine(ReturnPlayersToLobby(winnertxt));
      }

      if(photonEvent.Code == (byte)Constants.MushroomCollectedEventCode){
        object[] data = (object[]) photonEvent.CustomData;

        string area = (string)data[0];

        GameManager.instance.HunterAlert(area);
        Debug.Log("hunters are alerted!");
      }
    }
}
