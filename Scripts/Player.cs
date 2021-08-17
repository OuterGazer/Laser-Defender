using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //Configuration Parameters(things we need to know before the game)
    [Header("Config Parameters")]
    [SerializeField] float movementSpeed = 10.0f;
    float xMin, xMax, yMin, yMax; //To represent the screen boundaries

    [SerializeField] float fireRate;

    private Coroutine fireCoroutine;
    private WaitForSeconds projectileFiringPeriod;
    private WaitForSeconds missileFiringPeriod = new WaitForSeconds(0.50f);

    [SerializeField] float playerHealth = 1000;
    public float PlayerHealth => this.playerHealth;
    public void SubtractPlayerLife(float inDamage)
    {
        this.playerHealth -= inDamage;
    }
    public void HealPlayer()
    {
        this.playerHealth = 2000;
    }


    //Cached Component References (references to other game objects or components of game objects)

    SpriteRenderer playerSprite;
    [Header("Cached References")]
    [SerializeField] GameObject shootingLaser;
    [SerializeField] GameObject shootingMissile;
    private Vector2 shipBow;
    private Vector2 leftLaserCannon;
    private Vector2 rightLaserCannon;
    private float offset = 0.28f;


    [SerializeField] GameObject explosionVfx;
    [SerializeField] float explosionDuration = 1.0f;

    private AudioSource playSFX;
    [Range(0.0f, 1.0f)] [SerializeField] float volumeSFX = 0.70f;
    [SerializeField] AudioClip shootingLaserSFX;
    [SerializeField] AudioClip shootingMissileSFX;
    [SerializeField] AudioClip explosionSFX;

    [SerializeField] SceneLoader targetObject;


    //State variables (to keep track of the variables that govern states)
    [Header("State Variables")]
    private bool canShoot = true;
    private bool hasMissileRecoiled = true;
    private bool canShootFast = false;    
    public bool CanShootFast
    { get { return this.canShootFast; }
      set { this.canShootFast = value; } }
    private bool hasFastCountStarted = false;
    private bool canShootMissile = false;
    public bool CanShootMissile
    {
        get { return this.canShootMissile; }
        set { this.canShootMissile = value; }
    }
    private bool hasMissileCountStarted = false;


    /*=======================================================================================================================================================================*/

    // Start is called before the first frame update
    void Start()
    {
        this.playerSprite = this.gameObject.GetComponent<SpriteRenderer>();

        this.projectileFiringPeriod = new WaitForSeconds(this.fireRate);

        this.playSFX = this.gameObject.GetComponent<AudioSource>();

        this.targetObject = GameObject.FindObjectOfType<SceneLoader>();

        this.SetUpMoveBoundaries();
    }

    private void SetUpMoveBoundaries()
    {
        Camera gameCamera = Camera.main;

        this.xMin = gameCamera.ViewportToWorldPoint(Vector3.zero).x + this.playerSprite.bounds.extents.x;
        this.xMax = gameCamera.ViewportToWorldPoint(Vector3.one).x - this.playerSprite.bounds.extents.x;

        this.yMin = gameCamera.ViewportToWorldPoint(Vector3.zero).y + this.playerSprite.bounds.extents.y;
        this.yMax = gameCamera.ViewportToWorldPoint(Vector3.one).y - this.playerSprite.bounds.extents.y;
    }

    /*=======================================================================================================================================================================*/

    // Update is called once per frame
    void Update()
    {
        this.Move();

        this.Fire();

        if (this.canShootFast && !this.hasFastCountStarted)
        {
            this.StartCoroutine(this.StopShootingFast());
        }

        if (this.canShootMissile && !this.hasMissileCountStarted)
        {
            this.StartCoroutine(this.StopShootingMissile());
        }
    }

    private IEnumerator StopShootingMissile()
    {
        this.hasMissileCountStarted = true;

        yield return new WaitForSeconds(15.0f);

        this.canShootMissile = false;
        this.hasMissileCountStarted = false;
    }

    private IEnumerator StopShootingFast()
    {
        this.hasFastCountStarted = true;

        yield return new WaitForSeconds(10.0f);

        this.canShootFast = false;
        this.projectileFiringPeriod = new WaitForSeconds(this.fireRate);
        this.hasFastCountStarted = false;
    }

    private void Fire()
    {
        if (Input.GetButton("Fire1") && this.canShoot)
        {
            this.canShoot = false;
            this.fireCoroutine = this.StartCoroutine(this.FireContinuously());
        }
    }

    private IEnumerator FireContinuously()
    {
        //first of all find always where the bow of the ship os located
        this.shipBow = new Vector2(this.gameObject.transform.position.x,
                                   this.gameObject.transform.position.y + this.playerSprite.bounds.extents.y);

        this.leftLaserCannon = this.shipBow - new Vector2(this.offset, 0.0f);
        this.rightLaserCannon = this.shipBow - new Vector2(-this.offset, 0.0f);

        //secondly instantiate the laser from the ship's bow
        this.playSFX.pitch = UnityEngine.Random.Range(0.90f, 1.10f);
        this.playSFX.PlayOneShot(this.shootingLaserSFX, this.volumeSFX);
        Instantiate<GameObject>(this.shootingLaser, this.leftLaserCannon, Quaternion.identity);
        Instantiate<GameObject>(this.shootingLaser, this.rightLaserCannon, Quaternion.identity);

        //Fire a missile if the power up is active
        if (this.canShootMissile && this.hasMissileRecoiled)
        {
            this.hasMissileRecoiled = false;
            this.StartCoroutine(ShootMissile());            
        }

        if (this.canShootFast && this.fireRate == 0.30f)
            this.projectileFiringPeriod = new WaitForSeconds(this.fireRate / 2);

        yield return this.projectileFiringPeriod;

        this.canShoot = true;
    }

    private IEnumerator ShootMissile()
    {
        this.playSFX.pitch = UnityEngine.Random.Range(0.90f, 1.10f);
        this.playSFX.PlayOneShot(this.shootingMissileSFX, this.volumeSFX);
        Instantiate<GameObject>(this.shootingMissile, this.shipBow, Quaternion.identity);

        yield return this.missileFiringPeriod;

        this.hasMissileRecoiled = true;
    }

    private void Move()
    {
        float deltaX = Input.GetAxis("Horizontal") * this.movementSpeed;
        float deltaY = Input.GetAxis("Vertical") * this.movementSpeed;

        Vector2 clampedDeltaMovement = Vector2.ClampMagnitude(new Vector2(deltaX, deltaY), this.movementSpeed) * Time.deltaTime;

        float xPos = Mathf.Clamp(this.gameObject.transform.position.x + clampedDeltaMovement.x, xMin, xMax);
        float yPos = Mathf.Clamp(this.gameObject.transform.position.y + clampedDeltaMovement.y, yMin, yMax);

        this.gameObject.transform.position = new Vector2(xPos, yPos);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageDealer incomingProjectile = other.gameObject.GetComponent<DamageDealer>();

        if (incomingProjectile == null) { return; }

        this.ProcessHit(incomingProjectile);

    }

    private void ProcessHit(DamageDealer incomingProjectile)
    {
        this.playerHealth -= incomingProjectile.Damage;
        incomingProjectile.HasHitTarget();

        this.DestroyShip();
    }

    private void DestroyShip()
    {
        if (this.playerHealth <= 0)
        {
            GameObject explosion = Instantiate<GameObject>(this.explosionVfx, this.gameObject.transform.position, Quaternion.identity);

            GameObject.Destroy(this.gameObject);
            //this.isDead = true;

            AudioSource.PlayClipAtPoint(this.explosionSFX, Camera.main.transform.position, this.volumeSFX);
            GameObject.Destroy(explosion, this.explosionDuration);

            this.targetObject.SendMessage("LoadGameOver");
        }
    }
}
