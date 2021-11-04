using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fishColorEnergy : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        gameObject.GetComponent<SpriteRenderer>().color=new Color(1, gameObject.GetComponent<mlControlFishScript>().energy/100f, gameObject.GetComponent<mlControlFishScript>().energy/100f, 1);
    }
}
