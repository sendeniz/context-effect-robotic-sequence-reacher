using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Linq;
using System.Collections.Generic;

public class ReacherAgent : Agent
{
    public GameObject pendulumA;
    public GameObject pendulumB;
    public GameObject hand;
    public GameObject goal;
    public GameObject sphere;
    public int ActiveTarget;
    public int TimeTargetTouched;
    public int TimeTargetActive;
    public int NextTarget;
    public bool JustTouchedTarget = false;
    public float MoveSpeed = 0f;
    public float RewardToGet = 1.0f;
    public float MinReward = 0.7f;
    float m_GoalDegree;
    Rigidbody m_RbA;
    Rigidbody m_RbB;
    // speed of the goal zone around the arm (in radians)
    float m_GoalSpeed;
    // radius of the goal zone
    float m_GoalSize;
    // Magnitude of sinusoidal (cosine) deviation of the goal along the vertical dimension
    float m_Deviation;
    // Frequency of the cosine deviation of the goal along the vertical dimension
    float m_DeviationFreq;
    public int NewTargetActive;
    
    Vector3 PrevHandPos;
    Vector3 CenterLocation = new Vector3(0, -8, 0);


    //// Adds zero paddings of size 1x4 to observation vector
    //public float AddZeroPadding;
    //// Adds next target hot encoded identity to observation vector
    //public float AddNextTargetInfo = 1;
    //// Adds hot encoded noise identity from a discrete uniform to the observation vector
    //public float AddUniformNoiseInfo;
    //public List<int> ShuffledIdList = new List<int>() { 1, 0, 0, 0 };
    public bool ShuffleSwitch = false;

    StatsRecorder m_Recorder;

    // Parameters to tune
    public float torqueForce = 50f;
    public float TimeDecay = 0.992f;
    public float DistPenalty = 0f;
    public float MovePenalty;
    // Takes Scalar and substracts Reaction Time for reward value computation
    public float QuickMovementScalar = 0f;

    EnvironmentParameters m_ResetParams;

    /// <summary>
    /// Collect the rigidbodies of the reacher in order to resue them for
    /// observations and actions.
    /// </summary>
    public override void Initialize()
    {
        m_RbA = pendulumA.GetComponent<Rigidbody>();
        m_RbB = pendulumB.GetComponent<Rigidbody>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        SetResetParameters();
    }

    /// <summary>
    /// We collect the normalized rotations, angularal velocities, and velocities of both
    /// limbs of the reacher as well as the relative position of the target and hand.
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(pendulumA.transform.localPosition);
        sensor.AddObservation(pendulumA.transform.rotation);
        sensor.AddObservation(m_RbA.angularVelocity);
        sensor.AddObservation(m_RbA.velocity);

        sensor.AddObservation(pendulumB.transform.localPosition);
        sensor.AddObservation(pendulumB.transform.rotation);
        sensor.AddObservation(m_RbB.angularVelocity);
        sensor.AddObservation(m_RbB.velocity);

        sensor.AddObservation(goal.transform.localPosition);
        sensor.AddObservation(hand.transform.localPosition);

        sensor.AddObservation(NextTarget == 0 ? 1.0f : 0.0f);
        sensor.AddObservation(NextTarget == 1 ? 1.0f : 0.0f);
        sensor.AddObservation(NextTarget == 2 ? 1.0f : 0.0f);
        sensor.AddObservation(NextTarget == 3 ? 1.0f : 0.0f);

        //sensor.AddObservation(0);
        //sensor.AddObservation(0);
        //sensor.AddObservation(0);
        //sensor.AddObservation(0);

        //if (AddNextTargetInfo == 1)
        //{
        //    // Next target identity hot encoded
        //    sensor.AddObservation(NextTarget == 0 ? 1.0f : 0.0f);
        //    sensor.AddObservation(NextTarget == 1 ? 1.0f : 0.0f);
        //    sensor.AddObservation(NextTarget == 2 ? 1.0f : 0.0f);
        //    sensor.AddObservation(NextTarget == 3 ? 1.0f : 0.0f);
        //    m_Recorder.Add("Added next target identity", 1);
        //}
        //else if (AddUniformNoiseInfo == 1)
        //{
        //    // Discrete uniform random noise or randomized hot encoded identity
        //    if (ShuffleSwitch == false)
        //    {
        //        ShuffledIdList = ShuffledIdList.OrderBy(a => System.Guid.NewGuid()).ToList();
        //        ShuffleSwitch = true;
        //    }
        //    else
        //    {

