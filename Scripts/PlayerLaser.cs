using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerLaser : MonoBehaviour
{
    //Configuration Parameters(things we need to know before the game)

    [SerializeField] float movementSpeed = 50.0f;


    //Cached Component References (references to other game objects or components of game objects)

    Rigidbody2D laserRb;


    //State variables (to keep track of the variables that govern states)



    // Start is called before the first frame update
    void Start()
    {
        this.laserRb = this.gameObject.GetComponent<Rigidbody2D>();

        //Physics2D.IgnoreLayerCollision(10, 11);
    }

    // Update is called once per frame
    void Update()
    {
        this.laserRb.velocity = new Vector2(0, Vector2.one.y * this.movementSpeed);
    }
}

