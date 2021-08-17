using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class BossBehaviour : MonoBehaviour
{
    private bool hasOminousEntranceFinished = false;
    private bool isShieldActive = true;
    private bool isBossAttacking = false;
    private bool isFirstAttackActive = false;
    private bool isSecondAttackActive = false;
    private bool isThirdAttackActive = false;
    private int attackNumber = 0;

    private UIManager targetObject;
    
    [Header("Boss stats")]
    [SerializeField] int lifePoints = 1_000;
    public int LifePoints => this.lifePoints;
    [SerializeField] int scorePoints = 1_000;
    [SerializeField] float moveSpeed = 5.0f;
    

    [Header("Boss Cached References")]
    [SerializeField] GameObject bossShield;
    [SerializeField] GameObject homingMissile;
    [SerializeField] GameObject chargedLaser;
    [SerializeField] GameObject laserChargeLeft;
    [SerializeField] GameObject laserChargeRight;
    [SerializeField] GameObject greenBlaster;
    [SerializeField] GameObject redBlaster;
    [SerializeField] WaveConfig[] enemyWaves;
    private bool hasCharged = false;
    private Sprite bossSprite;
    private Rigidbody2D bossRb;
    private AudioSource playSFX;
    private SceneLoader toCheckForPause;
    private EnemySpawner spawnMinions;
    

    [Header("Miscelaneous")]
    [Range(0.0f, 1.0f)][SerializeField] float volumeSFX = 0.50f;
    [SerializeField] AudioClip chargingLaserSFX;
    [SerializeField] AudioClip shootingLaserSFX;
    [SerializeField] AudioClip shootingMissileSFX;
    [SerializeField] AudioClip greenBlasterSFX;
    [SerializeField] GameObject explosionVfx;
    [SerializeField] float explosionDuration = 1.0f;
    [SerializeField] AudioClip explosionSFX;
    [SerializeField] AudioClip shieldUpSFX;
    [SerializeField] AudioClip shieldDownSFX;

    [SerializeField] GameObject[] PowerUps;

    //positional and state variables for the different attacks
    private Vector2 shipBow;
    private Vector2 originPoint = new Vector2(0.0f, 6.50f);
    private Vector2 leftLimit = new Vector2(-3.50f, 6.50f);
    private Vector2 rightLimit = new Vector2(3.50f, 6.50f);
    private Vector2 targetPos;
    private int bounceCounter = 0;
    private float shotCounterLaser = 0.0f;
    private float shotCounterMissile = 0.0f;
    private float nextAngle = 6.0f; //for the second attack. 6 degrees per frame gives us 1 full rotation per second

    // Start is called before the first frame update
    void Start()
    {
        this.bossSprite = this.gameObject.GetComponent<SpriteRenderer>().sprite;
        this.playSFX = this.gameObject.GetComponent<AudioSource>();
        this.bossRb = this.gameObject.GetComponent<Rigidbody2D>();

        this.toCheckForPause = GameObject.FindObjectOfType<SceneLoader>();
        this.spawnMinions = GameObject.FindObjectOfType<EnemySpawner>();
        this.targetObject = GameObject.FindObjectOfType<UIManager>();
        this.targetObject.SendMessage("BossHasSpawned");

        this.laserChargeLeft.SetActive(false);
        this.laserChargeRight.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!this.toCheckForPause.IsGamePaused)
        {
            if (!this.hasOminousEntranceFinished)
            {
                this.BossArrival();
            }

            if (this.hasOminousEntranceFinished && !this.isBossAttacking)
            {
                this.DecideAttack();
            }

            if (this.isFirstAttackActive)
            {
                this.LeftRightMoveAndShoot();
            }

            if (this.isSecondAttackActive)
            {
                this.SpinAndShoot();
            }

            if (this.isThirdAttackActive)
            {
                this.CallMinionsAndShoot();
            }
        }
    }

    private void DecideAttack()
    {
        //we always make first the 3 attacks in a row, then decide the attacks randomly
        if (this.attackNumber > 2)
            this.attackNumber = Random.Range(0, 3);

        switch (this.attackNumber)
        {
            case 0:
                this.isFirstAttackActive = true;
                this.DeactivateShield();
                break;
            case 1:
                this.isSecondAttackActive = true;
                break;
            case 2:
                this.isThirdAttackActive = true;
                this.DeactivateShield();
                break;
        }

        this.isBossAttacking = true;
        this.attackNumber++;
    }

    private void CallMinionsAndShoot()
    {
        this.MoveThirdtAttack();

        this.CallMinions();

        this.FireClusters();
    }

    private void FireClusters()
    {
        this.shotCounterLaser -= Time.deltaTime;

        if(this.shotCounterLaser <= 0)
        {
            Instantiate<GameObject>(this.redBlaster, this.laserChargeLeft.transform.position, Quaternion.identity);
            Instantiate<GameObject>(this.redBlaster, this.laserChargeRight.transform.position, Quaternion.identity);
            this.playSFX.PlayOneShot(this.greenBlasterSFX, this.volumeSFX);

            this.shotCounterLaser = 1.0f;
        }
    }

    private void CallMinions()
    {
        this.shotCounterMissile -= Time.deltaTime;

        if (this.shotCounterMissile <= 0)
        {
            this.StartCoroutine(this.spawnMinions.SpawnAllEnemiesInWave(this.enemyWaves[Random.Range(0, this.enemyWaves.Length)]));
            this.shotCounterMissile = 6.0f; //this will spawn exactly 3 waves
        }
    }

    private void MoveThirdtAttack()
    {
        //if the boss has bounced 8 times, finish the attack when reaching the middle of the screen
        //else would be the very beginning of the attack, we set the first movement position to the left of the screen
        if (this.targetPos == this.originPoint && this.bounceCounter > 8)
        {
            if ((Vector2)this.gameObject.transform.position == this.originPoint)
            {
                this.bounceCounter = 0;
                this.shotCounterMissile = 0.0f;
                this.shotCounterLaser = 0.0f;
                this.isThirdAttackActive = false;
                this.isBossAttacking = false;
                this.ActivateShield();
                return; //else the code below would run at least once before the end of the frame
            }
        }
        else if (this.bounceCounter < 1)
        {
            this.targetPos = this.rightLimit;
        }

        //Always keep the boss moving towards the next target position
        this.gameObject.transform.position = Vector2.MoveTowards(this.gameObject.transform.position, this.targetPos, this.moveSpeed * Time.deltaTime);

        //once arrived at target position, change it to the other side of the screen
        //when bounced 8 times, change it to the middle to finish the attack
        if (Vector2.Distance(this.gameObject.transform.position, this.targetPos) < Vector2.kEpsilon)
        {
            if (this.targetPos == this.rightLimit)
            {
                this.targetPos = this.leftLimit;
            }
            else if (this.targetPos == this.leftLimit)
            {
                this.targetPos = this.rightLimit;
            }

            if (this.targetPos != this.originPoint)
                this.bounceCounter++;

            if (this.bounceCounter > 8)
                this.targetPos = this.originPoint;
        }
    }

    private void SpinAndShoot()
    {
        //first move to attack spot
        if ((Vector2.Distance(this.gameObject.transform.position, Vector2.zero) > Vector2.kEpsilon) &&
            this.nextAngle < 1080.0f)
        {
            this.gameObject.transform.position = Vector2.MoveTowards(this.gameObject.transform.position, Vector2.zero, this.moveSpeed * Time.deltaTime);
        }
        else
        {
            //once in attack spot deactivate shield to shoot
            if (this.isShieldActive && this.nextAngle < 1080.0f)
            {
                this.isShieldActive = false;
                this.DeactivateShield();
            }

            //rotate 3 times around axis and shoot
            if (this.nextAngle < 1080.0f)
            {
                this.bossRb.MoveRotation(this.nextAngle);
                this.nextAngle += 6.0f;

                //FIRE!
                this.shotCounterLaser -= Time.deltaTime;

                if (this.shotCounterLaser <= 0 && !this.isShieldActive)
                {
                    GameObject shotLeft = Instantiate<GameObject>(this.greenBlaster, this.laserChargeLeft.transform.position, Quaternion.identity);
                    GameObject shotRight = Instantiate<GameObject>(this.greenBlaster, this.laserChargeRight.transform.position, Quaternion.identity);
                    shotLeft.GetComponent<EnemyLaser_4x>().SetShotDirection(this.laserChargeLeft.transform.position - this.gameObject.transform.position);
                    shotRight.GetComponent<EnemyLaser_4x>().SetShotDirection(this.laserChargeLeft.transform.position - this.gameObject.transform.position);
                    this.playSFX.PlayOneShot(this.greenBlasterSFX, this.volumeSFX);

                    this.shotCounterLaser = 0.06f;
                }
            }

            //once we rotated 3 times activate shield and move towards origin point for another attack
            if (this.nextAngle >= 1080.0f)
            {
                //just in case the rotation is off by some amount, correct it
                if (this.bossRb.rotation != 0.0f)
                    this.bossRb.MoveRotation(0.0f);

                if (!this.isShieldActive)
                {
                    this.ActivateShield();
                    this.isShieldActive = true;
                }

                this.gameObject.transform.position = Vector2.MoveTowards(this.gameObject.transform.position, this.originPoint, this.moveSpeed * Time.deltaTime);

                if (Vector2.Distance(this.gameObject.transform.position, this.originPoint) < Vector2.kEpsilon)
                {
                    //return necessary values to standard once we reached origin point
                    this.nextAngle = 6.0f;
                    this.shotCounterLaser = 0.00f;
                    this.isBossAttacking = false;
                    this.isSecondAttackActive = false;
                }
            }
        }
    }

    private void LeftRightMoveAndShoot()
    {
        this.shotCounterLaser -= Time.deltaTime;
        this.shotCounterMissile -= Time.deltaTime;

        this.MoveFirstAttack();

        if(this.shotCounterLaser <= 0)
        {
            if (!this.hasCharged)
            {
                this.StartCoroutine(this.ChargeAndFire());
            }
        }
        
        if(this.shotCounterMissile <= 0)
            this.FireMissile();
    }

    private void FireMissile()
    {
        this.shipBow = new Vector2(this.gameObject.transform.position.x,
                                   this.gameObject.transform.position.y - this.bossSprite.bounds.extents.y);

        Instantiate<GameObject>(this.homingMissile, this.shipBow, Quaternion.identity);

        this.shotCounterMissile = 1.80f;
    }

    private IEnumerator ChargeAndFire()
    {  
        //this is purely to give time to play the charging animation
        this.laserChargeLeft.SetActive(true);
        this.laserChargeRight.SetActive(true);
        this.playSFX.PlayOneShot(this.chargingLaserSFX, this.volumeSFX);
        this.hasCharged = true;

        yield return new WaitForSeconds(0.30f);

        this.laserChargeLeft.SetActive(false);
        this.laserChargeRight.SetActive(false);
        this.hasCharged = false;
        //now we fire the actual lasers
        this.Fire();
    }

    private void Fire()
    {
        Instantiate<GameObject>(this.chargedLaser, this.laserChargeLeft.transform.position, Quaternion.identity);
        Instantiate<GameObject>(this.chargedLaser, this.laserChargeRight.transform.position, Quaternion.identity);
        this.playSFX.PlayOneShot(this.shootingLaserSFX, this.volumeSFX);

        this.shotCounterLaser = 0.60f;
    }

    private void MoveFirstAttack()
    {
        //if the boss has bounced 4 times, finish the attack when reaching the middle of the screen
        //else would be the very beginning of the attack, we set the first movement position to the left of the screen
        if (this.targetPos == this.originPoint && this.bounceCounter > 3)
        {
            if ((Vector2)this.gameObject.transform.position == this.originPoint)
            {
                this.bounceCounter = 0;
                this.isFirstAttackActive = false;
                this.isBossAttacking = false;
                this.ActivateShield();
                return; //else the code below would run at least once before the end of the frame
            }
        }
        else if (this.bounceCounter < 1)
        {
            this.targetPos = this.leftLimit;
        }

        //Always keep the boss moving towards the next target position
        this.gameObject.transform.position = Vector2.MoveTowards(this.gameObject.transform.position, this.targetPos, this.moveSpeed * Time.deltaTime);

        //once arrived at target position, change it to the other side of the screen
        //when bounced 4 times, change it to the middle to finish the attack
        if (Vector2.Distance(this.gameObject.transform.position, this.targetPos) < Vector2.kEpsilon)
        {
            if (this.targetPos == this.leftLimit)
            {
                this.targetPos = this.rightLimit;
            }
            else if(this.targetPos == this.rightLimit)
            {
                this.targetPos = this.leftLimit;
            }

            if(this.targetPos != this.originPoint)
                this.bounceCounter++;

            if (this.bounceCounter > 3)
                this.targetPos = this.originPoint;
        }
    }

    private void ActivateShield()
    {
        this.bossShield.SetActive(true);
        this.playSFX.PlayOneShot(this.shieldUpSFX, this.volumeSFX);
    }

    private void DeactivateShield()
    {
        this.bossShield.SetActive(false);
        this.playSFX.PlayOneShot(this.shieldDownSFX, this.volumeSFX);
    }

    private void BossArrival()
    {
        this.gameObject.transform.position = Vector2.MoveTowards(this.gameObject.transform.position, this.originPoint, 0.80f * Time.deltaTime);

        if ((Vector2)this.gameObject.transform.position == this.originPoint)
        {
            this.hasOminousEntranceFinished = true;
            this.targetPos = this.gameObject.transform.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!this.bossShield.activeSelf)
        {
            DamageDealer incomingProjectile = other.gameObject.GetComponent<DamageDealer>();

            if (incomingProjectile == null) { return; }

            ProcessHit(incomingProjectile);
        }
    }

    private void ProcessHit(DamageDealer incomingProjectile)
    {
        this.lifePoints -= incomingProjectile.Damage;
        incomingProjectile.HasHitTarget();

        this.DestroyShip();
    }

    private void DestroyShip()
    {
        if (this.lifePoints <= 0)
        {
            GameObject explosion = Instantiate<GameObject>(this.explosionVfx, this.gameObject.transform.position, Quaternion.identity);

            this.targetObject.UpdateScore(this.scorePoints);
            GameObject.Destroy(this.gameObject);
            this.InstantiatePowerUp();

            AudioSource.PlayClipAtPoint(this.explosionSFX, Camera.main.transform.position, 0.3f);//this.volumeSFX);
            GameObject.Destroy(explosion, this.explosionDuration);
        }
    }

    private void InstantiatePowerUp()
    {
        for(int i = 0; i < this.PowerUps.Length; i++)
        {
            Instantiate<GameObject>(this.PowerUps[i], this.gameObject.transform.position, Quaternion.identity);
        }        
    }

    private void OnDestroy()
    {
        SceneLoader sceneLoader = GameObject.FindObjectOfType<SceneLoader>();

        if(sceneLoader != null)
            sceneLoader.SetBossInfo();

        this.targetObject.HasBossSpawned = false;

        EnemySpawner enemySpawner = GameObject.FindObjectOfType<EnemySpawner>();

        if(enemySpawner != null)
            enemySpawner.BossIsDead();
    }
}
