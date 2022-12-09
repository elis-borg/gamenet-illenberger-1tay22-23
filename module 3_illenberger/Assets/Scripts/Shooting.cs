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
    public GameObject projectileFXPrefab,
                      projectilePrefab;

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
    public float startHealth = 500;
    private float health;
    private int eliminationOrder;
    public Image healthbar;
    public bool isLaserWeapon = true; //determine if vehicle is of projectile or laser variety;

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

      if(isLaserWeapon){
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
      else if(currentFireCooldown <= 0 && RacingGameManager.instance.cdTurnedOff == true) GetComponent<PlayerSetup>().playerUi.transform.Find("FireBtn").GetComponent<Button>().interactable = true;
    }

    public void Fire()
    {
      if(currentFireCooldown <= 0){ //not on cooldown
        if(isLaserWeapon){
          photonView.RPC("ShootLaser", RpcTarget.All);
        }
        else{
          photonView.RPC("ShootProjectile", RpcTarget.All);
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
        gameObject.tag = "Dead"; //change game object's tag to dead
      }
    }

    [PunRPC]
    public void CreateHitFX (Vector3 position)
    {
      GameObject hitFXGameObj = Instantiate(projectileFXPrefab, position, Quaternion.identity); //destry it after duration
        Destroy(hitFXGameObj, 0.2f);
    }

    public void Die(string victim, string killer){
      eliminationOrder++;

      if(photonView.IsMine){
        int viewId = this.photonView.ViewID;
        GameObject deadTxt = GameObject.Find("DeadTxt");

        transform.GetComponent<VehicleMovement>().enabled = false;
        GetComponent<PlayerSetup>().playerUi.transform.Find("FireBtn").GetComponent<Button>().interactable = false;
        deadTxt.GetComponent<Text>().text = "You were eliminated from the race.";

        RacingGameManager.instance.RemovePlayer(this.gameObject); //removes them from playerlist

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

        int remainingPlayersNeededCheck = PhotonNetwork.CurrentRoom.PlayerCount-1;
        Debug.Log(RacingGameManager.instance.deadPlayerList.Count + "/" + remainingPlayersNeededCheck);
        if(RacingGameManager.instance.deadPlayerList.Count == PhotonNetwork.CurrentRoom.PlayerCount-1){
          PhotonNetwork.RaiseEvent((byte) Constants.WhoWonEventCode, windata, raiseEventOpts, sendOption);
        }

        //insert spectatorcode here
        //Destroy(this.gameObject); //delete car
      }
    }

    [PunRPC]
    public void ShootProjectile()
    {
      GameObject p = PhotonNetwork.Instantiate(projectilePrefab.name, turretOrigin.transform.position, turretOrigin.transform.rotation*Quaternion.identity);
      Physics.IgnoreCollision(p.GetComponent<Collider>(), this.GetComponent<Collider>());
      p.GetComponent<Projectile>().source = this.gameObject;
      p.GetComponent<Rigidbody>().AddForce(turretOrigin.transform.forward*projectileSpeed);

      Destroy(p,2f); //to clear spawned projectiles from memory
    }

    [PunRPC]
    IEnumerator ShootLaser()
    {
      laserLine.enabled = true;
      laserLine.SetPosition(0, turretOrigin.position);

      RaycastHit hit;
      Ray ray = new Ray (turretOrigin.transform.position, turretOrigin.transform.forward);


      if (Physics.Raycast(ray, out hit, gunRange)){
        //Debug.Log(hit.collider.gameObject.name);
        //Debug.DrawRay(turretOrigin.transform.position, turretOrigin.transform.forward * hit.distance, Color.yellow); //u can only view this in scene!

        laserLine.SetPosition(1, hit.point);

        if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine){
          hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 60);
        }
      }
      else{
        laserLine.SetPosition(1,turretOrigin.transform.position + turretOrigin.transform.forward * gunRange); //limited beam of light
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
      RacingGameManager.instance.Gameover();
    }

    void OnEvent(EventData photonEvent)
    {
      if(photonEvent.Code == (byte)Constants.EliminatedWhoEventCode){
        object[] data = (object[]) photonEvent.CustomData;

        string vName = (string)data[0],
              kName = (string)data[1];
        eliminationOrder = (int)data[2];
        int viewId = (int)data[3];

        GameObject elimOrderUiTxt = RacingGameManager.instance.eliminateeTxtUI[eliminationOrder-1]; //moves the textbox
        elimOrderUiTxt.SetActive(true);

        elimOrderUiTxt.GetComponent<Text>().text = vName +  " is eliminated by " + kName;
      }

      if(photonEvent.Code == (byte)Constants.WhoWonEventCode){
        object[] data = (object[]) photonEvent.CustomData;

        string kName = (string)data[0];

        Debug.Log(kName + " is the winner!");
        //WinnerAnnouncement(kName);

        GameObject winnertxt = RacingGameManager.instance.winnerTxtUI; //moves the textbox
        winnertxt.GetComponent<Image>().enabled = true;

        winnertxt.transform.GetChild(0).GetComponent<Text>().text = kName + " is the last man standing!";
        foreach(GameObject go in RacingGameManager.instance.playerList){
          GetComponent<VehicleMovement>().enabled = false;
          GetComponent<PlayerSetup>().playerUi.transform.Find("FireBtn").GetComponent<Button>().interactable = false;
        }
        StartCoroutine(ReturnPlayersToLobby(winnertxt));
      }
    }
}
