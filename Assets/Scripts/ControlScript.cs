using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlScript : MonoBehaviour
{
    public float gameSpeed = 1;
    public int fishCount = 0;

    public int fishCountToSpawnOnGameStart = 99;
    public GameObject fishObj;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < fishCountToSpawnOnGameStart; i++)
            Instantiate(fishObj, new Vector3(Random.Range(-20,20), Random.Range(-20,20), 0), Quaternion.identity); 
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = gameSpeed;
        fishCount = GameObject.FindGameObjectsWithTag("fish").Length;
    }
}
