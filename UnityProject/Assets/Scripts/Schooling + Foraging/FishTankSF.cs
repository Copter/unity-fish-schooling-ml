using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents;

public class FishTankSF : MonoBehaviour
{
    public GameObject cluster;
    public int numCluster;
    public float fishSpawnRange;
    private EnvironmentParameters m_ResetParams;
    // public List<GameObject> foodClusters = new List<GameObject>();
    public GameObject foodCluster;
    public List<Transform> fishes = new List<Transform>();

    public List<List<Transform>> fishGroups = new List<List<Transform>>();

    FoodCollectorSettingsSF m_FoodCollectorSettings;

    private float nextUpdate=0.5f;

    private void Start()
    {
        m_FoodCollectorSettings = FindObjectOfType<FoodCollectorSettingsSF>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        foreach(Transform child in transform){
            if(child.tag == "agent"){
                fishes.Add(child);
            }
        }
    }

    private List<List<Transform>> GetFishGroups(){
        List<List<Transform>> groups = new List<List<Transform>>();
        List<Transform> checkedFishes = new List<Transform>(); 
        foreach(Transform fish in fishes){
            if(!checkedFishes.Contains(fish)){
                List<Transform> currentNetwork = new List<Transform>();
                currentNetwork.Add(fish);
                findNetwork(fish, checkedFishes, currentNetwork);
                groups.Add(currentNetwork);
            }
        }
        return groups;
    }

    private void findNetwork(Transform currentFish, List<Transform> checkedFishes, List<Transform> currentNetwork){
        checkedFishes.Add(currentFish);
        foreach(NeighborFish neighborFish in currentFish.GetComponent<FishSFAgent>().neighborFishes){
            Transform neighborTransform = neighborFish.FishTransform;
            if(!checkedFishes.Contains(neighborTransform)){
                currentNetwork.Add(neighborTransform);
                findNetwork(neighborTransform, checkedFishes, currentNetwork);
            }
        }
    }

    private void Update(){
        if(Time.time>=nextUpdate){
             Debug.Log(Time.time+">="+nextUpdate);
             // Change the next update (current second+1)
             nextUpdate=Mathf.FloorToInt(Time.time)+1;
             // Call your fonction
             UpdateOnSchedule();
         }
    }

    private void UpdateOnSchedule(){
        fishGroups = GetFishGroups();
        List<int> groupings = new List<int>();
        foreach(List<Transform> group in fishGroups){
            groupings.Add(group.Count);
        }
        m_FoodCollectorSettings.updateFishGrouping(groupings);
    }
    
    void CreateFoodCluster(int num, GameObject clusterObject, float cluster_level)
    {
        Transform wall = transform.Find("Wall");
        Transform upperBorder = wall.Find("borderU");
        Transform leftBorder = wall.Find("borderL");
        float widthRange = clusterObject.transform.lossyScale.x - (upperBorder.lossyScale.x);
        float heightRange = clusterObject.transform.lossyScale.y - (leftBorder.lossyScale.y);
        Debug.Log(clusterObject.transform.lossyScale.x);
        Debug.Log(upperBorder.lossyScale.x);
        for (int i = 0; i < num; i++)
        {

            GameObject cluster = Instantiate(clusterObject, new Vector3(0f, 0f, 0f) + transform.position,
                Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            cluster.GetComponent<FoodClusterSF>().respawnFood = true;
            cluster.GetComponent<FoodClusterSF>().myTank = this;
            float x_scale = cluster.transform.localScale.x * cluster_level;
            float y_scale = cluster.transform.localScale.y * cluster_level;
            cluster.transform.localScale = new Vector3(x_scale, y_scale, 1);
            // foodClusters.Add(cluster);
            foodCluster = cluster;
        }
    }

    public void ResetTank(GameObject[] agents, float cluster_level)
    {
        foreach (GameObject agent in agents)
        {
            if (agent.transform.parent == gameObject.transform)
            {
                ResetAgent(agent);
            }
        }
        Transform wall = transform.Find("Wall");
        wall.localScale = new Vector3(1/cluster_level, 1/cluster_level, 1);
        CreateFoodCluster(numCluster, cluster, cluster_level);
    }

    public void ResetAgent(GameObject agent)
    {
        agent.transform.position = new Vector3(Random.Range(-fishSpawnRange, fishSpawnRange), Random.Range(-fishSpawnRange, fishSpawnRange),
                    0f)
                    + transform.position;
        agent.transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
    }

    public void ResetArea()
    {
    }
}
