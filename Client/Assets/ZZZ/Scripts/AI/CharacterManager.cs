
using System.Collections.Generic;
using UnityEngine;
using HuHu;

public class CharacterManager : Singleton<CharacterManager> 
{
    //找到所有的玩家-GameObject.Tag

    [SerializeField] private GameObject[] players;
    [SerializeField] public List<Transform> playerTransform;

    [SerializeField] public List<GameObject> allEnemies;
    [SerializeField] public List<GameObject> activeEnemies;
    private GameObject[] enemies;
    protected override void Awake()
    {
        base.Awake();
        FindAllPlayer();
        FindAllEnemies();
    }

 

    private void Start()
    {
        InitEnemy();
    }

    private void InitEnemy()
    {
       for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i].activeSelf)
            {
                activeEnemies.Add(allEnemies[i]);
            }   
        }
    }

    public void AddEnemy(GameObject enemy)
    {
        if (!allEnemies.Contains(enemy))
        {
            allEnemies.Add(enemy);
        }
      
    }
    public void RemoveEnemyFromActive(GameObject enemy)
    {

        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }

    }
   

    private void FindAllPlayer()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++) 
        {
            playerTransform.Add(players[i].transform);
        
         }
    }
    private void FindAllEnemies()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            allEnemies.Add(enemies[i]);
        }
    }
}
