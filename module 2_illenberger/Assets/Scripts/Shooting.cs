using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Shooting : MonoBehaviourPunCallbacks
{
    public Camera camera;
    public GameObject hitFXPrefab;

    [Header ("Player Stats")]
    public float startHealth = 100;
    private float health;
    public Image healthbar;
    private int killCount;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
      health = startHealth;
      healthbar.fillAmount = health / startHealth;
      animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Fire()
    {
      RaycastHit hit;
      Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

      if (Physics.Raycast(ray, out hit, 200)){
        Debug.Log(hit.collider.gameObject.name);
        /* THIS WAS MOVED TO CreateHitFX();

        GameObject hitFXGameObj = Instantiate(hitFXPrefab, hit.point, Quaternion.identity); //destry it after duration
        Destroy(hitFXGameObj, 0.2f);*/

        photonView.RPC("CreateHitFX", RpcTarget.All, hit.point); //its in ALL becos no need to instantiate the fx for players who are just coming in the room

        if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine){ //dont hit ourselves when we fire
          //need to call an rpc function to deduct the playerhealth for every hit
          hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 25); //allbuffered lets all current and future players receive the broadcast

          if(hit.collider.gameObject.GetComponent<Shooting>().health <= 0){
            Debug.Log(hit.collider.GetComponent<Shooting>().health);
            killCount++;
            }
          Debug.Log(photonView.Owner.NickName + "kill count is at " + killCount);

          if(killCount > 9) {
            string winner = photonView.Owner.NickName;
            Debug.Log(winner + " reached 10 kills!");
            photonView.RPC("WinnerAnnouncement", RpcTarget.AllBuffered, winner);
          }
        }
      }
    }

    [PunRPC]
    public void TakeDamage(int damage, PhotonMessageInfo info)
    {
      this.health -= damage;
      this.healthbar.fillAmount = health/startHealth;

      if(health <= 0){
        Die();
        string victimName = photonView.Owner.NickName,
               killerName = info.Sender.NickName;

        /*but who shot who, every damage taken should output who shot it tothe player, PhotonMessageInfo parameter does that, it is also a default for all RPC functions
        Debug.Log(info.Sender.NickName + " killed " + info.photonView.Owner.NickName); //is now found in KillerNotification();*/

        photonView.RPC("KillerNotification", RpcTarget.All, victimName, killerName); //gives everyone a copy version
      }
    }

    //make particle fx apparent thruout the whole server
    [PunRPC]
    public void CreateHitFX (Vector3 position)
    {
      GameObject hitFXGameObj = Instantiate(hitFXPrefab, position, Quaternion.identity); //destry it after duration
        Destroy(hitFXGameObj, 0.2f);
    }

    public void Die(){
      if(photonView.IsMine){
        animator.SetBool("isDead", true);

        StartCoroutine(RespawnCountdown());
      }
    }

  [PunRPC]
  IEnumerator WinnerAnnouncement(string winner)
  {
    GameObject winnerImg = GameObject.Find("WinnerTxt").transform.GetChild(0).gameObject;
    winnerImg.GetComponent<Image>().enabled = true;
    float ejectTime = 10.0f;

    while(ejectTime > 0){
      yield return new WaitForSeconds(1.0f);
      ejectTime--;

      this.transform.GetComponent<PlayerSetup>().playerUiPrefab.transform.Find("FireBtn").GetComponent<Button>().enabled = false; //walk freely but no more firing

      winnerImg.transform.Find("Winner").GetComponent<Text>().text = winner + " won the match!";
      winnerImg.transform.Find("RespawnTimer").GetComponent<Text>().text = "Returning to lobby in " + ejectTime.ToString(".00");
    }
    //GameManager.instance.WinnerDetermined(); //other player doesnt get booted due to lag between countdowns but it should work
    //GameManager.instance.LeaveRoom();
    GameManager.instance.Gameover();
  }

  [PunRPC]
  IEnumerator KillerNotification(string victim, string killer)
  {
    GameObject killerImg = GameObject.Find("KillerTxt").transform.GetChild(0).gameObject;
    killerImg.GetComponent<Image>().enabled = true;
    float displayTime = 4.0f;

    while(displayTime > 0){
      yield return new WaitForSeconds(1.0f);
      displayTime--;

      killerImg.transform.Find("Text").GetComponent<Text>().text = victim  + " was tactically nuked by " + killer;
    }
    killerImg.transform.Find("Text").GetComponent<Text>().text = "";
    killerImg.GetComponent<Image>().enabled = false;
  }

  IEnumerator RespawnCountdown()
  {
    GameObject respawnTxt = GameObject.Find("RespawnTxt");
    float respawnTime = 5.0f;

    while(respawnTime > 0){
      yield return new WaitForSeconds(1.0f);
      respawnTime--;

      transform.GetComponent<PlayerMovementCtrlr>().enabled = false; //dead means no moving
      respawnTxt.GetComponent<Text>().text = "You are killed. Respawning in " + respawnTime.ToString(".00");
    }

    animator.SetBool("isDead", false);
    respawnTxt.GetComponent<Text>().text = "";

    /*respawn player after coutndown and reenable - CHANGE RESPAWN POINTS TO SINGLETON CLASS IN GAMEMANAGER.CS LATER
    int randomPointX = Random.Range(-20,20);
    int randomPointZ = Random.Range(-20,20);*/

    this.transform.position = GameManager.instance.PickRespawnPoint();
    transform.GetComponent<PlayerMovementCtrlr>().enabled = true;

    photonView.RPC("RegainHealth", RpcTarget.AllBuffered);
  }

  [PunRPC] //healthbar needs to be uptodate for other players in the room
  public void RegainHealth()
  {
    health = startHealth;
    healthbar.fillAmount = health/startHealth;
  }
}
