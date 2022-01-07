using UnityEngine;

public class FishTank : MonoBehaviour
{
    public GameObject food;
    public GameObject badFood;
    public int numFood;
    public int numBadFood;
    public bool respawnFood;
    public float range;

    void CreateFood(int num, GameObject type)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject f = Instantiate(type, new Vector3(Random.Range(-range, range), Random.Range(-range, range),
                    200f) + transform.position,
                Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            f.GetComponent<FoodLogic>().respawn = respawnFood;
            f.GetComponent<FoodLogic>().myArea = this;
        }
    }

    public void ResetFoodArea(GameObject[] agents)
    {
        foreach (GameObject agent in agents)
        {
            if (agent.transform.parent == gameObject.transform)
            {
                ResetAgent(agent);
            }
        }

        CreateFood(numFood, food);
    }

    public void ResetAgent(GameObject agent)
    {
        agent.transform.position = new Vector3(Random.Range(-range, range), Random.Range(-range, range),
                    200f)
                    + transform.position;
        agent.transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
    }

    public void ResetArea()
    {
    }
}
