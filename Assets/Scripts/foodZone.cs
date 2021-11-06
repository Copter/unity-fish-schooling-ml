using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class foodZone : MonoBehaviour
{
    public bool randomPositionOnGameStart = true;
    public float foodAmount = 500f;
    public List<mlControlFishScript> fishArray = new List<mlControlFishScript>();

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

    void OnTriggerStay2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "fish")
        {
            mlControlFishScript fish = coll.gameObject.GetComponent<mlControlFishScript>();
            if(fish.stomach < fish.maxStomach)
            {
                this.foodAmount -= 1f;
                fish.stomach += 1f;
                //fish.isInFoodZone = true;
            }
            //if (!fishArray.Contains(fish))
            //{
                //fishArray.Add(fish);
                //fish.isInFoodZone = true;
            //}
        }
    }


}
