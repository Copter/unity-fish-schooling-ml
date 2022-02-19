using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents;

public class FishTankSchooling : MonoBehaviour
{
    public float fishSpawnRange;
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

    public void ResetTank(GameObject[] agents, float cluster_level)
    {
        foreach (GameObject agent in agents)
        {
            if (agent.transform.parent == gameObject.transform)
            {
                ResetAgent(agent);
            }
        }
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
