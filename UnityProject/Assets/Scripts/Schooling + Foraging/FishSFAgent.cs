using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;
using System.Collections.Generic;

public class FishSFAgent : Agent
{

    //public bool allowKeyboardControl = false;
    public bool observe = false;
    BufferSensorComponent m_BufferSensor;
    FoodCollectorSettingsSF m_FoodCollectorSettings;

    private Rigidbody2D rb;
    Vector2 initialVector;
    public float steerStrength = 25f;
    public float speed;

    // public float stomach;

    private float visibleRadius = 30f;

    private int neighborCount = 0;

    private int foodEaten = 0;
    

    // The factor of the rate which energy is consumed.

    public enum Orientation { MoveForward, SteerLeft, SteerRight };
    public Orientation currentOrientation;

    EnvironmentParameters m_ResetParams;


    public override void Initialize()
    {
        m_FoodCollectorSettings = FindObjectOfType<FoodCollectorSettingsSF>();
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
        var tank = GetComponentInParent<FishTankSF>();
        tank.ResetAgent(gameObject);
        initialVector = RandomUnitVector();
        rb.velocity = speed * initialVector;
        // stomach = 0;
        foodEaten = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var localVelocity = transform.InverseTransformDirection(rb.velocity);
        Vector3 clusterPosition = transform.InverseTransformPoint(transform.parent.GetComponent<FishTankSF>().foodCluster.transform.position);
        // Stomach
        // sensor.AddObservation(stomach < 50f? 1f : 0f);
        // Agent velocity
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.y);
        // sensor.AddObservation(clusterPosition.x);
        // sensor.AddObservation(clusterPosition.y);

        //neighborCount


        List<NeighborFish> neighborFishes = ScanEnvironment();

        if(neighborFishes.Count < neighborCount){
            float difference = neighborCount - neighborFishes.Count;
            //negative reward punishment for losing neighbors
            AddReward(-difference * 0.3f);
            m_FoodCollectorSettings.totalScore -= difference;
        }
        neighborCount = neighborFishes.Count;
        
        foreach(NeighborFish fish in neighborFishes){
            float[] neighborFishData = {fish.PosX, fish.PosY, fish.VelocityX, fish.VelocitY};
            if(observe){
            }
            m_BufferSensor.AppendObservation(neighborFishData);
        }
    }

    public void Update() {
        if ((Time.frameCount % 100) == 0){
            m_FoodCollectorSettings.UpdateNeighborCount(neighborCount);
        }
        // int health = stomach < 50? 0 : 1;
        // GetComponent<SpriteRenderer>().color = new Color(1, health, health, 1);
    }

    private List<NeighborFish> ScanEnvironment(){
        FishTankSF tank = transform.parent.GetComponent<FishTankSF>();
        if(tank.fishes == null) return new List<NeighborFish>();
        Vector3 selfPosition = this.transform.position;
        Vector3 flockCenter = new Vector3(0,0,0);
        List<NeighborFish> neighborFishes = new List<NeighborFish>();
        foreach(Transform fish in tank.fishes){
            Vector3 neighborFishPosition = fish.position;
            Vector3 offset =  transform.InverseTransformPoint(neighborFishPosition);
            float sqrtDst = offset.x * offset.x + offset.y * offset.y;
            Vector2 velocity = transform.InverseTransformVector(fish.gameObject.GetComponent<FishSFAgent>().rb.velocity);
            if(sqrtDst < visibleRadius * visibleRadius){
                NeighborFish neighbor = new NeighborFish(offset.x, offset.y, velocity.x, velocity.y);
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
        rb.AddForce(transform.up * speed * input);
        if (rb.velocity.sqrMagnitude > speed * speed) // slow it down
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

    public Vector2 RandomUnitVector()
    {
        float randomAngle = Random.Range(0f, 360f);
        return new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
    }

    public void Satiate(){
        // stomach = 100f;
        foodEaten++;
        // if(foodEaten == 15)
        // {
        //     EndEpisode();
        // }
    }

    // private void FixedUpdate(){
    //     // if(stomach > 0f){
    //     //     stomach -= 0.01f;
    //     // }
    //     // if(stomach < 50f){
    //     //     AddReward(-0.05f);
    //     //     m_FoodCollectorSettings.totalScore -= 0.05f;
    //     // }
    // }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("food"))
        {
            collision.gameObject.GetComponent<FoodLogicSF>().OnEaten();
            // if(stomach < 50f){
            //     m_FoodCollectorSettings.totalScore += 3;
            //     m_FoodCollectorSettings.foodEatenWhenHungry += 1;
            //     AddReward(3f);
            // }else{
            //     m_FoodCollectorSettings.totalScore -= 2;
            //     m_FoodCollectorSettings.foodEatenWhenNotHungry += 1;
            //     AddReward(-2f);
            // }
            Satiate();
            m_FoodCollectorSettings.totalScore += 10;
            m_FoodCollectorSettings.foodEatenWhenHungry += 1;
            AddReward(2f);
        }
        if (collision.gameObject.CompareTag("wall"))
        {
            m_FoodCollectorSettings.totalWallHitCount += 1;
            m_FoodCollectorSettings.totalScore -= 3;
            AddReward(-3f);
        }

        if (collision.gameObject.CompareTag("agent"))
        {
            m_FoodCollectorSettings.totalAgentHitCount += 1;
            m_FoodCollectorSettings.totalScore -= 5;
            AddReward(-3f);
        }
    }
}