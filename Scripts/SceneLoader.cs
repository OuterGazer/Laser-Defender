using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    private UIManager UI_Manager;
    [SerializeField] TextMeshProUGUI instructionsText;
    [SerializeField] TextMeshProUGUI creditsText;

    [SerializeField] GameObject bossImage;
    [SerializeField] GameObject bossLifePoints;

    [SerializeField] GameObject pauseText;
    private bool isGamePaused = false;
    public bool IsGamePaused => this.isGamePaused;
    private SingletonMusic audioSource;
    private AudioSource bossMusic;

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            this.instructionsText.gameObject.SetActive(false);
            this.creditsText.gameObject.SetActive(false);
        }

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            this.UI_Manager = GameObject.FindObjectOfType<UIManager>();
            this.UI_Manager.SceneUpdated = true;
        }

        this.SetBossInfo();
        this.pauseText.SetActive(false);

        this.audioSource = GameObject.FindObjectOfType<SingletonMusic>();
    }

    public void SetBossInfo()
    {
        if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            if (!this.bossLifePoints.activeSelf)
            {
                this.bossMusic = GameObject.FindObjectOfType<BossBehaviour>().gameObject.GetComponent<AudioSource>();
                this.bossImage.SetActive(true);
                this.bossLifePoints.SetActive(true);
            }
            else
            {
                this.bossImage.SetActive(false);
                this.bossLifePoints.SetActive(false);
            }
        }        
    }

    private void Update()
    {
        if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            if (Input.GetKeyDown(KeyCode.P) && !this.isGamePaused)
            {
                this.isGamePaused = true;
                this.audioSource.CheckIfGameIsPaused();
                this.audioSource.PauseMusic();

                if (this.bossMusic != null)
                    this.bossMusic.Pause();

                this.pauseText.SetActive(true);
                Time.timeScale = 0;
            }
            else if (Input.GetKeyDown(KeyCode.P) && this.isGamePaused)
            {
                this.isGamePaused = false;
                this.audioSource.UnPauseMusic();

                if (this.bossMusic != null)
                    this.bossMusic.UnPause();

                this.audioSource.CheckIfGameIsPaused();
                this.pauseText.SetActive(false);
                Time.timeScale = 1;
            }

            if (this.isGamePaused)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    SceneManager.LoadScene(0);
                    this.isGamePaused = false;
                    this.audioSource.UnPauseMusic();
                    this.audioSource.CheckIfGameIsPaused();
                    this.pauseText.SetActive(false);
                    Time.timeScale = 1;
                }                    
            }
        }
    }

    public void LoadGameOver()
    {
        this.StartCoroutine(DelayGameOver());
    }

    private IEnumerator DelayGameOver()
    {
        yield return new WaitForSeconds(0.75f);

        SceneManager.LoadScene("Game Over");
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("Game Scene");
    }

    public void LoadStartMenu()
    {
        SceneManager.LoadScene("Start Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowAndHideInstructionsText()
    {
        if (this.creditsText.gameObject.activeSelf)
            this.creditsText.gameObject.SetActive(false);

        if (!this.instructionsText.gameObject.activeSelf)
            this.instructionsText.gameObject.SetActive(true);
        else
            this.instructionsText.gameObject.SetActive(false);
    }

    public void ShowAndHideCreditsText()
    {
        if (this.instructionsText.gameObject.activeSelf)
            this.instructionsText.gameObject.SetActive(false);

        if (!this.creditsText.gameObject.activeSelf)
            this.creditsText.gameObject.SetActive(true);
        else
            this.creditsText.gameObject.SetActive(false);
    }
}
