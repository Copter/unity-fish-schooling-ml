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
        //StartCoroutine(PrintReturnEverySecond());
    }

    // Update is called once per frame
    void Update()
    {
        string returnedObjectNames = "";
        RayPerceptionOutput.RayOutput[] rayOutputs = rays.RaySensor.RayPerceptionOutput.RayOutputs;
        for(int i = 0; i < rayOutputs.Length; i++)
        {
            GameObject hitObject = rayOutputs[i].HitGameObject;
            if (hitObject != null)
            {
                returnedObjectNames += hitObject.name + ", ";
            }
            else {
                returnedObjectNames += "-, ";
            }
        }
        print(returnedObjectNames);
    }
/*
    IEnumerator PrintReturnEverySecond()
    {
        string returnedObjectNames = "";
        RayPerceptionOutput.RayOutput[] rayOutputs = rays.RaySensor.RayPerceptionOutput.RayOutputs;
        for(int i = 0; i < rayOutputs.Length; i++)
        {
            GameObject hitObject = rayOutputs[i].HitGameObject;
            if (hitObject != null)
            {
                returnedObjectNames += hitObject + ", ";
            }
            else {
                returnedObjectNames += "-, ";
            }
        }
        print(returnedObjectNames);
        yield return new WaitForSeconds(1);
        StartCoroutine(PrintReturnEverySecond());
    }*/
}
