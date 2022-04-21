using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System;
using System.Linq;
using csDelaunay;
using UnityEditor;

public class FishSFAgent : Agent {

    //public bool allowKeyboardControl = false;
    public bool observe = false;
    [NonSerialized]
    private BufferSensorComponent[] m_BufferSensors;
    [NonSerialized]
    private FishTrainer m_FishTrainer;
    [NonSerialized]
    public Rigidbody2D rb;

    [Header("Abilities (edit in FishTrainer)")]
    [field: SerializeField, ReadOnlyField]
    private float steerStrength;
    [field: SerializeField, ReadOnlyField]
    private float maxSpeed;
    [field: SerializeField, ReadOnlyField]
    private float minSpeed;
    [field: SerializeField, ReadOnlyField]
    private float accelerationConstant;
    [field: SerializeField, ReadOnlyField]
    private float neighborSensorRadius;
    [field: SerializeField, ReadOnlyField]
    private float predatorSensorRadius;

    [Header("Rewards (edit in FishTrainer)")]
    // Rewards
    [field: SerializeField, ReadOnlyField]
    private float onEatenReward;
    [field: SerializeField, ReadOnlyField]
    private float eatReward;
    [field: SerializeField, ReadOnlyField]
    private float loseNeighborReward;
    [field: SerializeField, ReadOnlyField]
    private float wallCrashReward;
    [field: SerializeField, ReadOnlyField]
    private float neighborCrashReward;
    [field: SerializeField, ReadOnlyField]
    private float idleReward;

    [Header("Statistics")]
    [field: SerializeField, ReadOnlyField]
    private float score = 0;
    [field: SerializeField, ReadOnlyField]
    private float ApparentSpeed = 0f;
    [field: SerializeField, ReadOnlyField]
    private float accelerationInput = 0f;
    [field: SerializeField, ReadOnlyField]
    private int neighborCount = 0;
    [field: SerializeField, ReadOnlyField]
    private int foodEaten = 0;
    [field: SerializeField, ReadOnlyField]
    public float foodSensoryIntensity = 0f;
    [field: SerializeField, ReadOnlyField]
    public float predatorSensoryIntensity = 0f;
    [field: SerializeField, ReadOnlyField]
    private bool foodVisible = false;
    [field: SerializeField, ReadOnlyField]
    private bool predatorVisible = false;

    [Header("Neighbors")]
    [field: SerializeField, ReadOnlyField]
    public List<NeighborFish> neighborFishes = new List<NeighborFish>();

    public List<VisiblePredator> visiblePredators = new List<VisiblePredator>();

    [NonSerialized]
    private RayPerceptionSensorComponent2D foodSensorComponent;
    // [NonSerialized]
    // private RayPerceptionSensorComponent2D m_PredatorSensorComponent;
    [NonSerialized]
    private EnvironmentParameters environmentParameters;
    [NonSerialized]
    private SpriteRenderer spriteRenderer;
    [NonSerialized]
    public Block block;
    [NonSerialized]
    public FishTankSF tank;
    private Dictionary<Vector2f, Site> sites;
    private List<Edge> edges;
    public bool renderVoronoi = false;
    private BufferSensorComponent fishBufferSensor;
    private VoronoiDiagram voronoiDiagram;

    public override void Initialize() {
        m_FishTrainer = FindObjectOfType<FishTrainer>();
        environmentParameters = Academy.Instance.EnvironmentParameters;
        rb = GetComponent<Rigidbody2D>();
        // m_BufferSensors = GetComponents<BufferSensorComponent>();
        fishBufferSensor = GetComponent<BufferSensorComponent>();
        RayPerceptionSensorComponent2D[] sensorComponents = GetComponents<RayPerceptionSensorComponent2D>();
        voronoiDiagram = FindObjectOfType<VoronoiDiagram>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        foodSensorComponent = sensorComponents[0];

        this.steerStrength = m_FishTrainer.agentSteerStrength;
        this.maxSpeed = m_FishTrainer.agentMaxSpeed;
        this.minSpeed = m_FishTrainer.agentMinSpeed;
        this.accelerationConstant = m_FishTrainer.agentAccelerationConstant;
        this.neighborSensorRadius = m_FishTrainer.agentNeighborSensorRadius;
        this.predatorSensorRadius = m_FishTrainer.agentPredatorSensorRadius;
        this.onEatenReward = m_FishTrainer.onEatenReward;
        this.eatReward = m_FishTrainer.eatReward;
        this.loseNeighborReward = m_FishTrainer.loseNeighborReward;
        this.wallCrashReward = m_FishTrainer.wallCrashReward;
        this.neighborCrashReward = m_FishTrainer.neighborCrashReward;
        this.idleReward = m_FishTrainer.idleReward;
    }

