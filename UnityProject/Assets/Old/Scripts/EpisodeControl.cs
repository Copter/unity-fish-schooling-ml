using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class EpisodeControl : Agent
{
    public GameObject foodObj;
    public int foodCountToSpawn = 10;
    public float gameSpeed = 1;
    public int fishCount = 0;

    public int fishCountToSpawnOnGameStart = 99;
    public GameObject fishObj;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < fishCountToSpawnOnGameStart; i++)
            Instantiate(fishObj, new Vector3(Random.Range(-20, 20), Random.Range(-20, 20), 0), Quaternion.identity);
    }

    public override void OnEpisodeBegin()
    {
        // remove old food sources from previous generation
        if(GameObject.FindGameObjectsWithTag("food").Length > 0)
        {
            for(int i = 0; i < GameObject.FindGameObjectsWithTag("food").Length; i++)
            {
                Destroy(GameObject.FindGameObjectsWithTag("food")[i]);
            }
        }
        // generate new food sources with new random positions
        for(int i = 0; i < 10; i++)
        {
            Instantiate(foodObj, randomFoodArea(), Quaternion.identity);
        }
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {

    }

    public /*override*/ void OnActionReceived(float[] vectorAction)
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.FindGameObjectsWithTag("food").Length < 10)
        {
            // make sure food sources are always amount to 10 sources at any point in time
            for (int i = 0; i < 10 - GameObject.FindGameObjectsWithTag("food").Length; i++)
            {
                Instantiate(foodObj, randomFoodArea(), Quaternion.identity);
            }
        }

        fishCount = GameObject.FindGameObjectsWithTag("fish").Length;

    }


    public Vector3 randomFoodArea()
    {
        int randomdir = Random.Range(0, 4);
        if (randomdir == 0)
        {
            return new Vector3(Random.Range(-65f, 65f), Random.Range(25f, 35f), 0); //UP
        }
        else if (randomdir == 1)
        {
            return new Vector3(Random.Range(-65f, 65f), Random.Range(-35f, -25f), 0); //DOWN
        }
        else if (randomdir == 2)
        {
            return new Vector3(Random.Range(-65f, -25f), Random.Range(-25f, 25f), 0); //LEFT
        }
        else
        {
            return new Vector3(Random.Range(25f, 65f), Random.Range(-25f, 25f), 0); //RIGHT
        }
    }
}
