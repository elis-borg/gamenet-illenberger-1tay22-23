using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shooting : MonoBehaviour
{
    [SerializeField]
    Camera fpsCamera;

    [SerializeField]
    public float fireRate = 0.1f;
    private float fireTimer = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        if (Input.GetButton("Fire1") && fireTimer > fireRate)
        {
            fireTimer = 0.0f;
            Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                Debug.Log(hit.collider.gameObject.name); //just to test out if ray function is working
                if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
                {
                  //second condition is to make sure u dont shoot urself/ arent shooting self then it will send the next line

                  /*in offline scenario ud just be calling TakeDamage.cs but
                  in online, u need to broadcast to all players in the room that
                  specific player took damge via RPC (Remote Procedure Calls)

                  calls remote clients in the same room, to make a () an RPC u need to attribute it
                  [PunRPC] (do that above the function)

                  for calling:
                  */
                  hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 10);
                }
            }
        }
    }
}
