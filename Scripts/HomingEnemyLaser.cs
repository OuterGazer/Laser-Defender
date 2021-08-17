using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HomingEnemyLaser : MonoBehaviour
{
    //Configuration Parameters(things we need to know before the game)

    [SerializeField] float movementSpeed = 40.0f;
    private GameObject player;


    //Cached Component References (references to other game objects or components of game objects)

    private Rigidbody2D laserRb;
    private Vector2 dirToPlayer;


    //State variables (to keep track of the variables that govern states)



    // Start is called before the first frame update
    void Start()
    {
        this.laserRb = this.gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        this.laserRb.velocity =  this.dirToPlayer.normalized * this.movementSpeed;
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

