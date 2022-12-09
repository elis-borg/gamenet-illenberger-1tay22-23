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

    public Transform turretOrigin;

    [Header ("Gun Settings")]
    public float gunRange = 200f;
    public float laserDuration = 0.5f;
    public float fireRate = 1.0f;
    public float fireCooldown;
    private float currentFireCooldown = 0;
    public float projectileSpeed;

    private LineRenderer laserLine;

    [Header ("Player Stats")]
    public float startHealth;
    private float health;
    private int eliminationOrder;
    public Image healthbar;
    public bool isHunter = true; //determine if vehicle is of projectile or laser variety;

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
      health = startHealth;
      healthbar.fillAmount = health / startHealth;

      if(isHunter){
        //laserLine = GameObject.Find("Laser").transform.GetChild(3).GetComponent<LineRenderer>();
        laserLine = turretOrigin.GetComponent<LineRenderer>();
      }
    }

    void Update()
    {
      if(currentFireCooldown > 0) {
        currentFireCooldown -= Time.deltaTime;
        GetComponent<PlayerSetup>().playerUi.transform.Find("FireBtn").GetComponent<Button>().interactable = false;
      }
      else if(currentFireCooldown <= 0 && GameManager.instance.cdTurnedOff == true) GetComponent<PlayerSetup>().playerUi.transform.Find("FireBtn").GetComponent<Button>().interactable = true;
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
    }

    [PunRPC]
    public void TakeDamage(int damage, PhotonMessageInfo info)
    {
      this.health -= damage;
      this.healthbar.fillAmount = health/startHealth; //when copying dont forget to put image source for ur fillbar

      if(health <= 0){
        string victimName = photonView.Owner.NickName,
               killerName = info.Sender.NickName;

        Die(victimName, killerName);
        gameObject.tag = "Dead";
      }
    }

    [PunRPC]
    public void CreateHitFX (Vector3 position)
    {
      GameObject hitFXGameObj = Instantiate(hitFXPrefab, position, Quaternion.identity); //destry it after duration
        Destroy(hitFXGameObj, 0.2f);
    }

    public void Die(string victim, string killer){
      eliminationOrder++;

      if(photonView.IsMine){
        int viewId = this.photonView.ViewID;
        GameObject deadTxt = GameObject.Find("DeadTxt");

        transform.GetComponent<PlayerMovement>().enabled = false;
        GetComponent<PlayerSetup>().playerUi.transform.Find("FireBtn").GetComponent<Button>().interactable = false;
        deadTxt.GetComponent<Text>().text = "You were eliminated from the race.";

        //GameManager.instance.RemovePlayer(this.gameObject); //removes them from playerlist

        //event data for elimination
        object[] data = new object[] {victim, killer, eliminationOrder, viewId};
        //event data for winning
        object[] windata = new object[] {killer};

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

        //int remainingshiftersneededcheck = PhotonNetwork.CurrentRoom.PlayerCount-1;
        //int remainingshiftersneededcheck = GameManager.instance.shifterMax;
        //int remainingshiftersneededcheck = GameManager.instance.hunterMax;

        Debug.Log(GameManager.instance.deadShifters.Count + "/" + GameManager.instance.shifterMax); //dead shifters needed to declare hunters won
        if(GameManager.instance.deadShifters.Count == GameManager.instance.shifterMax){
          PhotonNetwork.RaiseEvent((byte) Constants.WhoWonEventCode, windata, raiseEventOpts, sendOption);
        }
        else if(GameManager.instance.deadHunters.Count == GameManager.instance.hunterMax){
          PhotonNetwork.RaiseEvent((byte)Constants.WhoWonEventCode, windata, raiseEventOpts, sendOption);
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
        Debug.Log(hit.collider.gameObject.name);

        if (hit.collider.gameObject.CompareTag("Mimic")){
          //insert instatiate prefabs of mimicable animal models
          //GameObject childAnimal = PhotonNetwork.Instantiate(mushroomPrefab.name, mushroomSpawns[lastShroomPoint].GetComponent<Transform>().position, Quaternion.identity);

          //destroy current prefab model then attach new model
          Transform removeTemp = this.gameObject.transform.Find("ShifterTemp");
          removeTemp.parent = null;
          //childAnimal.transform.parent = this.gameObject.transform;

        }
      }
    }

    [PunRPC]
    IEnumerator ShootDart()
    {
      Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

      laserLine.enabled = true;
      laserLine.SetPosition(0, turretOrigin.position);

      RaycastHit hit;
      //Ray ray = new Ray (turretOrigin.transform.position, turretOrigin.transform.forward);


      if (Physics.Raycast(ray, out hit, gunRange)){
        //Debug.Log(hit.collider.gameObject.name);
        //Debug.DrawRay(turretOrigin.transform.position, turretOrigin.transform.forward * hit.distance, Color.yellow); //u can only view this in scene!

        laserLine.SetPosition(1, hit.point);

        if (hit.collider.gameObject.CompareTag("Player") && hit.collider.gameObject.GetComponent<PlayerSetup>().roleTag != "hunter" && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine){
          hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 60);
        }
        else if(hit.collider.gameObject.CompareTag("Mimic")){ //if hitss normal animal, get damaged
          this.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 1);
        }
      }
      else{
        laserLine.SetPosition(1,turretOrigin.position + turretOrigin.forward * gunRange); //limited beam of light
      }

      yield return new WaitForSeconds(laserDuration);
      laserLine.enabled = false;
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

        elimOrderUiTxt.GetComponent<Text>().text = vName +  " is eliminated by " + kName;
      }

      if(photonEvent.Code == (byte)Constants.WhoWonEventCode){
        object[] data = (object[]) photonEvent.CustomData;

        string kName = (string)data[0];

        Debug.Log(kName + " is the winner!");
        //WinnerAnnouncement(kName);

        GameObject winnertxt = GameManager.instance.winnerTxtUI; //moves the textbox
        winnertxt.GetComponent<Image>().enabled = true;

        winnertxt.transform.GetChild(0).GetComponent<Text>().text = kName + " is the last man standing!";
        foreach(GameObject go in GameManager.instance.playerList){
          GetComponent<PlayerMovement>().enabled = false;
          GetComponent<PlayerSetup>().playerUi.transform.Find("FireBtn").GetComponent<Button>().interactable = false;
        }
        StartCoroutine(ReturnPlayersToLobby(winnertxt));
      }
    }
}
