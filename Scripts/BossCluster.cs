using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCluster : MonoBehaviour
{
    //Configuration Parameters(things we need to know before the game)

    [SerializeField] float movementSpeed = 40.0f;
    private float initialAngle;
    private Vector2 movementDir = Vector2.right;


    //Cached Component References (references to other game objects or components of game objects)

    private Rigidbody2D laserRb;


    //State variables (to keep track of the variables that govern states)



    // Start is called before the first frame update
    void Start()
    {
        this.laserRb = this.gameObject.GetComponent<Rigidbody2D>();

        this.initialAngle = Random.Range(0.0f, 359.0f);
        this.movementDir = Quaternion.Euler(0.0f, 0.0f, this.initialAngle) * this.movementDir;
    }

    // Update is called once per frame
    void Update()
    {
        this.laserRb.velocity = this.movementDir.normalized * this.movementSpeed;
    }
}

