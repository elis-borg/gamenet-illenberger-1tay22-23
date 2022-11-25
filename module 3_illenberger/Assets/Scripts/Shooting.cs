using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

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
    public float projectileSpeed;

    private LineRenderer laserLine;

    [Header ("Player Stats")]
    public float startHealth = 500;
    private float health;
    public Image healthbar;
    public bool isLaserWeapon = true; //determine if vehicle is of projectile or laser variety;

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
      if(fireCooldown > 0) fireCooldown -= Time.deltaTime;
    }

    public void Fire()
    {
      if(fireCooldown <= 0){ //not on cooldown
        if(isLaserWeapon){
          RaycastHit hit;
          //Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
          Ray ray = new Ray (turretOrigin.transform.position, turretOrigin.transform.forward);

          laserLine.SetPosition(0, turretOrigin.position);

          if (Physics.Raycast(ray, out hit, gunRange)){
            Debug.Log(hit.collider.gameObject.name);
            //Debug.DrawRay(turretOrigin.transform.position, turretOrigin.transform.forward * hit.distance, Color.yellow); //u can only view this in scene!

            laserLine.SetPosition(1, hit.point);
            photonView.RPC("CreateHitFX", RpcTarget.All, hit.point);

            if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine){
              hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 25);
            }
          }
          else{
            laserLine.SetPosition(1,turretOrigin.transform.position + turretOrigin.transform.forward * gunRange); //limited beam of light
          }
          StartCoroutine(ShootLaser());
        }
        else{
          photonView.RPC("ShootProjectile", RpcTarget.All);
        }
      }
    }

    [PunRPC]
    public void TakeDamage(int damage, PhotonMessageInfo info)
    {
      this.health -= damage;
      this.healthbar.fillAmount = health/startHealth; //when copying dont forget to put image source for ur fillbar

      if(health <= 0){
        Die();
        string victimName = photonView.Owner.NickName,
               killerName = info.Sender.NickName;

        photonView.RPC("KillerNotification", RpcTarget.All, victimName, killerName); //gives everyone a copy version
      }
    }

    [PunRPC]
    public void CreateHitFX (Vector3 position)
    {
      GameObject hitFXGameObj = Instantiate(projectileFXPrefab, position, Quaternion.identity); //destry it after duration
        Destroy(hitFXGameObj, 0.2f);
    }

    public void Die(){
      if(photonView.IsMine){
        GameObject deadTxt = GameObject.Find("DeadTxt");


        transform.GetComponent<VehicleMovement>().enabled = false;
        deadTxt.GetComponent<Text>().text = "You were eliminated from the race.";
        Destroy(this.gameObject); //delete car

        //insert code for spectator here
      }
    }

    [PunRPC]
    public void ShootProjectile()
    {
      GameObject p = PhotonNetwork.Instantiate(projectilePrefab.name, turretOrigin.transform.position, turretOrigin.transform.rotation*Quaternion.identity);
      Debug.Log("Projectile created.");
      Physics.IgnoreCollision(p.GetComponent<Collider>(), this.GetComponent<Collider>());
      p.GetComponent<Projectile>().source = this.gameObject;
      p.GetComponent<Rigidbody>().AddForce(turretOrigin.transform.forward*projectileSpeed);

      Destroy(p,2f); //to clear memory
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

      this.transform.GetComponent<PlayerSetup>().playerUiPrefab.transform.Find("FireBtn").GetComponent<Button>().interactable = false; //walk freely but no more firing

      winnerImg.transform.Find("Winner").GetComponent<Text>().text = winner + " is the last racer standing!";
    }
    RacingGameManager.instance.Gameover();
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

      killerImg.transform.Find("Text").GetComponent<Text>().text = victim  + "'s vehicle was shot down by " + killer;
    }
    killerImg.transform.Find("Text").GetComponent<Text>().text = "";
    killerImg.GetComponent<Image>().enabled = false;
  }

  IEnumerator ShootLaser()
  {
    laserLine.enabled = true;
    yield return new WaitForSeconds(laserDuration);
    laserLine.enabled = false;
  }
}
