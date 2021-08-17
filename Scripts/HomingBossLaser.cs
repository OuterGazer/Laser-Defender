using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HomingBossLaser : MonoBehaviour
{
    //Configuration Parameters(things we need to know before the game)

    [SerializeField] float movementSpeed = 40.0f;
    [SerializeField] float selfDestructCounter = 1.50f;
    [SerializeField] GameObject explosionVFX;
    [SerializeField] AudioClip explosionSFX;
    [SerializeField] GameObject explosionCluster;
    private GameObject player;


    //Cached Component References (references to other game objects or components of game objects)

    private Rigidbody2D laserRb;
    private Vector2 dirToPlayer;


    //State variables (to keep track of the variables that govern states)



    // Start is called before the first frame update
    void Start()
    {
        this.laserRb = this.gameObject.GetComponent<Rigidbody2D>();

        this.FindPlayer(this.gameObject.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        this.laserRb.velocity =  this.dirToPlayer.normalized * this.movementSpeed;

        this.selfDestructCounter -= Time.deltaTime;

        //explode the blasters after some time and spawn 3 clusters out of them. They will go in random directions calculated in the cluster's script
        if(this.selfDestructCounter <= 0)
        {
            AudioSource.PlayClipAtPoint(this.explosionSFX, Camera.main.transform.position, 0.50f);
            GameObject explosion = Instantiate<GameObject>(this.explosionVFX, this.gameObject.transform.position, Quaternion.identity);
            GameObject.Destroy(explosion, 1.0f);

            GameObject.Destroy(this.gameObject);

            for(int i = 0; i < 3; i++)
            {
                Instantiate<GameObject>(this.explosionCluster, this.gameObject.transform.position, Quaternion.identity);
            }            
        }
    }

    public void FindPlayer(Vector2 enemyPos)
    {
        if (GameObject.FindObjectOfType<Player>() != null)
        {
            this.player = GameObject.FindObjectOfType<Player>().gameObject;
            this.dirToPlayer = (Vector2)this.player.transform.position - enemyPos;
        }                
    }
}

