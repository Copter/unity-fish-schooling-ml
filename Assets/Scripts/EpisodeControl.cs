using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class EpisodeControl : Agent
{
    public GameObject foodObj;
    // Start is called before the first frame update
    void Start()
    {
    }

    public override void OnEpisodeBegin()
    {
        if(GameObject.FindGameObjectsWithTag("food").Length > 0)
        {
            for(int i = 0; i < GameObject.FindGameObjectsWithTag("food").Length; i++)
            {
                Destroy(GameObject.FindGameObjectsWithTag("food")[i]);
            }
        }
        Instantiate(foodObj, randomFoodArea(), Quaternion.identity);
    }

    public override void CollectObservations(VectorSensor sensor)
    {

    }

    public override void OnActionReceived(float[] vectorAction)
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.FindGameObjectsWithTag("food").Length <= 0)
        {
            Instantiate(foodObj, randomFoodArea(), Quaternion.identity);
        }
            
    }


    public Vector3 randomFoodArea()
    {
        int randomdir = Random.Range(0, 4);
        if (randomdir == 0)
        {
            return new Vector3(Random.Range(-35f, 35f), Random.Range(25f, 35f), 0); //UP
        }
        else if (randomdir == 1)
        {
            return new Vector3(Random.Range(-35f, 35f), Random.Range(-35f, -25f), 0); //DOWN
        }
        else if (randomdir == 2)
        {
            return new Vector3(Random.Range(-35f, -25f), Random.Range(-25f, 25f), 0); //LEFT
        }
        else
        {
            return new Vector3(Random.Range(25f, 35f), Random.Range(-25f, 25f), 0); //RIGHT
        }
    }
}
