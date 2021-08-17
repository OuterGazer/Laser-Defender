using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy Wave Config")]
public class WaveConfig : ScriptableObject
{
    [SerializeField] GameObject[] enemyPrefab;
    [SerializeField] GameObject[] pathPrefab;

    public GameObject[] EnemyPrefab { get { return this.enemyPrefab; } }
    public int PathNumberCount { get { return this.pathPrefab.Length; } }

    public List<Transform> GetPathWaypoints(int inPathPrefab)
    {
        List<Transform> waveWaypoints = new List<Transform>();

        foreach(Transform item in this.pathPrefab[inPathPrefab].transform) //Can also be foreach(Transform item in this.pathPrefab.GetComponentsInChildren<Transform>())) but also gets the parents transform
        {
            waveWaypoints.Add(item);
        }

        return waveWaypoints;
    }
}