        //    }
        //    sensor.AddObservation(ShuffledIdList[0]);
        //    sensor.AddObservation(ShuffledIdList[1]);
        //    sensor.AddObservation(ShuffledIdList[2]);
        //    sensor.AddObservation(ShuffledIdList[3]);
        //    m_Recorder.Add("Added discrete uniform noise", 1);
        //}
        //else if (AddZeroPadding == 1)
        //{
        //    sensor.AddObservation(0);
        //    sensor.AddObservation(0);
        //    sensor.AddObservation(0);
        //    sensor.AddObservation(0);
        //    m_Recorder.Add("Added zero padding", 1);
        //}

        // Add touch of target to observation vector
        if (JustTouchedTarget)
        {
            sensor.AddObservation(1.0f);
        }
        else
        {
            sensor.AddObservation(0.0f);
        }

        MoveSpeed = Vector3.Distance(hand.transform.position, PrevHandPos);
        PrevHandPos = hand.transform.position;

        m_Recorder.Add("Distance to base", Vector3.Distance(new Vector3(0f, -8.0f, 0f), (hand.transform.position - transform.position)));
        m_Recorder.Add("Distance moved", MoveSpeed);

        //m_Recorder.Add("Quick movement scalar", QuickMovementScalar);
        //m_Recorder.Add("Time Decay", TimeDecay);
        //m_Recorder.Add("Minimum Reward", MinReward);
        //m_Recorder.Add("Torque force", torqueForce);
        //m_Recorder.Add("Movement penality", MovePenalty);

