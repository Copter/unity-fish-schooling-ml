using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System;
using System.Linq;

public class FishSFAgent : Agent {

    //public bool allowKeyboardControl = false;
    public bool observe = false;
    [NonSerialized]
    private BufferSensorComponent m_BufferSensor;
    [NonSerialized]
    private FishTrainer m_FishTrainer;
    [NonSerialized]
    public Rigidbody2D rb;

    [Header("Abilities")]
    [field: SerializeField]
    private float steerStrength = 25f;
    [field: SerializeField]
    private float maxSpeed = 20f;
    [field: SerializeField]
    private float minSpeed = 10f;
    [field: SerializeField]
    private float accelerationConstant = 50f;
    [field: SerializeField]
    private float neighborSensorRadius = 40f;
    [field: SerializeField]
    private float predatorSensorRadius = 65f;

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

    [Header("Rewards")]
    // Rewards
    [field: SerializeField]
    private float onEatenReward = -300f;
    [field: SerializeField]
    private float eatReward = 2.5f;
    [field: SerializeField]
    private float loseNeighborReward = -0.25f;
    [field: SerializeField]
    private float wallCrashReward = -3f;
    [field: SerializeField]
    private float neighborCrashReward = -3f;

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

    public SpriteRenderer spriteRenderer;

    public Block block;

    public FishTankSF tank;

    public override void Initialize() {
        m_FishTrainer = FindObjectOfType<FishTrainer>();
        environmentParameters = Academy.Instance.EnvironmentParameters;
        rb = GetComponent<Rigidbody2D>();
        m_BufferSensor = GetComponent<BufferSensorComponent>();
        RayPerceptionSensorComponent2D[] sensorComponents = GetComponents<RayPerceptionSensorComponent2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        foodSensorComponent = sensorComponents[0];
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

        RayPerceptionSensor foodRaySensor = foodSensorComponent.RaySensor;
        // RayPerceptionSensor predatorRaySensor = m_PredatorSensorComponent.RaySensor;
        bool tempFoodVisible = false;
        foreach (RayPerceptionOutput.RayOutput output in foodRaySensor.RayPerceptionOutput.RayOutputs) {
            if (output.HitGameObject) {
                if (output.HitGameObject.CompareTag("food")) {
                    tempFoodVisible = true;
                }
            }
        }
        foodVisible = tempFoodVisible;

        Vector2 predatorPosition = new Vector2(0f, 0f);
        Vector2 predatorVelocity = new Vector2(0f, 0f);
        if (visiblePredators.Count > 0) {
            predatorPosition = visiblePredators[0].GetRelativePos(this.transform);
            predatorVelocity = visiblePredators[0].GetRelativeVelocity(this.transform);
        }
        var localVelocity = transform.InverseTransformDirection(rb.velocity);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.y);
        sensor.AddObservation(predatorPosition.x);
        sensor.AddObservation(predatorPosition.y);
        sensor.AddObservation(predatorVelocity.x);
        sensor.AddObservation(predatorVelocity.y);

        if (foodVisible) foodSensoryIntensity = 1f;
        if (predatorVisible) predatorSensoryIntensity = 1f;

        //neighborCount
        if (neighborFishes.Count < neighborCount) {
            float difference = neighborCount - neighborFishes.Count;
            //negative reward punishment for losing neighbors
            float loseNeighborTotalReward = difference * loseNeighborReward;
            m_FishTrainer.totalScore += loseNeighborTotalReward;
            this.score += loseNeighborTotalReward;
            AddReward(loseNeighborTotalReward);
        }
        neighborCount = neighborFishes.Count;

        foreach (NeighborFish neighborFish in neighborFishes) {
            FishSFAgent agent = neighborFish.FishComponent;
            Vector2 pos = neighborFish.GetRelativePos(this.transform);
            Vector2 velocity = neighborFish.Velocity(this.transform);
            float[] neighborFishData = { pos.x, pos.y, velocity.x, velocity.y, agent.foodSensoryIntensity, agent.predatorSensoryIntensity };
            m_BufferSensor.AppendObservation(neighborFishData);
        }
    }

    public void Update() {
        if ((Time.frameCount % 100) == 0) {
            m_FishTrainer.UpdateNeighborCount(neighborCount);
        }
        // GetComponent<SpriteRenderer>().color = new Color(1, 1 - foodSensoryIntensity, 1 - foodSensoryIntensity, 1);
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

        this.predatorVisible = tempPredatorVisible;

        if (!this.foodVisible) this.foodSensoryIntensity = maxFoodIntensity * 0.8f;

        if (!this.predatorVisible) this.predatorSensoryIntensity = maxPredatorIntensity * 0.8f;

        return (tempNeighborFishes, tempVisiblePredators);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers) {
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
        this.ApparentSpeed = rb.velocity.magnitude;
        this.spriteRenderer.color = new Color(1, (30 - this.rb.velocity.magnitude) / 20, (30 - this.rb.velocity.magnitude) / 20, 1);
    }
    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("food")) {
            collision.gameObject.GetComponent<FoodLogicSF>().OnEaten();
            Satiate();
            m_FishTrainer.foodEaten += 1;
            this.score += eatReward;
            m_FishTrainer.totalScore += eatReward;
            AddReward(eatReward);
        }
    }
    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("wall")) {
            m_FishTrainer.totalWallHitCount += 1;
            m_FishTrainer.totalScore += wallCrashReward;
            this.score += wallCrashReward;
            AddReward(wallCrashReward);
        }

        if (collision.gameObject.CompareTag("agent")) {
            m_FishTrainer.totalAgentHitCount += 1;
            m_FishTrainer.totalScore += neighborCrashReward;
            this.score += neighborCrashReward;
            AddReward(neighborCrashReward);
        }
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

    public void OnEaten() {
        AddReward(onEatenReward);
        m_FishTrainer.totalScore += onEatenReward;
        m_FishTrainer.fishEaten += 1;
        EndEpisode();
    }
}