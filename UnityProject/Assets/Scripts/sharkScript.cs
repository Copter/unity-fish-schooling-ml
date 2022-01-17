using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sharkScript : MonoBehaviour
{
    private Rigidbody2D rb;
	public float swimSpeed = 25f;
	Vector2 initialVector;
	public float steerStrength = 2.5f;

    //public RayPerceptionSensorComponent2D rays;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        initialVector = RandomUnitVector();
		rb.velocity = swimSpeed * initialVector;
        //rays = GetComponent<RayPerceptionSensorComponent2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // TO-DO:
        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            Vector2 steerDirVector = new Vector2(-rb.velocity.y, rb.velocity.x);
            steerDirVector = steerDirVector.normalized * steerStrength;
            rb.velocity = steerDirVector + (swimSpeed * rb.velocity.normalized);
        }
        else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
        {
            Vector2 steerDirVector = new Vector2(rb.velocity.y, -rb.velocity.x);
            steerDirVector = steerDirVector.normalized * steerStrength;
            rb.velocity = steerDirVector + (swimSpeed * rb.velocity.normalized);

        }
        else
        {
            rb.velocity = swimSpeed * rb.velocity.normalized;
        }

        transform.rotation = Quaternion.LookRotation(Vector3.forward, rb.velocity);
    }

    public Vector2 RandomUnitVector()
	{
		float randomAngle = Random.Range(0f, 360f);
		return new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
	}
}