    public override void OnEpisodeBegin() {
        SetResetParameters();
    }

    public void SetResetParameters() {
        tank.ResetAgent(this.transform);
        rb.velocity = (maxSpeed + minSpeed) * 0.5f * RandomUnitVector();
        foodEaten = 0;
    }

    public override void CollectObservations(VectorSensor sensor) {
        (this.neighborFishes, this.visiblePredators) = ScanEnvironment();

        //neighborCount
        if (this.neighborFishes.Count < this.neighborCount) {
            float difference = Math.Abs(neighborCount - neighborFishes.Count);
            //negative reward punishment for losing neighbors
            float loseNeighborTotalReward = difference * loseNeighborReward;
            this.AddScoreReward(loseNeighborTotalReward);
        }
        neighborCount = neighborFishes.Count;

        // #nullable enable
        //         NeighborFish? bestNeighbor = null;
        //         NeighborFish? worstNeighbor = null;
        // #nullable disable
        foreach (NeighborFish neighborFish in neighborFishes) {
            // if (neighborFish.FishComponent.foodSensoryIntensity > 0.1) {
            //     if (!bestNeighbor.HasValue)
            //         bestNeighbor = neighborFish;
            //     else if (neighborFish.FishComponent.foodSensoryIntensity > bestNeighbor.Value.FishComponent.foodSensoryIntensity)
            //         bestNeighbor = neighborFish;
            // }
            // if (neighborFish.FishComponent.predatorSensoryIntensity > 0.1) {
            //     if (!worstNeighbor.HasValue)
            //         worstNeighbor = neighborFish;
            //     else if (neighborFish.FishComponent.predatorSensoryIntensity > worstNeighbor.Value.FishComponent.predatorSensoryIntensity)
            //         worstNeighbor = neighborFish;
            // }

            FishSFAgent agent = neighborFish.FishComponent;
            Vector3 pos = neighborFish.GetRelativePos(this.transform);
            Vector3 velocity = neighborFish.GetRelativeVelocity(this.transform);
            float[] neighborFishData = { pos.x, pos.y, velocity.x, velocity.y, agent.foodSensoryIntensity, agent.predatorSensoryIntensity };
            fishBufferSensor.AppendObservation(neighborFishData);
        }

        Vector2 predatorPos = new Vector2(0, 0);
        Vector2 predatorVel = new Vector2(0, 0);
        if (visiblePredators.Count > 0) {
            predatorPos = visiblePredators[0].GetRelativePos(this.transform);
            predatorVel = visiblePredators[0].GetRelativeVelocity(this.transform);
        }
        Vector2 localVelocity = transform.InverseTransformDirection(rb.velocity);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.y);

        sensor.AddObservation(predatorPos.x);
        sensor.AddObservation(predatorPos.y);
        sensor.AddObservation(predatorVel.x);
        sensor.AddObservation(predatorVel.y);

