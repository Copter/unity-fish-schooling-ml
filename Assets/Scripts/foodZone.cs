using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodZone : MonoBehaviour
{
    public float foodAmount = 500f;
    private float foodRadius;
    public List<FishAgent> fishArray = new List<FishAgent>();

    // Start is called before the first frame update
    void Start()
    {
        this.foodRadius = (float)Random.Range(5, 10);
        this.transform.localScale = new Vector3(this.foodRadius * 2, this.foodRadius * 2, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (foodAmount <= 0)
        {
            Destroy(this.gameObject);
        }

    }

    void OnTriggerStay2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "fish")
        {
            FishAgent fish = coll.gameObject.GetComponent<FishAgent>();

            float consumptionAmount = fish.maxStomach - fish.stomach;
            if (this.foodAmount > consumptionAmount)
            {
                this.foodAmount -= consumptionAmount;
                fish.stomach += consumptionAmount;
            }
            else
            {
                fish.stomach += this.foodAmount;
                this.foodAmount = 0;
            }

        }
    }


}
