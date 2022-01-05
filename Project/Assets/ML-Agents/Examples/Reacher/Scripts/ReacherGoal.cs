using UnityEngine;
using Unity.MLAgents;


public class ReacherGoal : MonoBehaviour
{
    public GameObject agent;
    public GameObject hand;
    public GameObject goalOn;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == hand)
        {
            goalOn.transform.localScale = new Vector3(1f, 1f, 1f);

            if (agent.GetComponent<ReacherAgent>().JustTouchedTarget == false)
            {
                agent.GetComponent<ReacherAgent>().JustTouchedTarget = true; // record target touched
                agent.GetComponent<ReacherAgent>().ShuffleSwitch = false; // reset switch after touch
                agent.GetComponent<ReacherAgent>().TimeTargetTouched = Time.frameCount; // record time of target touch

                float ReactionTime = (agent.GetComponent<ReacherAgent>().TimeTargetTouched - agent.GetComponent<ReacherAgent>().TimeTargetActive);
                float FastMoveReward = Mathf.Max(agent.GetComponent<ReacherAgent>().QuickMovementScalar - ReactionTime, 0);
                float DecayedReward = agent.GetComponent<ReacherAgent>().RewardToGet;
                float Reward = (FastMoveReward + DecayedReward);
                agent.GetComponent<ReacherAgent>().AddReward(Reward);

                var statsRecorder = Academy.Instance.StatsRecorder;
                //statsRecorder.Add("Fast movement reward", FastMoveReward);
                //statsRecorder.Add("Decayed reward", DecayedReward);
                //statsRecorder.Add("Reward", Reward);
                statsRecorder.Add("Reaction time", ReactionTime);
            }

        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == hand)
        {
            goalOn.transform.localScale = new Vector3(0f, 0f, 0f);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject == hand)
        {

        }
    }
}
