using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class foodZone : MonoBehaviour
{
    public bool randomPositionOnGameStart = true;
    public float foodAmount = 500f;

    // Start is called before the first frame update
    void Start()
    {
        if(randomPositionOnGameStart) {
            transform.position = outerAreaRandom();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(foodAmount <= 0) {
            transform.position = outerAreaRandom();
            foodAmount = 500f;
        }
        
    }

    public Vector3 outerAreaRandom()
    {
        int randomdir = Random.Range(0,4);
        if(randomdir == 0)
        {
            return new Vector3(Random.Range(-65f,65f), Random.Range(25f,35f), 0); //UP
        }
        else if(randomdir == 1)
        {
            return new Vector3(Random.Range(-65f,65f), Random.Range(-35f,-25f), 0); //DOWN
        }
        else if(randomdir == 2)
        {
            return new Vector3(Random.Range(-65f,-25f), Random.Range(-25f,25f), 0); //LEFT
        }
        else
        {
            return new Vector3(Random.Range(25f,65f), Random.Range(-25f,25f), 0); //RIGHT
        }
    } 
}
