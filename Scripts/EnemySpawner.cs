using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] List<WaveConfig> waveConfigs;
    [SerializeField] int numberOfWaves = 1;
    [SerializeField] bool isLoopingThroughWaves = false;
    private bool hasBossFightStarted = false;
    [SerializeField] int startingWave = 0;
    [SerializeField] GameObject bossEnemy;
    private GameObject boss;

    private GameObject chosenEnemy;
    private int pathNumber;
    private int enemyCount;
    private float enemySpawnRate;
    private float enemySpeed;

    private SingletonMusic audioSource;

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        this.audioSource = GameObject.FindObjectOfType<SingletonMusic>();
        
        //if the boss kills the player, we need to up the music again if they retry
        if(this.audioSource.IsMusicSilenced)
            this.StartCoroutine(this.audioSource.FadeUpMusic());

        do
        {
            yield return this.StartCoroutine(SpawnAllWaves());
        } while (this.isLoopingThroughWaves);
    }

    private void Update()
    {
        if (!this.isLoopingThroughWaves && !this.hasBossFightStarted)
        {
            this.hasBossFightStarted = true;

            this.StartCoroutine(this.StartBossFight());
        }        
    }

    private IEnumerator StartBossFight()
    {
        this.StartCoroutine(this.audioSource.FadeOutMusic());

        yield return new WaitUntil(() => this.audioSource.IsMusicSilenced == true);

        this.boss = Instantiate<GameObject>(this.bossEnemy, new Vector3(0.0f, 13.50f, 0.0f), Quaternion.identity);
    }

    public void BossIsDead()
    {
        this.StartCoroutine(this.StartOver());
    }

    public IEnumerator StartOver()
    {
        this.hasBossFightStarted = false;
        this.isLoopingThroughWaves = true;

        this.StartCoroutine(this.audioSource.FadeUpMusic());

        yield return new WaitUntil(() => this.audioSource.IsMusicSilenced == false);        

        do
        {
            yield return this.StartCoroutine(SpawnAllWaves());
        } while (this.isLoopingThroughWaves);
    }

    private IEnumerator SpawnAllWaves()
    {       
        for(int j = 0; j < this.numberOfWaves; j++)
        {
            for (int i = this.startingWave; i < 8; i++)//this.waveConfigs.Count; i++)
            {
                WaveConfig currentWave;

                switch (i)
                {
                    case 0:
                    case 1:
                    case 5:
                        currentWave = this.waveConfigs[0];
                        break;
                    case 2:
                    case 4:
                    case 7:
                        currentWave = this.waveConfigs[1];
                        break;
                    case 3:
                    case 6:
                        currentWave = this.waveConfigs[2];
                        break;
                    default:
                        currentWave = null;
                        break;
                }

                //WaveConfig currentWave = this.waveConfigs[i];

                yield return this.StartCoroutine(SpawnAllEnemiesInWave(currentWave));
                //yield return new WaitUntil(() => currentWave.EnemyCount > 3);
            }
        }        

        this.isLoopingThroughWaves = false; //comment out his line to make final boss appear
    }

    public IEnumerator SpawnAllEnemiesInWave(WaveConfig inCurrentWave) //it's public so the boss can use it to spawn a wave of enemies at certain points of the fight
    {
        //First we choose the enemy type for the wave
        this.chosenEnemy = inCurrentWave.EnemyPrefab[Random.Range(0, inCurrentWave.EnemyPrefab.Length)];

        //second we choose the pregenerated path
        this.pathNumber = Random.Range(0, inCurrentWave.PathNumberCount);

        //third we choose how many enemies will appear
        //fourth we choose the spawn rate of the enemies in the wave
        //at last we choose the speed of the wave
        if (inCurrentWave.name.Equals("Wave Easy"))
        {
            this.enemyCount = Random.Range(4, 7);
            this.enemySpawnRate = Random.Range(0.5f, 0.7f);
            this.enemySpeed = Random.Range(6.0f, 8.0f);
        }            
        else if(inCurrentWave.name.Equals("Wave Medium"))
        {
            this.enemyCount = Random.Range(2, 5);
            this.enemySpawnRate = Random.Range(1.0f, 1.5f);
            this.enemySpeed = Random.Range(3.0f, 5.0f);
        }            
        else if (inCurrentWave.name.Equals("Wave Hard"))
        {
            this.enemyCount = Random.Range(6, 9);
            this.enemySpawnRate = Random.Range(0.3f, 0.5f);
            this.enemySpeed = Random.Range(9.0f, 11.0f);
        }            

        for (int i = 0; i < this.enemyCount; i++)
        {            
            List<Transform> chosenPath = inCurrentWave.GetPathWaypoints(this.pathNumber);

            GameObject enemy = GameObject.Instantiate<GameObject>(this.chosenEnemy, chosenPath[0].position, Quaternion.identity);

            enemy.GetComponent<EnemyPathing>().SetWaveInfo(inCurrentWave, this.pathNumber, this.enemySpeed);

            yield return new WaitForSeconds(this.enemySpawnRate);
        }        
    }
}
