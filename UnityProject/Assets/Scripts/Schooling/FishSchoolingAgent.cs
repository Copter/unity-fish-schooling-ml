using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;
using System.Collections.Generic;

public class FishSchoolingAgent : Agent
{

    //public bool allowKeyboardControl = false;
    public bool observe = false;
    BufferSensorComponent m_BufferSensor;
    FoodCollectorSettings m_FoodCollectorSettings;

    private Rigidbody2D rb;
    Vector2 initialVector;
    public float steerStrength = 25f;
    public float minimumSpeed;
    public float maximumSpeed;

    private float visibleRadius = 25f;

    private int neighborCount = 0;
    

    // The factor of the rate which energy is consumed.

    public enum Orientation { MoveForward, SteerLeft, SteerRight };
    public Orientation currentOrientation;

    EnvironmentParameters m_ResetParams;


    public override void Initialize()
    {
        m_FoodCollectorSettings = FindObjectOfType<FoodCollectorSettings>();
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
        var tank = GetComponentInParent<FishTankSchooling>();
        tank.ResetAgent(gameObject);
        initialVector = RandomUnitVector();
        rb.velocity = maximumSpeed * initialVector;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var localVelocity = transform.InverseTransformDirection(rb.velocity);
        // Agent velocity
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.y);

        //neighborCount


        List<NeighborFish> neighborFishes = ScanEnvironment();

        if(neighborFishes.Count < neighborCount){
            float difference = neighborCount - neighborFishes.Count;
            //negative reward punishment for losing neighbors
            AddReward(-difference);
        }
        neighborCount = neighborFishes.Count;

        foreach(NeighborFish fish in neighborFishes){
            float[] neighborFishData = {fish.PosX, fish.PosY, fish.VelocityX, fish.VelocitY};
            if(observe){
            }
            m_BufferSensor.AppendObservation(neighborFishData);
        }
    }

    private List<NeighborFish> ScanEnvironment(){
        FishTankSchooling tank = transform.parent.GetComponent<FishTankSchooling>();
        if(tank.fishes == null) return new List<NeighborFish>();
        Vector3 selfPosition = this.transform.position;
        Vector3 flockCenter = new Vector3(0,0,0);
        List<NeighborFish> neighborFishes = new List<NeighborFish>();
        foreach(Transform fish in tank.fishes){
            Vector3 neighborFishPosition = fish.position;
            Vector3 offset =  neighborFishPosition - selfPosition;
            float sqrtDst = offset.x * offset.x + offset.y * offset.y;
            Vector2 velocity = fish.gameObject.GetComponent<FishSchoolingAgent>().rb.velocity;
            if(sqrtDst < visibleRadius * visibleRadius){
                NeighborFish neighbor = new NeighborFish(offset.x, offset.y, velocity.x, velocity.y, fish);
                neighborFishes.Add(neighbor);
            }
        }
        return neighborFishes;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers);
    }

    public void MoveAgent(ActionBuffers actionBuffers)
    {
        var continuousActions = actionBuffers.ContinuousActions;
        this.MoveSteer(Mathf.Clamp(continuousActions[0], -1f, 1f));
        this.MoveForward(1);
        transform.rotation = Quaternion.LookRotation(Vector3.forward, rb.velocity);
    }

    public void MoveForward(float input)
    {
        rb.AddForce(transform.up * maximumSpeed * input);
        if (rb.velocity.sqrMagnitude > maximumSpeed * maximumSpeed) // slow it down
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

    void FixedUpdate()
    {

    }

    public Vector2 RandomUnitVector()
    {
        float randomAngle = Random.Range(0f, 360f);
        return new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
            m_FoodCollectorSettings.totalWallHitCount += 1;
            m_FoodCollectorSettings.totalScore -= 1;
            AddReward(-1f);
        }

        if (collision.gameObject.CompareTag("agent"))
        {
            m_FoodCollectorSettings.totalAgentHitCount += 1;
            m_FoodCollectorSettings.totalScore -= 5;
            AddReward(-3f);
        }
    }
}