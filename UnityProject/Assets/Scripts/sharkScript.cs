using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sharkScript : MonoBehaviour
{
    private Rigidbody2D rb;
	public float swimSpeed = 25f;
	Vector2 initialVector;
	public float steerStrength = 2.5f;
    public bool playerControlled = false;
    public float rayLength = 42f;
    public float rayThickness = 20f;
    public LayerMask rayLayerMask;


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
        if(playerControlled){
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

        Debug.DrawRay(transform.position, transform.TransformDirection(Vector2.up) * rayLength);

        //RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.up), rayLength, rayLayerMask);    // ~LayerMask.NameToLayer ("Food")
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, rayThickness, transform.TransformDirection(Vector2.up), rayLength, rayLayerMask);

        if (hits.Length > 0) {
            RaycastHit2D hit = hits[0];
            
            if (hit) {
                //print("Raycast hit: " + hit.collider.name);

                if(hit.collider.gameObject.tag == "agent")
                {
                    //print("Target acquired: " + hit.collider.name);
                    Transform TargetObjTransform = hit.collider.gameObject.transform;

                    var relativePoint = transform.InverseTransformPoint(TargetObjTransform.position);
                    if (relativePoint.x < -1f) 
                    {
                        print (hit.collider.name + " is to the left");
                        Vector2 steerDirVector = new Vector2(-rb.velocity.y, rb.velocity.x);
                        steerDirVector = steerDirVector.normalized * steerStrength;
                        rb.velocity = steerDirVector + (swimSpeed * rb.velocity.normalized);
                    } 
                    else if (relativePoint.x > 1f) 
                    {
                        print (hit.collider.name + " is to the right");
                        Vector2 steerDirVector = new Vector2(rb.velocity.y, -rb.velocity.x);
                        steerDirVector = steerDirVector.normalized * steerStrength;
                        rb.velocity = steerDirVector + (swimSpeed * rb.velocity.normalized);
                    } 
                    else 
                    {
                        print (hit.collider.name + " is directly ahead");
                        rb.velocity = swimSpeed * rb.velocity.normalized;
                    }

                    transform.rotation = Quaternion.LookRotation(Vector3.forward, rb.velocity);
                }
                else if(hit.collider.gameObject.tag == "wall")
                {
                    //if(hit.distance <= 10f) {
                    print (hit.collider.name + " ahead, avoiding it.");

                    var relativePoint = transform.InverseTransformPoint(Vector3.zero);
                    if (relativePoint.x < -1f) 
                    {
                        Vector2 steerDirVector = new Vector2(-rb.velocity.y, rb.velocity.x);
                        steerDirVector = steerDirVector.normalized * steerStrength;
                        rb.velocity = steerDirVector + (swimSpeed * rb.velocity.normalized);
                    } 
                    else if (relativePoint.x > 1f) 
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
                    //}
                }
            }
        }

        if(!playerControlled){
            rb.velocity = swimSpeed * rb.velocity.normalized;
            transform.rotation = Quaternion.LookRotation(Vector3.forward, rb.velocity);
        }
    }

    public Vector2 RandomUnitVector()
	{
		float randomAngle = Random.Range(0f, 360f);
		return new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
	}
}
