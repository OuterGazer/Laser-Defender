using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    private UIManager targetObject;
    [SerializeField] int scorePoints = 1000;

    [SerializeField] AudioClip PU_SFX;

    [SerializeField] float movementSpeed = 15.0f;
    private Vector2 PuSpeed = Vector2.one;
    private Quaternion initialAngle;
    private Rigidbody2D PU_Rb;
    private Sprite PU_Sprite;
    private SpriteRenderer PU_SpriteRend;

    private Player player;

    private float spawnTime;
    private bool countdownHasStarted = false;
    private float blinkingTimer;
    private Vector3 initialScale;

    float xMin, xMax, yMin, yMax; //To represent the screen boundaries

    // Start is called before the first frame update
    void Start()
    {
        this.player = FindObjectOfType<Player>();

        this.PU_Rb = this.gameObject.GetComponent<Rigidbody2D>();

        this.PU_SpriteRend = this.gameObject.GetComponent<SpriteRenderer>();

        this.PU_Sprite = this.PU_SpriteRend.sprite;

        this.SetUpMoveBoundaries();

        this.SetUpInitialMoveDirection();

        this.spawnTime = Time.time;
        this.initialScale = this.gameObject.transform.localScale;

        this.targetObject = GameObject.FindObjectOfType<UIManager>();
    }

    private void SetUpInitialMoveDirection()
    {
        this.initialAngle = Quaternion.Euler(0.0f, 0.0f, Random.Range(10.0f, 170.0f));
        this.PuSpeed = this.initialAngle * this.PuSpeed;
        this.PuSpeed = this.PuSpeed.normalized * this.movementSpeed;
    }

    private void SetUpMoveBoundaries()
    {
        Camera gameCamera = Camera.main;

        this.xMin = gameCamera.ViewportToWorldPoint(Vector3.zero).x + this.PU_Sprite.bounds.extents.x;
        this.xMax = gameCamera.ViewportToWorldPoint(Vector3.one).x - this.PU_Sprite.bounds.extents.x;

        this.yMin = gameCamera.ViewportToWorldPoint(Vector3.zero).y + this.PU_Sprite.bounds.extents.y;
        this.yMax = gameCamera.ViewportToWorldPoint(Vector3.one).y - this.PU_Sprite.bounds.extents.y;
    }

    // Update is called once per frame
    private void Update()
    {
        //Marks the beginning of the countdown to destroy the power up if not picked
        if ((Time.time >= this.spawnTime + 6.0f) && !this.countdownHasStarted)
        {
            this.countdownHasStarted = true;
        }

        this.BlinkBeforeDisappear();
    }

    private void BlinkBeforeDisappear()
    {
        if (this.countdownHasStarted == true)
        {
            this.blinkingTimer += Time.deltaTime;

            if (this.blinkingTimer < 0.40f)
                this.gameObject.transform.localScale = Vector3.zero;
            else if (this.blinkingTimer < 0.80f)
                this.gameObject.transform.localScale = this.initialScale;
            else if (this.blinkingTimer < 1.20f)
                this.gameObject.transform.localScale = Vector3.zero;
            else if (this.blinkingTimer < 1.60f)
                this.gameObject.transform.localScale = this.initialScale;
            else if (this.blinkingTimer < 2.00f)
                this.gameObject.transform.localScale = Vector3.zero;
            else if (this.blinkingTimer < 2.40f)
                this.gameObject.transform.localScale = this.initialScale;
            else if (this.blinkingTimer < 2.80f)
                this.gameObject.transform.localScale = Vector3.zero;
            else if (this.blinkingTimer < 3.20f)
                this.gameObject.transform.localScale = this.initialScale;
            else if (this.blinkingTimer > 3.50f)
                GameObject.Destroy(this.gameObject);
        }
    }

    void FixedUpdate()
    {
        if((this.gameObject.transform.position.x <= this.xMin) ||
           (this.gameObject.transform.position.x >= this.xMax))
        {
            this.PuSpeed.Set(this.PuSpeed.x * -1, this.PuSpeed.y);
        }

        if ((this.gameObject.transform.position.y <= this.yMin) ||
           (this.gameObject.transform.position.y >= this.yMax))
        {
            this.PuSpeed.Set(this.PuSpeed.x, this.PuSpeed.y * -1);
        }

        this.PuSpeed = Vector2.ClampMagnitude(this.PuSpeed, this.movementSpeed);
        this.PU_Rb.MovePosition(this.PU_Rb.position + this.PuSpeed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            switch (this.gameObject.name)
            {
                case "Life Up PU(Clone)":
                    this.player.HealPlayer();
                    break;
                case "Fast Shooting PU(Clone)":
                    this.player.CanShootFast = true;
                    break;
                case "Missiles PU(Clone)":
                    this.player.CanShootMissile = true;
                    break;
            }

            AudioSource.PlayClipAtPoint(PU_SFX, Camera.main.transform.position, 0.50f);
            this.targetObject.UpdateScore(this.scorePoints);
            GameObject.Destroy(this.gameObject);
        }        
    }
}
