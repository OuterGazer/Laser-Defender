using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyLaser_4x : MonoBehaviour
{
    //Configuration Parameters(things we need to know before the game)

    [SerializeField] float movementSpeed = 40.0f;


    //Cached Component References (references to other game objects or components of game objects)

    private Rigidbody2D laserRb;
    private Vector2 shotDirection;


    //State variables (to keep track of the variables that govern states)



    // Start is called before the first frame update
    void Start()
    {
        this.laserRb = this.gameObject.GetComponent<Rigidbody2D>();
    }

    public void SetShotDirection(Vector2 inDir)
    {
        this.shotDirection = inDir;
    }

    // Update is called once per frame
    void Update()
    {
        this.laserRb.velocity = this.shotDirection.normalized * this.movementSpeed;
    }
}

