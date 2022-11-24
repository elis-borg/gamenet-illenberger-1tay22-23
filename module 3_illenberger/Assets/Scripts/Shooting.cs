using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Shooting : MonoBehaviourPunCallbacks
{
    public Camera camera;
    public GameObject projectileFXPrefab,
                      projectilePrefab,
                      laserFXPrefab;

    public Transform turretOrigin;

    public float gunRange = 50f,
                laserDuration = 0.05f,
                projectileSpeed;

    [Header ("Player Stats")]
    public float startHealth = 100;
    private float health;
    public Image healthbar;
    public bool isLaserWeapon = true; //determine if vehicle is of projectile or laser variety;

    // Start is called before the first frame update
    void Start()
    {
      health = startHealth;
      healthbar.fillAmount = health / startHealth;
      if(isLaserWeapon){
        //laserLine = GetComponent<LineRenderer>();
      }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Fire()
    {
      if(isLaserWeapon){
        RaycastHit hit;
        //Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if (Physics.Raycast(turretOrigin.transform.position, turretOrigin.transform.forward, out hit, 200)){
          Debug.Log(hit.collider.gameObject.name);
          Debug.DrawRay(turretOrigin.transform.position, turretOrigin.transform.forward * hit.distance, Color.yellow); //u can only view this in scene!

          photonView.RPC("CreateHitFX", RpcTarget.All, hit.point);

          if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine){
            //need to call an rpc function to deduct the playerhealth for every hit
            hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 25);
            Debug.Log("Called TakeDamage();");
          }
        }
      }
      else{
        GameObject p = Instantiate(projectilePrefab, turretOrigin.transform.position, turretOrigin.transform.rotation);
        Physics.IgnoreCollision(p.GetComponent<Collider>(), this.GetComponent<Collider>());
        //b.GetComponent<Bullet>().source = this.gameObject;
        p.GetComponent<Rigidbody>().AddForce(turretOrigin.transform.forward*projectileSpeed);
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


        photonView.RPC("KillerNotification", RpcTarget.All, victimName, killerName); //gives everyone a copy version
      }
    }

    //make particle fx apparent thruout the whole server
    [PunRPC]
    public void CreateHitFX (Vector3 position)
    {
      GameObject hitFXGameObj = Instantiate(projectileFXPrefab, position, Quaternion.identity); //destry it after duration
        Destroy(hitFXGameObj, 0.2f);
    }

    public void Die(){
      if(photonView.IsMine){

        //StartCoroutine(RespawnCountdown());
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
    //GameManager.instance.killcountRoom();
    //RacingGameManager.instance.Gameover();
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
}
