using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SingletonMusic : MonoBehaviour
{
    private int currentScene;
    private AudioSource backgroundMusic;
    public void PauseMusic()
    {
        this.backgroundMusic.Pause();
    }
    public void UnPauseMusic()
    {
        this.backgroundMusic.UnPause();
    }

    //state variables
    private bool isGamePaused = false;
    public void CheckIfGameIsPaused()
    {
        if (!this.isGamePaused)
            this.isGamePaused = true;
        else
            this.isGamePaused = false;
    }
    private bool isMusicSilenced = false;
    public bool IsMusicSilenced => this.isMusicSilenced;


    public IEnumerator FadeOutMusic()
    {
        float currentTime = 0.0f;

        while(currentTime < 5.0f)
        {
            currentTime += Time.deltaTime;

            this.backgroundMusic.volume = Mathf.Lerp(this.backgroundMusic.volume, 0.0f, currentTime / 5.0f);

            yield return null;
        }

        this.isMusicSilenced = true;
        yield break;        
    }

    public IEnumerator FadeUpMusic()
    {
        float currentTime = 0.0f;

        while (currentTime < 2.5f)
        {
            currentTime += Time.deltaTime;

            this.backgroundMusic.volume = Mathf.Lerp(this.backgroundMusic.volume, 0.5f, currentTime / 2.5f);

            yield return null;
        }

        this.isMusicSilenced = false;
        yield break;
    }

    //Singleton pattern
    private void Awake()
    {
        int sceneLoaderObjectCount = GameObject.FindObjectsOfType(this.GetType()).Length;

        if (sceneLoaderObjectCount > 1)
        {
            this.gameObject.SetActive(false); //SetActive activates inmediately, while Destroy() happens at the very end of the frame
            GameObject.Destroy(this.gameObject); //When loading a new scene, for a full frame the 2 copies of the script coexist and other gameobjects will link to the wrong version
        }
        else
            GameObject.DontDestroyOnLoad(this.gameObject);

        this.backgroundMusic = this.gameObject.GetComponent<AudioSource>();
        this.currentScene = SceneManager.GetActiveScene().buildIndex;
    }

    private void Update()
    {
        if (!this.isGamePaused)
        {
            if (SceneManager.GetActiveScene().buildIndex == 2 && this.backgroundMusic.isPlaying)
                this.backgroundMusic.Stop();
            else if (SceneManager.GetActiveScene().buildIndex == 1 && !this.backgroundMusic.isPlaying)
                this.backgroundMusic.Play();
            else if (this.currentScene == 0 && !this.backgroundMusic.isPlaying)
                this.backgroundMusic.Play();
        }   
    }    
}
