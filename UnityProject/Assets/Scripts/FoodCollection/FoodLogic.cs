using UnityEngine;

public class FoodLogic : MonoBehaviour
{
    public bool respawn;
    public FoodCluster myCluster;

    public void OnEaten()
    {
        if (respawn)
        {
            transform.position = new Vector3(Random.Range(-myCluster.width/2, myCluster.width/2),
                Random.Range(-myCluster.height/2, myCluster.height/2),
                200f) + myCluster.transform.position;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
