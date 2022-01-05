using UnityEngine;

public class FoodLogic : MonoBehaviour
{
    public bool respawn;
    public FishTank myArea;

    public void OnEaten()
    {
        if (respawn)
        {
            transform.position = new Vector3(Random.Range(-myArea.range, myArea.range),
                Random.Range(-myArea.range, myArea.range),
                200f) + myArea.transform.position;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
