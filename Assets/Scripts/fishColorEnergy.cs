using UnityEngine;

public class FishColorEnergy : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        var health = gameObject.GetComponent<FishAgent>().energy / 100f;
        gameObject.GetComponent<SpriteRenderer>().color=new Color(1, health, health, 1);
    }
}
