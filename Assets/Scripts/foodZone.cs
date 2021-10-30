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

    }

    // Update is called once per frame
    void Update()
    {
        if(foodAmount <= 0) {
            Destroy(this);
        }
        
    }

    
}
