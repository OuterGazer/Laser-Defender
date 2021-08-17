using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPathing : MonoBehaviour
{
    //Configuration Parameters(things we need to know before the game)

    private WaveConfig waveInfo;
    private int pathNumber;
    private float moveSpeed;
    //private EnemySpawner enemySpawner;
    private List<Transform> waypoints;
    private int nextWaypoint = 0;


    //Cached Component References (references to other game objects or components of game objects)


    //State variables (to keep track of the variables that govern states)


    // Start is called before the first frame update
    void Start()
    {
        //this.enemySpawner = GameObject.FindObjectOfType<EnemySpawner>();
        this.waypoints = this.waveInfo.GetPathWaypoints(this.pathNumber); //Get the waypoints from the current wave featured in enemyspawner
    }

    // Update is called once per frame
    void Update()
    {
        this.MoveEnemy();
    }

    /// <summary>
    /// Sets all the wave information for the spawned enemy
    /// </summary>
    /// <param name="inWaveInfo">The wave from which the information should be read</param>
    public void SetWaveInfo(WaveConfig inWaveInfo, int inPathNumber, float inMoveSpeed)
    {
        this.waveInfo = inWaveInfo;
        this.pathNumber = inPathNumber;
        this.moveSpeed = inMoveSpeed;
    }

    private void MoveEnemy()
    {
        if (this.gameObject.transform.position != this.waypoints[this.waypoints.Count - 1].position)
        {
            this.gameObject.transform.position = Vector2.MoveTowards(this.gameObject.transform.position,
                                                                     this.waypoints[this.nextWaypoint].position,
                                                                     this.moveSpeed * Time.deltaTime);

            if (this.gameObject.transform.position == this.waypoints[this.nextWaypoint].position)
            {
                this.nextWaypoint++;
            }
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
