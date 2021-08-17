using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

public class UIManager : MonoBehaviour
{
    private TextMeshProUGUI scoreText;
    private int score = 0;

    private Player player;
    private TextMeshProUGUI playerHealth;

    private BossBehaviour boss;
    private TextMeshProUGUI bossHealth;
    private bool hasBossSpawned;
    public bool HasBossSpawned
    {
        get { return this.hasBossSpawned; }
        set { this.hasBossSpawned = value; }
    }

    private bool sceneUpdated = false;
    public bool SceneUpdated
    {
        get { return this.sceneUpdated; }
        set { this.sceneUpdated = value; }
    }

    private void Awake()
    {
        int UI_Count = GameObject.FindObjectsOfType<UIManager>().Length;

        if(UI_Count > 1)
        {
            this.gameObject.SetActive(false);
            GameObject.Destroy(this.gameObject);
        }
        else
        {
            GameObject.DontDestroyOnLoad(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        this.player = GameObject.FindObjectOfType<Player>();
        this.playerHealth = TextMeshProUGUI.FindObjectsOfType<TextMeshProUGUI>().ToList<TextMeshProUGUI>().Find(x => x.name == "Player Life Text");

        this.scoreText = TextMeshProUGUI.FindObjectsOfType<TextMeshProUGUI>().ToList<TextMeshProUGUI>().Find(x => x.CompareTag("Score Text"));
        this.scoreText.text = score.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        this.DisplayUIInfo();

        this.FindAndLinkScoreText();
    }

    private void DisplayUIInfo()
    {
        this.scoreText.text = score.ToString();

        if (this.player.PlayerHealth > 0)
            this.playerHealth.text = this.player.PlayerHealth.ToString();
        else
            this.playerHealth.text = "0";

        if (this.hasBossSpawned)
        {
            if (this.boss.LifePoints > 0)
                this.bossHealth.text = this.boss.LifePoints.ToString();
            else
                this.bossHealth.text = "0";
        }
    }

    private void FindAndLinkScoreText()
    {
        if (!this.sceneUpdated)
        {
            if(SceneManager.GetActiveScene().buildIndex == 2)
            {
                this.scoreText = TextMeshProUGUI.FindObjectsOfType<TextMeshProUGUI>().ToList<TextMeshProUGUI>().Find(x => x.CompareTag("Score Text"));
                this.sceneUpdated = true;
            }            
        }
        else if (this.sceneUpdated)
        {
            if(SceneManager.GetActiveScene().buildIndex == 1)
            {
                this.ResetScore();
                this.scoreText = TextMeshProUGUI.FindObjectsOfType<TextMeshProUGUI>().ToList<TextMeshProUGUI>().Find(x => x.CompareTag("Score Text"));
            }            
        }
    }

    public void UpdateScore(int inEnemyScore)
    {
        this.score += inEnemyScore;
    }

    public void ResetScore()
    {
        this.score = 0;
        this.sceneUpdated = false;
        this.player = GameObject.FindObjectOfType<Player>();
        this.playerHealth = TextMeshProUGUI.FindObjectsOfType<TextMeshProUGUI>().ToList<TextMeshProUGUI>().Find(x => x.name == "Player Life Text");
    }

    private void BossHasSpawned()
    {
        this.hasBossSpawned = true;

        SceneLoader sceneLoader = GameObject.FindObjectOfType<SceneLoader>();
        sceneLoader.SetBossInfo();

        this.boss = GameObject.FindObjectOfType<BossBehaviour>();
        this.bossHealth = TextMeshProUGUI.FindObjectsOfType<TextMeshProUGUI>().ToList<TextMeshProUGUI>().Find(x => x.name == "Boss Life Text");
    }
}
