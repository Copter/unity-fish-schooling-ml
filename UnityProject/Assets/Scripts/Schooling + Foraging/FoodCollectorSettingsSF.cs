using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using System.Collections.Generic;
using System.Linq;

public class FoodCollectorSettingsSF : MonoBehaviour
{
    [HideInInspector]
    public GameObject[] agents;
    [HideInInspector]
    public FishTankSF[] listArea;
    private float avgNeighbors = 0f;
    private int avgNeighborTicker = 0;
    public int foodEatenWhenNotHungry = 0;
    public int foodEatenWhenHungry = 0;
    public float totalScore = 0;
    public int totalAgentHitCount = 0;
    public int totalWallHitCount = 0;
    public int timeScale = 10;
    public Text scoreText;
    public float defaultClusterLevel = 1f;

    public bool renderVisionConeSelected = false;
    public bool renderNeighborSensorSelected = false;
    public bool renderNeighborRaySelected = true;
    public bool renderNeighborRayAll = false;
    private List<int> fishGroups = new List<int>();

    StatsRecorder m_Recorder;
    EnvironmentParameters m_ResetParams;

    private float cluster_level;

    public void Awake()
    {
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        m_Recorder = Academy.Instance.StatsRecorder;
    }

    void EnvironmentReset()
    {
        cluster_level = m_ResetParams.GetWithDefault("food_cluster", defaultClusterLevel);
        Debug.Log("cluster level: " + cluster_level);
        ClearObjects(GameObject.FindGameObjectsWithTag("food"));
        ClearObjects(GameObject.FindGameObjectsWithTag("food_cluster"));
        agents = GameObject.FindGameObjectsWithTag("agent");
        listArea = FindObjectsOfType<FishTankSF>();
        FoodClusterSF[] listCluster = FindObjectsOfType<FoodClusterSF>();
        foreach (var cluster in listCluster)
        {
            Destroy(cluster.gameObject);
        }
        foreach (var fa in listArea)
        {
            fa.ResetTank(agents, cluster_level);
        }

        totalScore = 0;
    }

    void ClearObjects(GameObject[] objects)
    {
        foreach (var food in objects)
        {
            Destroy(food);
        }
    }

    public void updateFishGrouping(List<int> newGroupings){
        fishGroups = newGroupings;
    }

    public void UpdateNeighborCount(int count){
        avgNeighbors = (avgNeighbors * avgNeighborTicker + count) / (avgNeighborTicker + 1);
        avgNeighborTicker += 1;
    }

    public Dictionary<int, int> GetFishGroupings(List<int> newGrouping){
        Dictionary<int, int> groupings = new Dictionary<int, int>();
        foreach(int num in newGrouping){
            if(groupings.ContainsKey(num)){
                groupings[num] = groupings[num] + 1;
            }else{
                groupings.Add(num, 1);
            }
        }
        return groupings;
    }

    public void Update()
    {
        float averageGroupSize = (float) fishGroups.Average();
        int maximumGroupSize = fishGroups.Max();
        scoreText.text = $"TotalScore: {totalScore}\nTotalWallHit: {totalWallHitCount}\nTotalAgentHit: {totalAgentHitCount}\nAvgNeighborCount: {avgNeighbors}\nAvgGroupSize: {averageGroupSize}\nMaxGroupSize: {maximumGroupSize}";
        Dictionary<int, int> groupings = GetFishGroupings(fishGroups);
        foreach(KeyValuePair<int, int> entry in groupings){
            scoreText.text += $"\ngroup of {entry.Key} : {entry.Value}";
        }
        // Send stats via SideChannel so that they'll appear in TensorBoard.
        // These values get averaged every summary_frequency steps, so we don't
        // need to send every Update() call.

        if ((Time.frameCount % 100) == 0)
        {
            m_Recorder.Add("Agent/avgGroupSize", averageGroupSize);
            m_Recorder.Add("Agent/maximumGroupSize", maximumGroupSize);
            m_Recorder.Add("Agent/TotalScore", totalScore);
            m_Recorder.Add("Agent/TotalWallHit", totalWallHitCount);
            m_Recorder.Add("Agent/TotalAgentHit", totalAgentHitCount);
            m_Recorder.Add("Agent/avgNeighbors", avgNeighbors);
            m_Recorder.Add("Agent/FoodEatenWhenHungry", foodEatenWhenHungry);
            m_Recorder.Add("Agent/FoodEatenWhenNotHungry", foodEatenWhenNotHungry);
        }
    }
}
