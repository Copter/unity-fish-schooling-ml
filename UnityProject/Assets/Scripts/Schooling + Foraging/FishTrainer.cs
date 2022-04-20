using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using System.Collections.Generic;
using System.Linq;

public class FishTrainer : MonoBehaviour {
    [Header("Statistics")]
    [field: SerializeField, ReadOnlyField]
    private float avgNeighbors = 0f;
    [field: SerializeField, ReadOnlyField]
    public int foodEaten = 0;
    [field: SerializeField, ReadOnlyField]
    public float totalScore = 0;
    [field: SerializeField, ReadOnlyField]
    public int totalAgentHitCount = 0;
    [field: SerializeField, ReadOnlyField]
    public int totalWallHitCount = 0;
    [field: SerializeField, ReadOnlyField]
    public int fishEaten = 0;
    [field: SerializeField, ReadOnlyField]
    private int totalFish = 1;

    [Header("Tank Settings")]
    public int fishPerTank;
    public int predatorsPerTank;
    public int clustersPerTank;
    public int fishSpawnRange;
    [Header("Agent Settings")]
    // Abilities
    [field: SerializeField]
    public float agentSteerStrength = 25f;
    [field: SerializeField]
    public float agentMaxSpeed = 20f;
    [field: SerializeField]
    public float agentMinSpeed = 10f;
    [field: SerializeField]
    public float agentAccelerationConstant = 50f;
    [field: SerializeField]
    public float agentNeighborSensorRadius = 40f;
    [field: SerializeField]
    public float agentPredatorSensorRadius = 65f;
    // Rewards
    [field: SerializeField]
    public float onEatenReward = -100f;
    [field: SerializeField]
    public float eatReward = 2.5f;
    [field: SerializeField]
    public float loseNeighborReward = -0.3f;
    [field: SerializeField]
    public float wallCrashReward = -3f;
    [field: SerializeField]
    public float neighborCrashReward = -3f;
    [field: SerializeField]
    public float idleReward = 0f;
    [Header("Predator Settings")]
    [field: SerializeField]
    public float predatorCruiseSpeed = 10f;
    [field: SerializeField]
    public float predatorChaseSpeed = 22f;
    [field: SerializeField]
    public float predatorVisibleRadius = 40f;
    [field: SerializeField]
    public float predatorSteerStrength = 5f;
    [field: SerializeField]
    public float predatorChaseForceMagnitude = 30f;
    [field: SerializeField]
    public int predatorMaxStomach = 10;
    [field: SerializeField]
    public bool predatorSwimToCluster = false;
    [Header("UI Settings")]
    public Text scoreText;
    public bool renderVoronoiSelected = true;
    public bool renderVisionConeSelected = false;
    public bool renderNeighborSensorSelected = false;
    public bool renderNeighborRaySelected = true;
    public bool renderPredatorSensorSelected = false;
    public bool renderPredatorRaySelected = true;
    public bool renderNeighborRayAll = false;
    [Header("Trainer Settings")]
    [field: SerializeField]
    public float defaultClusterLevel = 1f;
    [HideInInspector]
    public GameObject[] agents;
    [HideInInspector]
    public FishTankSF[] listArea;
    [HideInInspector]
    private int avgNeighborTicker = 0;
    private List<int> fishGroups = new List<int>();

    StatsRecorder m_Recorder;
    EnvironmentParameters m_ResetParams;

    private float cluster_level;

    public void Awake() {
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        m_Recorder = Academy.Instance.StatsRecorder;
    }

    void EnvironmentReset() {
        cluster_level = m_ResetParams.GetWithDefault("food_cluster", defaultClusterLevel);
        Debug.Log("cluster level: " + cluster_level);
        ClearObjects(GameObject.FindGameObjectsWithTag("food"));
        ClearObjects(GameObject.FindGameObjectsWithTag("food_cluster"));
        agents = GameObject.FindGameObjectsWithTag("agent");
        listArea = FindObjectsOfType<FishTankSF>();
        FoodClusterSF[] listCluster = FindObjectsOfType<FoodClusterSF>();
        Block[] listBlock = FindObjectsOfType<Block>();
        foreach (var cluster in listCluster) {
            Destroy(cluster.gameObject);
        }
        foreach (var block in listBlock) {
            Destroy(block.gameObject);
        }
        foreach (var fa in listArea) {
            fa.ResetTank(agents, cluster_level);
        }
        int agentCount = agents.Count();
        totalFish = agentCount > 0 ? agentCount : 1;
        totalScore = 0;
    }

    void ClearObjects(GameObject[] objects) {
        foreach (var food in objects) {
            Destroy(food);
        }
    }

    public void updateFishGrouping(List<int> newGroupings) {
        fishGroups = newGroupings;
    }

    public void UpdateNeighborCount(int count) {
        avgNeighbors = (avgNeighbors * avgNeighborTicker + count) / (avgNeighborTicker + 1);
        avgNeighborTicker += 1;
    }

    public Dictionary<int, int> GetFishGroupings(List<int> newGrouping) {
        Dictionary<int, int> groupings = new Dictionary<int, int>();
        foreach (int num in newGrouping) {
            if (groupings.ContainsKey(num)) {
                groupings[num] = groupings[num] + 1;
            } else {
                groupings.Add(num, 1);
            }
        }
        return groupings;
    }

    public void Update() {
        int maximumGroupSize = 0;
        float averageGroupSize = 0f;
        if (fishGroups.Count > 0) {
            averageGroupSize = (float)fishGroups.Average();
            maximumGroupSize = fishGroups.Max();
        }
        scoreText.text = $"AverageScore: {totalScore / totalFish}\n" +
        $"AvgWallHit: {totalWallHitCount / totalFish}\n" +
        $"AverageAgentHit: {totalAgentHitCount / totalFish}\n" +
        $"AvgNeighborCount: {avgNeighbors}\n" +
        $"AvgGroupSize: {averageGroupSize}\n" +
        $"MaxGroupSize: {maximumGroupSize}\n" +
        $"TotalFishEaten: {fishEaten}\n" +
        $"TotalFoodEatn: {foodEaten}\n";
        Dictionary<int, int> groupings = GetFishGroupings(fishGroups);
        foreach (KeyValuePair<int, int> entry in groupings) {
            scoreText.text += $"\ngroup of {entry.Key} : {entry.Value}";
        }
        // Send stats via SideChannel so that they'll appear in TensorBoard.
        // These values get averaged every summary_frequency steps, so we don't
        // need to send every Update() call.

        if ((Time.frameCount % 100) == 0) {
            m_Recorder.Add("Agent/avgGroupSize", averageGroupSize);
            m_Recorder.Add("Agent/maximumGroupSize", maximumGroupSize);
            m_Recorder.Add("Agent/TotalScore", totalScore);
            m_Recorder.Add("Agent/AverageScore", totalScore / totalFish);
            m_Recorder.Add("Agent/TotalWallHit", totalWallHitCount);
            m_Recorder.Add("Agent/TotalAgentHit", totalAgentHitCount);
            m_Recorder.Add("Agent/avgNeighbors", avgNeighbors);
            m_Recorder.Add("Agent/FoodEaten", foodEaten);
            m_Recorder.Add("Agent/TotalTimesEatenByPredator", fishEaten);
            // m_Recorder.Add("Agent/FoodEatenWhenNotHungry", foodEatenWhenNotHungry);
        }
    }
}
