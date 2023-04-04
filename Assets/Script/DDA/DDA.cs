using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Script.Networks
{
    public class DDA : MonoBehaviour
    {
        [SerializeField] GameObject players;

        [SerializeField] EnemySpawner enemySpawner;
        [SerializeField] GameObject enemyPrefab;
        Enemy enemyStats;

        [SerializeField] float spawnDelay = 3.0f;
        [SerializeField] int spawnCount = 2;
        [SerializeField] List<Transform> spawnPoints;

        [SerializeField] int numbersOfPlayer = 4;
        [SerializeField] int playerLevel = 1;
        [SerializeField] float playerATK = 1.0f;

        [SerializeField] float KKPM = 1.0f;
        [SerializeField]
        float KD = 1.0f;

        [SerializeField] int numbersOfEnemy = 5;



        // Start is called before the first frame update
        void Start()
        {
            enemyStats = enemyPrefab.GetComponent<Enemy>();
        }

        // Update is called once per frame
        void Update()
        {
            spawnCount = (int)Mathf.Round((float)((2 * (1 + (0.1 * numbersOfPlayer))) * KKPM * (1 + (0.1 * playerLevel))/KD));
            spawnDelay = (float)((3.0f *(1 + (0.01 * numbersOfEnemy))) / (KKPM *(1 + (0.1 * playerLevel))));


            enemyStats.health = (float)(enemyStats.health * (1 + (0.1 * numbersOfPlayer)) * KKPM * (1 + (0.1 * playerLevel))
 * (1 + (0.1 * playerATK)));

            enemySpawner.SpawnEnemy(enemyPrefab, spawnDelay, spawnCount, spawnPoints);
        }
    }
}
