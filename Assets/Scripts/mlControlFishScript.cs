using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class mlControlFishScript : Agent
{

    //public bool allowKeyboardControl = false;

    private bool isAlive = true;
    public bool isInFoodZone = false;

    public int timeAlive;
    
    private Rigidbody2D rb;
	public float swimSpeed = 1f;
	Vector2 initialVector;
	public float steerStrength = 1f;
    public float minimumSpeed = 0.2f;

    public float energy = 100f;
    public float maxEnergy = 100f;
    public float stomach = 0f;
    public float maxStomach = 100f;

    public float energyConsumptionFactor = 0.05f;
    
    // The factor of the rate which energy is consumed.

	public enum Orientation {MoveForward, SteerLeft, SteerRight};
	public Orientation currentOrientation;
    

    // public Transform Target;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
		initialVector = RandomUnitVector();
		rb.velocity = swimSpeed * initialVector;
    }

    public override void OnEpisodeBegin()
    {
        // Move the target to a new spot
        transform.position = new Vector2(Random.Range(-20,20), Random.Range(-20,20));
        isAlive = true;
        energy = 100f;
        stomach = 0f;
        rb.constraints = RigidbodyConstraints2D.None;
        timeAlive = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        // sensor.AddObservation(Target.localPosition.x);
        // sensor.AddObservation(Target.localPosition.y);
        // sensor.AddObservation(this.transform.localPosition.x);
        // sensor.AddObservation(this.transform.localPosition.y);
        sensor.AddObservation(this.energy);
        sensor.AddObservation(isInFoodZone ? 1f : 0f);
        // Agent velocity
        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.y);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        if(isAlive) {
            float horizontalOutput = vectorAction[0];
            this.moveSteer(horizontalOutput);

            float speedOutput = vectorAction[1];
            this.move(speedOutput);

            // set fitness as energy
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
    }


    void FixedUpdate()
    {
        if(isAlive) {
            energyCalculations();
            SetReward(energy);
            transform.rotation = Quaternion.LookRotation(Vector3.forward, rb.velocity);
            deadConditions();
        }
    }

    //////////////////////////////////
    // Update is called once per frame
    /*
    void FixedUpdate()
    {
        if(allowKeyboardControl)
        {
            if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
            {
                currentOrientation = Orientation.SteerLeft;
            }
            else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
            {
                currentOrientation = Orientation.SteerRight;
            }
            else
            {
                currentOrientation = Orientation.MoveForward;
            }

            if (Input.GetKey(KeyCode.W))
            {
                swimSpeed += 0.01f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                if (swimSpeed > 0.011f)
                    swimSpeed -= 0.01f;
            }
        }

		if(currentOrientation == Orientation.MoveForward)	
			move();
		else if(currentOrientation == Orientation.SteerLeft)
			moveSteerLeft();
		else if(currentOrientation == Orientation.SteerRight)
			moveSteerRight();
        
        energyCalculations();

        deadConditions();

		transform.rotation = Quaternion.LookRotation(Vector3.forward, rb.velocity);
    }
    */

	public Vector2 RandomUnitVector()
	{
		float randomAngle = Random.Range(0f, 360f);
		return new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
	}

    
	public void move(float input)
	{
        if(rb.velocity.magnitude <= minimumSpeed && input < 0) {
            rb.velocity = rb.velocity.normalized * minimumSpeed;
        } else {
		    rb.AddForce(swimSpeed * rb.velocity.normalized * input);
        }
	}

	public void moveSteer(float input)
	{
		Vector2 steerDirVector = new Vector2(rb.velocity.y, -rb.velocity.x);
		Vector2 steerLeftForce = steerDirVector.normalized * steerStrength * input;
		rb.AddForce(steerLeftForce);
	}
    

    public void energyCalculations()
    {
        Vector2 steerDirVector = new Vector2(rb.velocity.y, -rb.velocity.x);
        energy -= (rb.velocity.magnitude) * energyConsumptionFactor;
        
        if(energy < (maxEnergy - 1f) && stomach > 0) {
            energy += 1f;
            stomach -= 1f;
        }
    }

    public void deadConditions() 
    {
        bool isOutOfEnergyDead = energy <= 0;
        // bool isNoVelocityDead = deadWhenNoVelocity && rb.velocity.magnitude == 0;
        bool isNoVelocityDead = false;
        isAlive = !isNoVelocityDead && !isOutOfEnergyDead;
        
        if(!isAlive)
        {
            // Destroy(gameObject);
            SetReward(0f);
            this.transform.localPosition = new Vector2(100, 100);
            //rb.constraints = RigidbodyConstraints2D.FreezePosition;
        }
    }

	void OnTriggerStay2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "food" && stomach < maxStomach)
        {
            coll.gameObject.GetComponent<foodZone>().foodAmount += -1f;
            stomach += 1f;
            isInFoodZone = true;
        } else {
            isInFoodZone = false;
        }
    }
}