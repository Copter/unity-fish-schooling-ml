using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;

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

    public bool renderVisionConeSelected = true;
    public bool renderNeighborSensorSelected = true;
    public bool renderNeighborRaySelected = true;
    public bool renderNeighborRayAll = true;

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

    public void UpdateNeighborCount(int count){
        avgNeighbors = (avgNeighbors * avgNeighborTicker + count) / (avgNeighborTicker + 1);
        avgNeighborTicker += 1;
    }

    public void Update()
    {
        scoreText.text = $"TotalScore: {totalScore}\nTotalWallHit: {totalWallHitCount}\nTotalAgentHit: {totalAgentHitCount}\nAvgNeighborCount: {avgNeighbors}";

        // Send stats via SideChannel so that they'll appear in TensorBoard.
        // These values get averaged every summary_frequency steps, so we don't
        // need to send every Update() call.
        if ((Time.frameCount % 100) == 0)
        {
            m_Recorder.Add("Agent/TotalScore", totalScore);
            m_Recorder.Add("Agent/TotalWallHit", totalWallHitCount);
            m_Recorder.Add("Agent/TotalAgentHit", totalAgentHitCount);
            m_Recorder.Add("Agent/avgNeighbors", avgNeighbors);
            m_Recorder.Add("Agent/FoodEatenWhenHungry", foodEatenWhenHungry);
            m_Recorder.Add("Agent/FoodEatenWhenNotHungry", foodEatenWhenNotHungry);
        }
    }
}
