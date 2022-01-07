using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishCollisionController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionStay2D(Collision2D coll)
    {
        if (coll.gameObject.tag != "food")
        {
            gameObject.GetComponent<FishAgent>().energy += -10f;
        }
    }
}
