using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class fishReturnRaytraced : MonoBehaviour
{
    public RayPerceptionSensorComponent2D rays;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PrintReturnEverySecond());
    }

    // Update is called once per frame
    void Update()
    {
        //print(rays.GetRayPerceptionInput());
        //rays.GetRayPerceptionInput().DetectableTags;
    }

    IEnumerator PrintReturnEverySecond()
    {
        print(rays.GetRayPerceptionInput().OutputSize());
        print(rays.GetRayPerceptionInput().RayExtents(0));
        /*
        foreach(string str in rays.GetRayPerceptionInput().OutputSize())  
        {
            print(str);
        }
        */
        yield return new WaitForSeconds(1);
        StartCoroutine(PrintReturnEverySecond());
    }
}
