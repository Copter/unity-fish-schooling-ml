using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using Unity.Mathematics;

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
    public List<NeighborFish> neighborFishes = new List<NeighborFish>();
    private float eatReward = 5f;
    private float loseNeighborReward = -0.35f;
    private float wallCrashReward = -3f;
    private float neighborCrashReward = -3f;
    public float foodSensoryIntensity = 0f;
    private bool foodVisible = false;
    public RayPerceptionSensorComponent2D m_RayPerceptionSensorComponent2D; 


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
        m_RayPerceptionSensorComponent2D = GetComponent<RayPerceptionSensorComponent2D>();
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

    public void SetResetParameters(){

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        RayPerceptionSensor raySensor = m_RayPerceptionSensorComponent2D.RaySensor;
        bool tempFoodVisible = false;
        foreach(RayPerceptionOutput.RayOutput output in raySensor.RayPerceptionOutput.RayOutputs){
            if(output.HitGameObject){
                if(output.HitGameObject.CompareTag("food")){
                    tempFoodVisible = true;
                }
            }
        }
        foodVisible = tempFoodVisible;
        if(foodVisible) foodSensoryIntensity = 1f;

        var localVelocity = transform.InverseTransformDirection(rb.velocity);
        Vector3 clusterPosition = transform.InverseTransformPoint(transform.parent.GetComponent<FishTankSF>().foodCluster.transform.position);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.y);

        //neighborCount
        if(neighborFishes.Count < neighborCount){
            float difference = neighborCount - neighborFishes.Count;
            //negative reward punishment for losing neighbors
            m_FoodCollectorSettings.totalScore += difference * loseNeighborReward;
            AddReward(difference * loseNeighborReward);
            m_FoodCollectorSettings.totalScore -= difference;
        }
        neighborCount = neighborFishes.Count;
        
        foreach(NeighborFish fish in neighborFishes){
            float[] neighborFishData = {fish.PosX, fish.PosY, fish.VelocityX, fish.VelocitY, fish.FishTransform.GetComponent<FishSFAgent>().foodSensoryIntensity};
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
        GetComponent<SpriteRenderer>().color = new Color(1, 1 - foodSensoryIntensity, 1 - foodSensoryIntensity, 1);
    }

    private List<NeighborFish> ScanEnvironment(){
        FishTankSF tank = transform.parent.GetComponent<FishTankSF>();

        if(tank.fishes == null) return new List<NeighborFish>();

        Vector3 selfPosition = this.transform.position;
        Vector3 flockCenter = new Vector3(0,0,0);
        List<NeighborFish> tempNeighborFishes = new List<NeighborFish>();
        List<Transform> tempNeighborTransforms = new List<Transform>();
        List<float> dists = new List<float>();
        
        float maxFoodIntensity = 0f;
        foreach(Transform fish in tank.fishes){
            Vector3 neighborFishPosition = fish.position;
            Vector3 offset =  transform.InverseTransformPoint(neighborFishPosition);
            float sqrtDst = offset.x * offset.x + offset.y * offset.y;

            if(sqrtDst <= visibleRadius * visibleRadius){
                FishSFAgent neighborAgent = fish.gameObject.GetComponent<FishSFAgent>();
                
                if(neighborAgent.foodSensoryIntensity >= maxFoodIntensity) maxFoodIntensity = neighborAgent.foodSensoryIntensity;
                
                Vector2 velocity = transform.InverseTransformVector(neighborAgent.rb.velocity);
                
                NeighborFish neighbor = new NeighborFish(offset.x, offset.y, velocity.x, velocity.y, fish);
                tempNeighborFishes.Add(neighbor);
                tempNeighborTransforms.Add(fish);
                dists.Add(math.sqrt(sqrtDst));
            }
        }
        if(!foodVisible) this.foodSensoryIntensity = maxFoodIntensity * 0.8f;
        if(this.foodSensoryIntensity < 0.1f) this.foodSensoryIntensity = 0;
        
        return tempNeighborFishes;
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
        float randomAngle = UnityEngine.Random.Range(0f, 360f);
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

    private void FixedUpdate(){
        // if(stomach > 0f){
        //     stomach -= 0.01f;
        // }
        // if(stomach < 50f){
        //     AddReward(-0.05f);
        //     m_FoodCollectorSettings.totalScore -= 0.05f;
        // }
        neighborFishes= ScanEnvironment();
    }
    void OnTriggerEnter2D(Collider2D collision)
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
            m_FoodCollectorSettings.foodEatenWhenHungry += 1;
            m_FoodCollectorSettings.totalScore += eatReward;
            AddReward(eatReward);
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("wall"))
        {
            m_FoodCollectorSettings.totalWallHitCount += 1;
            m_FoodCollectorSettings.totalScore += wallCrashReward;
            AddReward(wallCrashReward);
        }

        if (collision.gameObject.CompareTag("agent"))
        {
            m_FoodCollectorSettings.totalAgentHitCount += 1;
            m_FoodCollectorSettings.totalScore += neighborCrashReward;
            AddReward(neighborCrashReward);
        }
    }

    private void OnDrawGizmosSelected(){
        if(m_FoodCollectorSettings.renderNeighborRaySelected){
            foreach(NeighborFish fish in neighborFishes){
                Vector3 target = transform.TransformPoint(fish.GetPos());
                float intensity = ((visibleRadius - Vector2.Distance(new Vector2(0,0),fish.GetPos())) / visibleRadius);
                Gizmos.color = new Color(0,1,1,intensity);
                Gizmos.DrawLine (transform.position, target);
            }
        }
        if(m_FoodCollectorSettings.renderNeighborSensorSelected){
            //draw neighbor sensor radius
            Gizmos.color = Color.blue;
            float corners = 30; // How many corners the circle should have
            float size = visibleRadius; // How wide the circle should be
            Vector3 origin = transform.position; // Where the circle will be drawn around
            Vector3 startRotation = transform.right * size; // Where the first point of the circle starts
            Vector3 lastPosition = origin + startRotation;
            float angle = 0;
            while (angle <= 360)
            {
                angle += 360 / corners;
                Vector3 nextPosition = origin + (Quaternion.Euler(0, 0, angle) * startRotation);
                Gizmos.DrawLine(lastPosition, nextPosition);
                // Gizmos.DrawSphere(nextPosition, 1);
            
                lastPosition = nextPosition;
            }
        }

        if(m_FoodCollectorSettings.renderVisionConeSelected){
        //draw food ray sensor
        Gizmos.color = Color.yellow;
        float corners = 30; // How many corners the circle should have
        float size = visibleRadius; // How wide the circle should be
        Vector3 origin = transform.position; // Where the circle will be drawn around
        Vector3 startRotation = transform.TransformDirection((Quaternion.Euler(0,0,18f) * new Vector3(1,0,0)) * 100); // Where the first point of the circle starts
        Vector3 lastPosition = origin + startRotation;
        float angle = 0;
        Gizmos.DrawLine(transform.position, lastPosition);
        while (angle <= 70 * 2)
        {
            angle += 360 / corners;
            Vector3 nextPosition = origin + (Quaternion.Euler(0, 0, angle) * startRotation);
            Gizmos.DrawLine(lastPosition, nextPosition);
            lastPosition = nextPosition;
        }
        Gizmos.DrawLine(transform.position, lastPosition);
        }
    }

    private void OnDrawGizmos(){
        if(m_FoodCollectorSettings.renderNeighborRayAll){
            Gizmos.color = new Color(0,1,1,1);
            foreach(NeighborFish fish in neighborFishes){
                Vector3 target = transform.TransformPoint(fish.GetPos());
                Gizmos.DrawLine (transform.position, target);
            }
        }
    }
}