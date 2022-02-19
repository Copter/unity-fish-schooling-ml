using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodCluster : MonoBehaviour
{
    public GameObject food;
    public bool respawnFood;
    public bool respawnCluster = true;
    public float width;
    public float height;
    private float x;
    private float y;
    private List<GameObject>foodArray = new List<GameObject>();
    public FishTank myTank;
    // Start is called before the first frame update
    void Start()
    {
        width = GetComponent<SpriteRenderer>().bounds.size.x;
        height = GetComponent<SpriteRenderer>().bounds.size.y;
        x = transform.position.x;
        y = transform.position.y;

        CreateFood(20, food);
    }

    // Update is called once per frame
    void Update()
    {
        bool empty = true;
        for(int i = 0; i<foodArray.Count; i++)
        {
            if (foodArray[i] != null) empty = false;
        }
        if(empty)
        {
            Transform wall = myTank.transform.Find("Wall");
            Transform upperBorder = wall.Find("borderU");
            Transform leftBorder = wall.Find("borderL");
            float widthRange = transform.lossyScale.x - (upperBorder.lossyScale.x);
            float heightRange = transform.lossyScale.y - (leftBorder.lossyScale.y);
            transform.position = new Vector3(Random.Range(-widthRange / 2, widthRange / 2), Random.Range(-heightRange / 2, heightRange / 2),
                    200f) + myTank.transform.position;
            CreateFood(20, food);
        }
    }

    void CreateFood(int num, GameObject type)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject food = Instantiate(type, new Vector3(Random.Range(-width/2, width/2), Random.Range(-height/2, height/2),
                    200f) + transform.position,
                Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            food.GetComponent<FoodLogic>().respawn = respawnFood;
            food.GetComponent<FoodLogic>().myCluster = this;
            foodArray.Add(food);
        }
    }
}
