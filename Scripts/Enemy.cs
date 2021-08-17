using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] float health = 100;
    [SerializeField] int enemyScore;
    private float shotCounter; //for debuggin purposes
    [SerializeField] float minTimeBetweenShots = 0.20f;
    [SerializeField] float maxTimeBetweenShots = 3.0f;

    private Vector2 offset1; //for Enemy 2
    private Vector2 offset2; //for Enemy 5
    private Transform[] enemy7_shootingPos = new Transform[5];
    private EnemyLaser_4x shotDirection;
    private HomingEnemyLaser shotDirectionHoming;

    private SpriteRenderer enemySprite;
    private Vector2 shipBow;
    [Header("Cache References")]
    [SerializeField] GameObject shootingLaser;

    [SerializeField] GameObject explosionVfx;
    [SerializeField] float explosionDuration = 1.0f;

    private GameObject laserCharge;
    private bool hasCharged = false;
    
    private AudioSource playSFX;
    [Header("Sound Info")]
    [Range(0.0f, 1.0f)][SerializeField] float volumeSFX = 0.70f;
    [SerializeField] AudioClip chargingLaserSFX;
    [SerializeField] AudioClip shootingLaserSFX;
    [SerializeField] AudioClip explosionSFX;

    private UIManager UIManager;

    [Header("Power Ups")]
    [SerializeField] GameObject[] PowerUps;


    private void Awake()
    {
        this.UIManager = GameObject.FindObjectOfType<UIManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        this.enemySprite = this.gameObject.GetComponent<SpriteRenderer>();

        this.shotCounter = Random.Range(this.minTimeBetweenShots, this.maxTimeBetweenShots);

        this.playSFX = this.gameObject.GetComponent<AudioSource>();

        if (this.gameObject.name.Equals("Enemy 3(Clone)"))
        {
            this.laserCharge = this.gameObject.GetComponentInChildren<Animator>().gameObject;
            this.laserCharge.SetActive(false);
        }

        this.offset1 = new Vector2(0.365f, 0.0f);
        this.offset2 = new Vector2(0.275f, 0.0f);        
    }

    // Update is called once per frame
    void Update()
    {
        this.CountDownAndShoot();

        //if enemy 7 then rotate it 360 degrees every 2 seconds
        if (this.gameObject.name.Equals("Enemy 7(Clone)"))
        {
            this.gameObject.transform.Rotate(0.0f, 0.0f, 3.0f);
        }
    }

    private void CountDownAndShoot()
    {
        this.shotCounter -= Time.deltaTime;

        if(this.shotCounter <= 0)
        {
            if (this.gameObject.name.Equals("Enemy 3(Clone)") && !this.hasCharged)
            {
                this.StartCoroutine(this.ChargeAndFire());
                return; //to avoid the ship to double shoot
            }
            else if(this.hasCharged)
                return; //to avoid the ship to double shoot

            this.Fire();
        }
    }

    private IEnumerator ChargeAndFire()
    {
        this.laserCharge.SetActive(true);
        this.playSFX.pitch = Random.Range(0.90f, 1.10f);
        this.playSFX.PlayOneShot(this.chargingLaserSFX, this.volumeSFX);
        this.hasCharged = true;

        yield return new WaitForSeconds(0.30f);
        
        this.laserCharge.SetActive(false);
        this.hasCharged = false;
        this.Fire();
    }

    private void Fire()
    {
        //first of all find always where the bow of the ship os located
        this.shipBow = new Vector2(this.gameObject.transform.position.x,
                                   this.gameObject.transform.position.y - this.enemySprite.bounds.extents.y);

        //secondly instantiate the laser from the ship's bow and play a shooting SFX
        if(this.gameObject.name.Equals("Enemy 2(Clone)"))
        {            
            Instantiate<GameObject>(this.shootingLaser, this.shipBow - this.offset1, Quaternion.identity);
            Instantiate<GameObject>(this.shootingLaser, this.shipBow + this.offset1, Quaternion.identity);
        }
        else if(this.gameObject.name.Equals("Enemy 5(Clone)"))
        {
            Instantiate<GameObject>(this.shootingLaser, this.shipBow - this.offset2, Quaternion.identity);
            Instantiate<GameObject>(this.shootingLaser, this.shipBow + this.offset2, Quaternion.identity);
        }
        else if (this.gameObject.name.Equals("Enemy 7(Clone)")) //rotating enemy with 4 shots on each direction
        {
            for(int i = 1; i < 5; i++)
            {                
                this.enemy7_shootingPos[i] = this.gameObject.GetComponentsInChildren<Transform>()[i];

                GameObject shot = Instantiate<GameObject>(this.shootingLaser, this.enemy7_shootingPos[i].position, Quaternion.identity);
                this.shotDirection = shot.GetComponent<EnemyLaser_4x>();
                this.shotDirection.SetShotDirection(this.enemy7_shootingPos[i].position - this.gameObject.transform.position); //sets each shot to move in its direction relative to the ships center
            }            
        }
        else if (this.gameObject.name.Equals("Enemy 8(Clone)"))
        {
            GameObject shot = Instantiate<GameObject>(this.shootingLaser, this.shipBow, Quaternion.identity);
            this.shotDirectionHoming = shot.GetComponent<HomingEnemyLaser>();
            this.shotDirectionHoming.FindPlayer(shot.transform.position);

        }
        else
        {
            Instantiate<GameObject>(this.shootingLaser, this.shipBow, Quaternion.identity);
        }
        this.playSFX.pitch = Random.Range(0.90f, 1.10f);
        this.playSFX.PlayOneShot(this.shootingLaserSFX, this.volumeSFX);
        

        //thirdly we assign a new value to the counter
        this.shotCounter = Random.Range(this.minTimeBetweenShots, this.maxTimeBetweenShots);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageDealer incomingProjectile = other.gameObject.GetComponent<DamageDealer>();

        if (incomingProjectile == null) { return; }

        ProcessHit(incomingProjectile);
    }

    private void ProcessHit(DamageDealer incomingProjectile)
    {
        this.health -= incomingProjectile.Damage;
        incomingProjectile.HasHitTarget();

        this.DestroyShip();
    }

    private void DestroyShip()
    {
        if (this.health <= 0)
        {
            GameObject explosion = Instantiate<GameObject>(this.explosionVfx, this.gameObject.transform.position, Quaternion.identity);

            this.UIManager.UpdateScore(this.enemyScore);
            GameObject.Destroy(this.gameObject);
            this.InstantiatePowerUp();

            AudioSource.PlayClipAtPoint(this.explosionSFX, Camera.main.transform.position, 0.3f);//this.volumeSFX);
            GameObject.Destroy(explosion, this.explosionDuration);
        }
    }

    private void InstantiatePowerUp()
    {
        int spawnChance = Random.Range(0, 100);

        if(spawnChance < 5)
            Instantiate<GameObject>(this.PowerUps[Random.Range(0, this.PowerUps.Length)], this.gameObject.transform.position, Quaternion.identity);
    }
}
