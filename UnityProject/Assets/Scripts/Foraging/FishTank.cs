using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents;

public class FishTank : MonoBehaviour
{
    public GameObject cluster;
    public int numCluster;
    public bool respawnCluster;
    public float fishSpawnRange;
    private List<GameObject> foodClusters = new List<GameObject>();
    private EnvironmentParameters m_ResetParams;
    public List<Transform> fishes = new List<Transform>();

    private void Start()
    {
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        foreach(Transform child in transform){
            if(child.tag == "agent"){
                fishes.Add(child);
            }
        }
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

            GameObject cluster = Instantiate(clusterObject, new Vector3(Random.Range(-widthRange/2, widthRange / 2), Random.Range(-heightRange / 2, heightRange/2),
                    200f) + transform.position,
                Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            cluster.GetComponent<FoodCluster>().respawnCluster = respawnCluster;
            cluster.GetComponent<FoodCluster>().respawnFood = cluster_level > 0.95;
            cluster.GetComponent<FoodCluster>().myTank = this;
            float x_scale = cluster.transform.localScale.x * cluster_level;
            float y_scale = cluster.transform.localScale.y * cluster_level;
            cluster.transform.localScale = new Vector3(x_scale, y_scale, 1);
            foodClusters.Add(cluster);
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
                    200f)
                    + transform.position;
        agent.transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));
    }

    public void ResetArea()
    {
    }
}
