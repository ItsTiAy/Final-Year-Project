using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public Transform bulletContainer;

    public Agent agent1;
    public TrainingEnemy target;

    private void Start()
    {
        Pathfinding.Initialize();
        ResetScene();
    }

    public void TargetHit(int teamNumber)
    {
        // 1 agent hit, 2 enemy hit

        if (teamNumber == 1)
        {
            Debug.Log("Hit Agent");
            agent1.SetReward(-1);
        }
        else
        {
            Debug.Log("Hit Target");
            agent1.AddReward(1);
        }

        agent1.EndEpisode();

        ResetScene();
    }

    public void ResetScene()
    {
        agent1.StopAllCoroutines();
        target.StopAllCoroutines();

        foreach (Transform bullet in bulletContainer)
        {
            Destroy(bullet.gameObject);
        }

        agent1.transform.localPosition = new Vector3(Random.Range(-11, 11) + 0.5f, Random.Range(-6, 7) + 0.5f);
        target.transform.localPosition = new Vector3(Random.Range(-11, 11) + 0.5f, Random.Range(-6, 7) + 0.5f);

        agent1.GetComponent<EnemyAgent2>().turret.rotation = Quaternion.identity;
        target.turret.rotation = Quaternion.identity;

        agent1.GetComponent<EnemyAgent2>().reloading = false;
        target.reloading = false;

        target.GenerateNewPath();
    }
}