        // if (bestNeighbor is null) {
        //     sensor.AddObservation(0);
        //     sensor.AddObservation(0);
        //     sensor.AddObservation(0);
        //     sensor.AddObservation(0);
        // } else {
        //     Vector3 bestNeighborPos = bestNeighbor.Value.GetRelativePos(this.transform);
        //     Vector3 bestNeighborVel = bestNeighbor.Value.GetRelativeVelocity(this.transform);
        //     sensor.AddObservation(bestNeighborPos.x);
        //     sensor.AddObservation(bestNeighborPos.y);
        //     sensor.AddObservation(bestNeighborVel.x);
        //     sensor.AddObservation(bestNeighborVel.y);
        // }
        // if (worstNeighbor is null) {
        //     sensor.AddObservation(0);
        //     sensor.AddObservation(0);
        //     sensor.AddObservation(0);
        //     sensor.AddObservation(0);
        // } else {
        //     Vector3 worstNeighborPos = worstNeighbor.Value.GetRelativePos(this.transform);
        //     Vector3 worstNeighborVel = worstNeighbor.Value.GetRelativeVelocity(this.transform);
        //     sensor.AddObservation(worstNeighborPos.x);
        //     sensor.AddObservation(worstNeighborPos.y);
        //     sensor.AddObservation(worstNeighborVel.x);
        //     sensor.AddObservation(worstNeighborVel.y);
        // }
    }

    public void Update() {
        if ((Time.frameCount % 100) == 0) {
            m_FishTrainer.UpdateNeighborCount(neighborCount);
        }
        this.spriteRenderer.color = new Color(1, (30 - this.rb.velocity.magnitude) / 20, (30 - this.rb.velocity.magnitude) / 20, 1);
        this.ApparentSpeed = rb.velocity.magnitude;
        // if (this.renderVoronoi) {
        bool inVoronoiList = voronoiDiagram.selectedFish.Contains(this);
        if (Selection.gameObjects.Contains(this.gameObject)) {
            if (!inVoronoiList) voronoiDiagram.selectedFish.Add(this);
        } else {
            if (inVoronoiList) voronoiDiagram.selectedFish.Remove(this);
        }
    }

    private (List<NeighborFish>, List<VisiblePredator>) ScanEnvironment() {
        if (!block) {
            Debug.Log("block not found");
            return (new List<NeighborFish>(), new List<VisiblePredator>());
        }

        if (tank.fishes == null) return (new List<NeighborFish>(), new List<VisiblePredator>());

        Block[] blocksToScan = new Block[9];
        int blockCount = 0;
        for (int i = block.blockXPos - 1; i <= block.blockXPos + 1; i++) {
            for (int j = block.blockYPos - 1; j <= block.blockYPos + 1; j++) {
                if (i < tank.gridBlocks.GetLength(0) && j < tank.gridBlocks.GetLength(0) && i >= 0 && j >= 0) {
                    blocksToScan[blockCount] = tank.gridBlocks[i, j];
                    blockCount++;
                }
            }
        }

        List<FishSFAgent> fishToScan = blocksToScan.Where(block => block != null).SelectMany(block => block.fishInBlock).Distinct().ToList();
        List<Predator> predatorsToScan = blocksToScan.Where(block => block != null).SelectMany(block => block.predatorsInBlock).Distinct().ToList();

        List<NeighborFish> tempNeighborFishes = new List<NeighborFish>();
        List<VisiblePredator> tempVisiblePredators = new List<VisiblePredator>();

        float maxFoodIntensity = 0f;
        float maxPredatorIntensity = 0f;

        // foreach (FishSFAgent fish in tank.fishes) {
        foreach (FishSFAgent fish in fishToScan) {
            Vector3 neighborFishPosition = fish.transform.position;
            Vector3 offset = transform.InverseTransformPoint(neighborFishPosition);
            float sqrtDst = offset.x * offset.x + offset.y * offset.y;

            if (sqrtDst <= neighborSensorRadius * neighborSensorRadius) {
                if (fish.foodSensoryIntensity >= maxFoodIntensity) maxFoodIntensity = fish.foodSensoryIntensity;
                if (fish.predatorSensoryIntensity >= maxPredatorIntensity) maxPredatorIntensity = fish.predatorSensoryIntensity;
                NeighborFish neighbor = new NeighborFish(fish);
                tempNeighborFishes.Add(neighbor);
            }
        }

        bool tempPredatorVisible = false;
        // foreach (Predator predator in tank.predators) {
        foreach (Predator predator in tank.predators) {
            Vector3 visiblePredatorPosition = predator.transform.position;
            Vector3 offset = transform.InverseTransformPoint(visiblePredatorPosition);
            float sqrtDst = offset.x * offset.x + offset.y * offset.y;

            if (sqrtDst <= predatorSensorRadius * predatorSensorRadius) {
                Vector2 velocity = transform.InverseTransformVector(predator.rb.velocity);
                VisiblePredator visiblePredator = new VisiblePredator(predator);
                tempVisiblePredators.Add(visiblePredator);
                tempPredatorVisible = true;
            }
        }

        RayPerceptionSensor foodRaySensor = foodSensorComponent.RaySensor;
        bool tempFoodVisible = false;
        foreach (RayPerceptionOutput.RayOutput output in foodRaySensor.RayPerceptionOutput.RayOutputs) {
            if (output.HitGameObject) {
                if (output.HitGameObject.CompareTag("food")) {
                    tempFoodVisible = true;
                }
            }
        }
        this.foodVisible = tempFoodVisible;

        this.predatorVisible = tempPredatorVisible;

        if (!this.foodVisible) {
            this.foodSensoryIntensity = maxFoodIntensity > 0.1 ? maxFoodIntensity * 0.8f : 0;
        } else {
            this.foodSensoryIntensity = 1f;
        }
        if (!this.predatorVisible) {
            this.predatorSensoryIntensity = maxPredatorIntensity > 0.1 ? maxPredatorIntensity * 0.8f : 0;
        } else {
            this.predatorSensoryIntensity = 1f;
        }

        tempNeighborFishes.OrderBy(a => a.GetRelativePos(this.transform).sqrMagnitude);
        tempVisiblePredators.OrderBy(a => a.GetRelativePos(this.transform).sqrMagnitude);

        return (tempNeighborFishes, tempVisiblePredators);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers) {
        this.AddScoreReward(idleReward);
        MoveAgent(actionBuffers);
    }

    public void MoveAgent(ActionBuffers actionBuffers) {
        var continuousActions = actionBuffers.ContinuousActions;
        this.MoveSteer(Mathf.Clamp(continuousActions[0], -1f, 1f));
        this.MoveForward(Mathf.Clamp(continuousActions[1], -1f, 1f));
        this.accelerationInput = Mathf.Clamp(continuousActions[1], -1f, 1f);
        transform.rotation = Quaternion.LookRotation(Vector3.forward, rb.velocity);
    }

    public void MoveForward(float input) {
        rb.AddForce(transform.up * accelerationConstant * input);
        if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed) // slow it down
        {
            rb.velocity *= 0.8f;
        } else if (rb.velocity.sqrMagnitude < minSpeed * minSpeed) {
            rb.velocity *= 1.2f;
        }
    }

    public void MoveSteer(float input) {
        Vector2 steerDirVector = new Vector2(rb.velocity.y, -rb.velocity.x);
        Vector2 steerLeftForce = steerDirVector.normalized * steerStrength * input;
        rb.AddForce(steerLeftForce);
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        var continuousActionsOut = actionsOut.ContinuousActions;
        if (Input.GetKey(KeyCode.D)) {
            continuousActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.W)) {
            continuousActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.A)) {
            continuousActionsOut[0] = -1;
        }
        if (Input.GetKey(KeyCode.S)) {
            continuousActionsOut[1] = -1;
        }
    }

    public Vector2 RandomUnitVector() {
        float randomAngle = UnityEngine.Random.Range(0f, 360f);
        return new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
    }

    public void Satiate() {
        foodEaten++;
    }

    private void FixedUpdate() {
    }
    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("food")) {
            collision.gameObject.GetComponent<FoodLogicSF>().OnEaten();
            Satiate();
            m_FishTrainer.foodEaten += 1;
            this.AddScoreReward(eatReward);
        }
    }
    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("wall")) {
            m_FishTrainer.totalWallHitCount += 1;
            this.AddScoreReward(wallCrashReward);
        }

        if (collision.gameObject.CompareTag("agent")) {
            m_FishTrainer.totalAgentHitCount += 1;
            this.AddScoreReward(neighborCrashReward);
        }
    }
    public void OnEaten() {
        this.AddScoreReward(onEatenReward);
        this.m_FishTrainer.fishEaten += 1;
        EndEpisode();
    }
    void AddScoreReward(float reward) {
        this.score += reward;
        m_FishTrainer.totalScore += reward;
        AddReward(reward);
    }
    private void OnDrawGizmosSelected() {
        if (m_FishTrainer.renderNeighborRaySelected) {
            foreach (NeighborFish fish in neighborFishes) {
                Vector3 target = transform.TransformPoint(fish.GetRelativePos(this.transform));
                float intensity = ((neighborSensorRadius - Vector2.Distance(new Vector2(0, 0), fish.GetRelativePos(this.transform))) / neighborSensorRadius);
                Gizmos.color = new Color(0, 1, 1, intensity);
                Gizmos.DrawLine(transform.position, target);
            }
        }
        if (m_FishTrainer.renderNeighborSensorSelected) {
            //draw neighbor sensor radius
            Gizmos.color = Color.blue;
            float corners = 30; // How many corners the circle should have
            float size = neighborSensorRadius; // How wide the circle should be
            Vector3 origin = transform.position; // Where the circle will be drawn around
            Vector3 startRotation = transform.right * size; // Where the first point of the circle starts
            Vector3 lastPosition = origin + startRotation;
            float angle = 0;
            while (angle <= 360) {
                angle += 360 / corners;
                Vector3 nextPosition = origin + (Quaternion.Euler(0, 0, angle) * startRotation);
                Gizmos.DrawLine(lastPosition, nextPosition);
                // Gizmos.DrawSphere(nextPosition, 1);

                lastPosition = nextPosition;
            }
        }

        if (m_FishTrainer.renderPredatorSensorSelected) {
            //draw neighbor sensor radius
            Gizmos.color = Color.yellow;
            float corners = 30; // How many corners the circle should have
            float size = predatorSensorRadius; // How wide the circle should be
            Vector3 origin = transform.position; // Where the circle will be drawn around
            Vector3 startRotation = transform.right * size; // Where the first point of the circle starts
            Vector3 lastPosition = origin + startRotation;
            float angle = 0;
            while (angle <= 360) {
                angle += 360 / corners;
                Vector3 nextPosition = origin + (Quaternion.Euler(0, 0, angle) * startRotation);
                Gizmos.DrawLine(lastPosition, nextPosition);
                // Gizmos.DrawSphere(nextPosition, 1);

                lastPosition = nextPosition;
            }
        }

        if (m_FishTrainer.renderPredatorRaySelected) {
            foreach (VisiblePredator visiblePredator in visiblePredators) {
                Vector3 target = transform.TransformPoint(visiblePredator.GetRelativePos(this.transform));
                float intensity = ((predatorSensorRadius - Vector2.Distance(new Vector2(0, 0), visiblePredator.GetRelativePos(this.transform))) / predatorSensorRadius);
                Gizmos.color = new Color(1, 0, 0, intensity);
                Gizmos.DrawLine(transform.position, target);
            }
        }

        if (m_FishTrainer.renderVisionConeSelected) {
            //draw food ray sensor
            Gizmos.color = Color.yellow;
            float corners = 30; // How many corners the circle should have
            float size = neighborSensorRadius; // How wide the circle should be
            Vector3 origin = transform.position; // Where the circle will be drawn around
            Vector3 startRotation = transform.TransformDirection((Quaternion.Euler(0, 0, 18f) * new Vector3(1, 0, 0)) * 100); // Where the first point of the circle starts
            Vector3 lastPosition = origin + startRotation;
            float angle = 0;
            Gizmos.DrawLine(transform.position, lastPosition);
            while (angle <= 70 * 2) {
                angle += 360 / corners;
                Vector3 nextPosition = origin + (Quaternion.Euler(0, 0, angle) * startRotation);
                Gizmos.DrawLine(lastPosition, nextPosition);
                lastPosition = nextPosition;
            }
            Gizmos.DrawLine(transform.position, lastPosition);
        }
    }

    private void OnDrawGizmos() {
        if (m_FishTrainer.renderNeighborRayAll) {
            Gizmos.color = new Color(0, 1, 1, 1);
            foreach (NeighborFish fish in neighborFishes) {
                Vector3 target = transform.TransformPoint(fish.GetRelativePos(this.transform));
                Gizmos.DrawLine(transform.position, target);
            }
        }
    }
    private void OnMouseDown()
    {
        GameObject.Find("Main Camera").GetComponent<CameraControl>().followWho = gameObject;
        GameObject.Find("Main Camera").GetComponent<CameraControl>().followName = gameObject.name;
        GameObject.Find("Main Camera").GetComponent<CameraControl>().framesFollowed = 0;
    }
}