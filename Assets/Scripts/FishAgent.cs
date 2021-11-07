using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class FishAgent : Agent
{

    //public bool allowKeyboardControl = false;

    private bool isAlive = true;
    public bool isInFoodZone = false;

    public int timeAlive;
    public int timeWhenNotHungry;
    
    private Rigidbody2D rb;
	public float swimSpeed = 1f;
	Vector2 initialVector;
	public float steerStrength = 1f;
    public float minimumSpeed = 0.2f;

    public float energy = 100f;
    public float energyConversionThreshold = 70f;
    public float maxEnergy = 100f;
    public float stomach = 0f;
    public float maxStomach = 100f;

    public float energyConsumptionFactor = 0.05f;
    
    // The factor of the rate which energy is consumed.

	public enum Orientation {MoveForward, SteerLeft, SteerRight};
	public Orientation currentOrientation;
    

    // public Transform Target;

    public override void OnEpisodeBegin()
    {
        // Move the target to a new spot
        transform.position = new Vector2(Random.Range(-20,20), Random.Range(-20,20));
        rb = GetComponent<Rigidbody2D>();
        initialVector = RandomUnitVector();
        rb.velocity = swimSpeed * initialVector;
        isAlive = true;
        energy = 100f;
        stomach = 0f;
        //rb.constraints = RigidbodyConstraints2D.None;
        timeAlive = 0;
        timeWhenNotHungry = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Hungry or not?
        sensor.AddObservation(this.energy < this.energyConversionThreshold? 1f: 0f);
        // Is in food zone or not?
        sensor.AddObservation(isInFoodZone ? 1f : 0f);
        // Agent velocity
        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.y);
        sensor.AddObservation(this.isAlive ? 1f : -1f);
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
            transform.rotation = Quaternion.LookRotation(Vector3.forward, rb.velocity);
            timeAlive += 1;
            if(this.energy >= this.energyConversionThreshold)
            {
                this.timeWhenNotHungry += 1;
            }
            SetReward(this.timeWhenNotHungry);
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
        energy -= 0.05f;
        if(energy < (energyConversionThreshold) && stomach > 0) {
            float conversionAmount = 0f;
            if (this.stomach >= this.maxEnergy - this.energy)
            {
                conversionAmount = this.maxEnergy - this.energy;
                conversionAmount = this.maxEnergy - this.energy;
            }else
            {
                conversionAmount = this.stomach;
            }
            this.energy += conversionAmount;
            this.stomach -= conversionAmount;
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
            //SetReward(this.timeWhenNotHungry);
            this.transform.localPosition = new Vector2(100, 100);
            //rb.constraints = RigidbodyConstraints2D.FreezePosition;
        }
    }

    void OnTriggerStay2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "food")
        {
            isInFoodZone = true;
        }
        else
        {
            isInFoodZone = false;
        }
    }
}