        RewardToGet *= TimeDecay;
        RewardToGet = Mathf.Max(MinReward, RewardToGet);
    }

    /// <summary>
    /// The agent's four actions correspond to torques on each of the two joints.
    /// </summary>
    public override void OnActionReceived(float[] vectorAction)
    {
        m_GoalDegree += m_GoalSpeed;
        UpdateGoalPosition();

        var torqueX = Mathf.Clamp(vectorAction[0], -1f, 1f) * torqueForce;
        var torqueZ = Mathf.Clamp(vectorAction[1], -1f, 1f) * torqueForce;
        m_RbA.AddTorque(new Vector3(torqueX, 0f, torqueZ));

        torqueX = Mathf.Clamp(vectorAction[2], -1f, 1f) * torqueForce;
        torqueZ = Mathf.Clamp(vectorAction[3], -1f, 1f) * torqueForce;
        m_RbB.AddTorque(new Vector3(torqueX, 0f, torqueZ));
    }

    /// <summary>
    /// Used to move the position of the target goal around the agent.
    /// </summary>
    void UpdateGoalPosition()
    {
        // If the correct target has been sucessfully touched and the frame count is larger
        // than 500 timesteps initialize new target that is not the previous target
        if (JustTouchedTarget && Time.frameCount - TimeTargetTouched >= 500)
        {
            bool TargetChosen = false;
            while (TargetChosen == false)
            {
                int NewTarget = Random.Range(0, 4);
                if (NewTarget != NextTarget)
                {
                    TargetChosen = true;
                    ActiveTarget = NextTarget;
                    NextTarget = NewTarget;
                    NewTargetActive = 1;
                }
            }

            m_GoalDegree = ActiveTarget * 90;
            JustTouchedTarget = false;
            TimeTargetActive = Time.frameCount;
            RewardToGet = 1.0f;
        }

        // if the ITI is active collect x,y,z coordinates of hand location
        //else if (JustTouchedTarget && Time.frameCount - TimeTargetTouched < 500)
        //{
        //    Vector3 HandPointCloud = (hand.transform.position - transform.position);
        //    float Handx = HandPointCloud[0];
        //    float Handy = HandPointCloud[1];
        //    float Handz = HandPointCloud[2];
        //    m_Recorder.Add("Hand x coordinate during ITI", Handx);
        //    m_Recorder.Add("Hand y coordinate during ITI", Handy);
        //    m_Recorder.Add("Hand z coordinate during ITI", Handz);
        //}

        var radians = m_GoalDegree * Mathf.PI / 180f;
        var goalX = 8f * Mathf.Cos(radians);
        var goalY = 8f * Mathf.Sin(radians);
        var goalZ = m_Deviation * Mathf.Cos(m_DeviationFreq * radians);
        goal.transform.position = new Vector3(goalY, -8.0f, goalX) + transform.position;

        // measures distance to (next) active target after ITI
        if (NewTargetActive == 1)
        {
            float DistToTargetOnset = Vector3.Distance((goal.transform.position - transform.position), (hand.transform.position - transform.position));
            float DistToCenterOnset = Vector3.Distance((hand.transform.position - transform.position), (CenterLocation));
            m_Recorder.Add("Distance at onset to target", DistToTargetOnset);
            m_Recorder.Add("Distance at onset to center", DistToCenterOnset);
            NewTargetActive = 0;
        }
        float DistToTarget = Vector3.Distance((goal.transform.position - transform.position), (hand.transform.position - transform.position));
        // compute and apply penalty
        float penaltyToApply = (MovePenalty * MoveSpeed) + (DistPenalty * DistToTarget);
        GetComponent<ReacherAgent>().AddReward(penaltyToApply);
    }

    /// <summary>
    /// Resets the position and velocity of the agent and the goal.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        pendulumA.transform.position = new Vector3(0f, -4f, 0f) + transform.position;
        pendulumA.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        m_RbA.velocity = Vector3.zero;
        m_RbA.angularVelocity = Vector3.zero;

        pendulumB.transform.position = new Vector3(0f, -10f, 0f) + transform.position;
        pendulumB.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
        m_RbB.velocity = Vector3.zero;
        m_RbB.angularVelocity = Vector3.zero;

        // Select one of four target from 0 to 3
        ActiveTarget = Random.Range(0, 4);

        bool TargetChosen = false;
        while (TargetChosen == false)
        {
            int NewTarget = Random.Range(0, 4);
            if (NewTarget != ActiveTarget)
            {
                TargetChosen = true;
                NextTarget = NewTarget;
            }
        }

        m_GoalDegree = ActiveTarget * 90;
        TimeTargetActive = Time.frameCount;
        RewardToGet = 1.0f;
        PrevHandPos = hand.transform.position;

        UpdateGoalPosition();

        SetResetParameters();

        m_Recorder = Academy.Instance.StatsRecorder;

        goal.transform.localScale = new Vector3(m_GoalSize, m_GoalSize, m_GoalSize);
    }

    public void SetResetParameters()
    {
        m_GoalSize = m_ResetParams.GetWithDefault("goal_size", 5);
        m_GoalSpeed = Random.Range(-1f, 1f) * m_ResetParams.GetWithDefault("goal_speed", 0); // was 1
        m_Deviation = m_ResetParams.GetWithDefault("deviation", 0);
        m_DeviationFreq = m_ResetParams.GetWithDefault("deviation_freq", 0);

        // Reset parameters to default value if no value is specified in the config .yaml file
        DistPenalty = m_ResetParams.GetWithDefault("DistPenalty", 0.0f);
        MovePenalty = m_ResetParams.GetWithDefault("MovePenalty", 0.0f);
        QuickMovementScalar = m_ResetParams.GetWithDefault("QuickMovementScalar", 0.0f);
        TimeDecay = m_ResetParams.GetWithDefault("TimeDecay", 0.992f);
        MinReward = m_ResetParams.GetWithDefault("MinReward", 0.7f);
        RewardToGet = m_ResetParams.GetWithDefault("RewardToGet", 1.0f);
        //torqueForce = m_ResetParams.GetWithDefault("torqueForce", 50f);

        //AddNextTargetInfo = m_ResetParams.GetWithDefault("AddNextTargetInfo", 1f);
        //AddZeroPadding = m_ResetParams.GetWithDefault("AddZeroPadding", 0f);
        //AddUniformNoiseInfo = m_ResetParams.GetWithDefault("AddUniformNoiseInfo", 0f);
    }
}
