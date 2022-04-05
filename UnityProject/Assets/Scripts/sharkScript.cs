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
    public float centerRayThickness = 1f;
    public LayerMask rayLayerMask;
    public float raysAngleDeg = 30f;
    RaycastHit2D priorityHit;   //Center ray
    RaycastHit2D leftRayHit;    //Leftmost Ray 
    RaycastHit2D rightRayHit;   //Rightmost Ray
    RaycastHit2D leftRay2Hit;
    RaycastHit2D rightRay2Hit;
    public bool editTankCenterPos = false;
    public Vector3 tankCenterPos;

    //public RayPerceptionSensorComponent2D rays;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        initialVector = RandomUnitVector();
		rb.velocity = swimSpeed * initialVector;
        //rays = GetComponent<RayPerceptionSensorComponent2D>();
        
        if(!editTankCenterPos) tankCenterPos = transform.position;
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

        //RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.TransformDirection(Vector2.up), rayLength, rayLayerMask);    // ~LayerMask.NameToLayer ("Food")
        float raysAngleRad = raysAngleDeg * Mathf.Deg2Rad;
        
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, centerRayThickness, transform.TransformDirection(Vector2.up), rayLength, rayLayerMask);
        RaycastHit2D[] hitsL = Physics2D.CircleCastAll(transform.position, rayThickness, transform.TransformDirection(new Vector2(-Mathf.Sin(raysAngleRad), Mathf.Cos(raysAngleRad))), rayLength, rayLayerMask);
        RaycastHit2D[] hitsR = Physics2D.CircleCastAll(transform.position, rayThickness, transform.TransformDirection(new Vector2(Mathf.Sin(raysAngleRad), Mathf.Cos(raysAngleRad))), rayLength, rayLayerMask);
        RaycastHit2D[] hitsL2 = Physics2D.CircleCastAll(transform.position, rayThickness, transform.TransformDirection(new Vector2(-Mathf.Sin(raysAngleRad/2), Mathf.Cos(raysAngleRad/2))), rayLength, rayLayerMask);
        RaycastHit2D[] hitsR2 = Physics2D.CircleCastAll(transform.position, rayThickness, transform.TransformDirection(new Vector2(Mathf.Sin(raysAngleRad/2), Mathf.Cos(raysAngleRad/2))), rayLength, rayLayerMask);

        Debug.DrawRay(transform.position, transform.TransformDirection(Vector2.up) * rayLength);
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector2(Mathf.Sin(raysAngleRad), Mathf.Cos(raysAngleRad))) * rayLength);
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector2(-Mathf.Sin(raysAngleRad), Mathf.Cos(raysAngleRad))) * rayLength);
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector2(Mathf.Sin(raysAngleRad/2), Mathf.Cos(raysAngleRad/2))) * rayLength);
        Debug.DrawRay(transform.position, transform.TransformDirection(new Vector2(-Mathf.Sin(raysAngleRad/2), Mathf.Cos(raysAngleRad/2))) * rayLength);

        if (hits.Length > 0) 
        {
            //RaycastHit2D hit = hits[0];

            foreach(RaycastHit2D hit in hits)
            {
                if(hit.collider.gameObject.tag == "agent")
                {
                    priorityHit = hit;
                    break;
                }
                else if(hit.collider.gameObject.tag == "wall"/* && priorityHit.collider.gameObject.tag != "agent"*/)
                {
                    priorityHit = hit;
                }
            }

            foreach(RaycastHit2D hit in hitsL)
            {
                if(hit.collider.gameObject.tag == "agent")
                {
                    leftRayHit = hit;
                    break;
                }
                else if(hit.collider.gameObject.tag == "wall") 
                {
                    leftRayHit = hit;
                }
            }

            foreach(RaycastHit2D hit in hitsR)
            {
                if(hit.collider.gameObject.tag == "agent")
                {
                    rightRayHit = hit;
                    break;
                }
                else if(hit.collider.gameObject.tag == "wall") 
                {
                    rightRayHit = hit;
                }
            }

            foreach(RaycastHit2D hit in hitsL2)
            {
                if(hit.collider.gameObject.tag == "agent")
                {
                    leftRay2Hit = hit;
                    break;
                }
                else if(hit.collider.gameObject.tag == "wall") 
                {
                    leftRay2Hit = hit;
                }
            }

            foreach(RaycastHit2D hit in hitsR2)
            {
                if(hit.collider.gameObject.tag == "agent")
                {
                    rightRay2Hit = hit;
                    break;
                }
                else if(hit.collider.gameObject.tag == "wall") 
                {
                    rightRay2Hit = hit;
                }
            }

            
            //if (hit) 
            //{
                //print("Raycast hit: " + hit.collider.name);

                if(priorityHit.collider.gameObject.tag == "agent")
                {
                    //print("Target acquired: " + priorityhit.collider.name);
                    Transform TargetObjTransform = priorityHit.collider.gameObject.transform;

                    var relativePoint = transform.InverseTransformPoint(TargetObjTransform.position);
                    if (relativePoint.x < -1f) 
                    {
                        //print (priorityHit.collider.name + " is slightly left");
                        turnLeft();
                    } 
                    else if (relativePoint.x > 1f) 
                    {
                        //print (priorityHit.collider.name + " is slightly right");
                        turnRight();
                    } 
                    else 
                    {
                        //print (priorityHit.collider.name + " is directly ahead");
                        rb.velocity = swimSpeed * rb.velocity.normalized;
                    }

                    transform.rotation = Quaternion.LookRotation(Vector3.forward, rb.velocity);
                }
                else if(hitsL.Length > 0 && leftRayHit.collider.gameObject.tag == "agent") 
                {
                    //print (leftRayHit.collider.name + " is far left");
                    turnLeft();
                }
                else if(hitsR.Length > 0 && rightRayHit.collider.gameObject.tag == "agent") 
                {
                    //print (rightRayHit.collider.name + " is far right");
                    turnRight();
                }
                else if(hitsL2.Length > 0 && leftRay2Hit.collider.gameObject.tag == "agent") 
                {
                    //print (leftRay2Hit.collider.name + " is to the left");
                    turnLeft();
                }
                else if(hitsR2.Length > 0 && rightRay2Hit.collider.gameObject.tag == "agent") 
                {
                    //print (rightRay2Hit.collider.name + " is to the right");
                    turnRight();
                }
                /*
                else if(leftRayHit.collider.gameObject.tag == "wall") 
                {
                    print (priorityHit.collider.name + " is to the left, avoiding it.");
                    turnRight();
                }
                else if(rightRayHit.collider.gameObject.tag == "wall") 
                {
                    print (priorityHit.collider.name + " is to the right, avoiding it.");
                    turnLeft();
                }
                */
                else if(priorityHit.collider.gameObject.tag == "wall")
                {
                    //if(priorityHit.distance <= 10f) {
                    //print (priorityHit.collider.name + " ahead, avoiding it.");

                    var relativePoint = transform.InverseTransformPoint(tankCenterPos); //Vector3.zero
                    if (relativePoint.x < -1f) 
                    {
                        turnLeft();
                    } 
                    else if (relativePoint.x > 1f) 
                    {
                        turnRight();
                    } 
                    else 
                    {
                        rb.velocity = swimSpeed * rb.velocity.normalized;
                    }

                    transform.rotation = Quaternion.LookRotation(Vector3.forward, rb.velocity);
                    //}
                }
            //}
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

    public void turnLeft()
    {
        Vector2 steerDirVector = new Vector2(-rb.velocity.y, rb.velocity.x);
        steerDirVector = steerDirVector.normalized * steerStrength;
        rb.velocity = steerDirVector + (swimSpeed * rb.velocity.normalized);
    }

    public void turnRight()
    {
        Vector2 steerDirVector = new Vector2(rb.velocity.y, -rb.velocity.x);
        steerDirVector = steerDirVector.normalized * steerStrength;
        rb.velocity = steerDirVector + (swimSpeed * rb.velocity.normalized);
    }

    /*
    RaycastHit2D resultHit;
    public RaycastHit2D rayHitAgentOrWall(RaycastHit2D[] arrayOfHits)
    {
        foreach(RaycastHit2D hit in arrayOfHits)
        {
            if(hit.collider.gameObject.tag == "agent")
            {
                return(resultHit);
            }
            else if(hit.collider.gameObject.tag == "wall")
            {
                resultHit = hit;
            }
        }
        return(resultHit);
    }
    */

    private void OnMouseDown()
    {
        GameObject.Find("Main Camera").GetComponent<cameraControl>().followWho = this.gameObject;
        //transform.parent = otherGameObject.transform;
    }
}
