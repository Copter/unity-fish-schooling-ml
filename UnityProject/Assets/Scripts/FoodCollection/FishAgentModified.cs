using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class FishAgentModified : Agent
{

    //public bool allowKeyboardControl = false;
    FoodCollectorSettings m_FoodCollecterSettings;

    private bool isAlive = true;
    public bool isInFoodZone = false;

    public int timeAlive;
    public int timeWhenNotHungry;

    private Rigidbody2D rb;
    public float swimSpeed = 25f;
    Vector2 initialVector;
    public float steerStrength = 25f;
    public float minimumSpeed = 0.2f;
    public float maximumSpeed = 25f;

    public float energy = 100f;
    public float energyConversionThreshold = 70f;
    public float maxEnergy = 100f;
    public float stomach = 0f;
    public float maxStomach = 100f;

    public float energyConsumptionFactor = 0.001f;

    // The factor of the rate which energy is consumed.

    public enum Orientation { MoveForward, SteerLeft, SteerRight };
    public Orientation currentOrientation;

    EnvironmentParameters m_ResetParams;

    RayPerceptionSensorComponent2D rays;
    public bool printWhatISee;


    public override void Initialize()
    {
        m_FoodCollecterSettings = FindObjectOfType<FoodCollectorSettings>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        rb = GetComponent<Rigidbody2D>();
        SetResetParameters();

        rays = gameObject.GetComponent<RayPerceptionSensorComponent2D>();
    }

    public void SetResetParameters()
    {

    }

    public override void OnEpisodeBegin()
    {
        var tank = GetComponentInParent<FishTank>();
        tank.ResetAgent(gameObject);
        initialVector = RandomUnitVector();
        rb.velocity = swimSpeed * initialVector;
        isAlive = true;
        energy = 100f;
        stomach = 0f;
        timeAlive = 0;
        timeWhenNotHungry = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var localVelocity = transform.InverseTransformDirection(rb.velocity);

        // Hungry = -1, NotHungry = 1
        sensor.AddObservation(this.stomach > 0 ? 1f : -1f);
        // Agent velocity
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.y);

        if(printWhatISee){
            string returnedObjectNames = "";
            RayPerceptionOutput.RayOutput[] rayOutputs = rays.RaySensor.RayPerceptionOutput.RayOutputs;
            for(int i = 0; i < rayOutputs.Length; i++)
            {
                GameObject hitObject = rayOutputs[i].HitGameObject;
                if (hitObject != null)
                {
                    returnedObjectNames += hitObject.name + ", ";
                }
                else {
                    returnedObjectNames += "-, ";
                }
            }
            print(returnedObjectNames);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers);
        if (stomach < 1f && MaxStep > 0)
        {
            AddReward(-1f / MaxStep);
        }
    }

    public void MoveAgent(ActionBuffers actionBuffers)
    {
        var continuousActions = actionBuffers.ContinuousActions;
        if (isAlive)
        {
            this.MoveSteer(Mathf.Clamp(continuousActions[0], -1f, 1f));
            this.MoveForward(Mathf.Clamp(continuousActions[1], -1f, 1f));
            transform.rotation = Quaternion.LookRotation(Vector3.forward, rb.velocity);
        }
    }

    public void MoveForward(float input)
    {
        rb.AddForce(transform.up * swimSpeed * input);
        if (rb.velocity.sqrMagnitude > maximumSpeed) // slow it down
        {
            rb.velocity *= 0.95f;
        }
    }

    public void MoveSteer(float input)
    {
        Vector2 steerDirVector = new Vector2(rb.velocity.y, -rb.velocity.x);
        Vector2 steerLeftForce = steerDirVector.normalized * steerStrength * input;
        rb.AddForce(steerLeftForce);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        if (Input.GetKey(KeyCode.D))
        {
            continuousActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.W))
        {
            continuousActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            continuousActionsOut[0] = -1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            continuousActionsOut[1] = -1;
        }
    }

    private void Update()
    {
        var health = energy / 100f;
        GetComponent<SpriteRenderer>().color = new Color(1, health, health, 1);
    }

    void FixedUpdate()
    {
        if (isAlive)
        {
            ConvertFoodToEnergy();
            DeadConditions();
        }
    }

    public Vector2 RandomUnitVector()
    {
        float randomAngle = Random.Range(0f, 360f);
        return new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
    }


    public void ConvertFoodToEnergy()
    {
        energy -= (rb.velocity.magnitude) * energyConsumptionFactor;
        energy -= 0.001f;
        if (stomach > 0)
        {
            float conversionAmount = 0f;
            if (this.stomach >= this.maxEnergy - this.energy)
            {
                conversionAmount = this.maxEnergy - this.energy;
                conversionAmount = this.maxEnergy - this.energy;
            }
            else
            {
                conversionAmount = this.stomach;
            }
            this.energy += conversionAmount;
            this.stomach -= conversionAmount;
        }
    }

    public void DeadConditions()
    {
        bool isOutOfEnergyDead = energy <= 0;
        bool isNoVelocityDead = false;
        isAlive = !isNoVelocityDead && !isOutOfEnergyDead;
        // fish died
        if (!isAlive)
        {
            AddReward(-10f);
            EndEpisode();
        }
    }

    void Satiate()
    {
        this.stomach = this.maxStomach;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("food"))
        {
            Satiate();
            collision.gameObject.GetComponent<FoodLogic>().OnEaten();
            m_FoodCollecterSettings.totalScore += 1;
            AddReward(1f);
        }

        if (collision.gameObject.CompareTag("wall"))
        {
            energy -= 5f;
            m_FoodCollecterSettings.totalWallHitCount += 1;
            m_FoodCollecterSettings.totalScore -= 1;
            AddReward(-1f);
        }

        if (collision.gameObject.CompareTag("agent"))
        {
            energy -= 10f;
            m_FoodCollecterSettings.totalAgentHitCount += 1;
            m_FoodCollecterSettings.totalScore -= 5;
            AddReward(-5f);
        }

        if (collision.gameObject.CompareTag("shark"))
        {
            energy = 0f;
            stomach = 0f;
            /*
            m_FoodCollecterSettings.totalAgentHitCount += 1;
            m_FoodCollecterSettings.totalScore -= 5;
            AddReward(-5f);
            */
        }
    }

    private void OnMouseDown()
    {
        GameObject.Find("Main Camera").GetComponent<cameraControl>().followWho = this.gameObject;
        //transform.parent = otherGameObject.transform;
    }
}