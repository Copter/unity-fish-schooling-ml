using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;
using System.Collections.Generic;

public class FishAgent : Agent
{

    //public bool allowKeyboardControl = false;
    public bool observe = false;
    FoodCollectorSettings m_FoodCollecterSettings;
    BufferSensorComponent m_BufferSensor;
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

    public float visibleRadius = 42f;

    public float energy;
    public float energyConversionThreshold = 70f;
    public float maxEnergy = 100f;
    public float stomach;
    public float maxStomach = 100f;
    public float energyConsumptionFactor = 0.001f;
    public int foodEaten;

    
    

    // The factor of the rate which energy is consumed.

    public enum Orientation { MoveForward, SteerLeft, SteerRight };
    public Orientation currentOrientation;

    EnvironmentParameters m_ResetParams;


    public override void Initialize()
    {
        m_FoodCollecterSettings = FindObjectOfType<FoodCollectorSettings>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        rb = GetComponent<Rigidbody2D>();
        SetResetParameters();
        m_BufferSensor = GetComponent<BufferSensorComponent>();
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
        foodEaten = 0;
        timeWhenNotHungry = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var localVelocity = transform.InverseTransformDirection(rb.velocity);

        // Hungry = -1, NotHungry = 1
        sensor.AddObservation(this.stomach >= 70 ? 1f : -1f);
        // Agent velocity
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.y);

        List<Vector2> neighborPositionOffsets = ScanEnvironment();

        foreach(Vector2 offset in neighborPositionOffsets){
            float[] pos = {offset.x, offset.y};
            if(observe){
            }
            m_BufferSensor.AppendObservation(pos);
        }
    }

    private List<Vector2> ScanEnvironment(){
        FishTank tank = transform.parent.GetComponent<FishTank>();
        if(tank.fishes == null) return new List<Vector2>();
        Vector3 selfPosition = this.transform.position;
        Vector3 flockCenter = new Vector3(0,0,0);
        List<Vector2> neighborPositionOffsets = new List<Vector2>();
        foreach(Transform fish in tank.fishes){
            Vector3 neighborFishPosition = fish.position;
            Vector3 offset =  neighborFishPosition - selfPosition;
            float sqrtDst = offset.x * offset.x + offset.y * offset.y;
            if(sqrtDst < visibleRadius * visibleRadius){
                neighborPositionOffsets.Add(new Vector2(offset.x, offset.y));
            }
        }
        return neighborPositionOffsets;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers);
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
        if (stomach < 70 && MaxStep > 0)
        {
            AddReward(-0.01f);
        }
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
        this.foodEaten += 1;
        if(foodEaten == 15)
        {
            EndEpisode();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("food"))
        {
            Satiate();
            collision.gameObject.GetComponent<FoodLogic>().OnEaten();
            m_FoodCollecterSettings.totalScore += 1;
            AddReward(1f);
            // if (this.stomach < 70)
            // {
            //     m_FoodCollecterSettings.totalScore += 1;
            //     AddReward(1f);
            // }
            // else
            // {
            //     m_FoodCollecterSettings.totalScore -= 1;
            //     AddReward(-1f);
            // }

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
    }
}