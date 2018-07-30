﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPoseRecognizingAgent : Agent {

    private KinectManager kinectMgr;
    private long KinectUserId;

    private BrainDataManager brainMgr;
    private int poseID;

    private bool isPoseMatched = false;
    private int PoseMatchCount;
    private float estimationTimeEllapsed;
    private float poseScore;

    PoseParameter pose;

    public void Init(long userID, int PoseID)
    {
        kinectMgr = KinectManager.Instance;
        brainMgr = BrainDataManager.Instance;
        KinectUserId = userID;
        poseID = PoseID;

        pose = brainMgr.GetPoseInfo(poseID);

        if (agentParameters == null)
            agentParameters = new AgentParameters();
    }

    void Update()
    {
        estimationTimeEllapsed += Time.deltaTime;

        EstimatePose();
        if (isPoseMatched)
        {
            EventMsgDispatcher.Instance.TriggerEvent(EventDef.User_Pose_Detected, poseID);
            isPoseMatched = false;
        }
    }

    public override void InitializeAgent()
    {
    }

    public override void CollectObservations()
    {
        if (KinectUserId == 0)
            KinectUserId = kinectMgr.GetPrimaryUserID(); //Delete later
        if (kinectMgr.IsUserInKinectView(KinectUserId))
        {
            for (int i = 0; i < pose.num_joint_detections; i++)
            {
                int JointIdx = pose.joint_ids[i];
                if (kinectMgr.IsJointTracked(KinectUserId, JointIdx))
                {
                    Vector3 JointOrientation = kinectMgr.GetJointOrientation(KinectUserId, JointIdx).eulerAngles;
                    JointOrientation = NormalizeAngles(JointOrientation);
                    AddVectorObs(JointOrientation);
                }
            }
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        int isMatched = Mathf.RoundToInt(vectorAction[0]);
        if (isMatched >= 1)
            PoseMatchCount++;
        //Debug.Log(string.Format("Agent {0}: action = {1}, isMatched = {2}", poseID, vectorAction[0], isMatched));
    }

    public override void AgentOnDone()
    {
        //Destroy(gameObject);
    }

    public override void AgentReset()
    {
        isPoseMatched = false;
        PoseMatchCount = 0;
        estimationTimeEllapsed = 0f;
    }

    void EstimatePose()
    {
        if (estimationTimeEllapsed > pose.estimate_time)
        {
            if (PoseMatchCount > 0)
            {
                poseScore = PoseMatchCount / estimationTimeEllapsed;
                isPoseMatched = poseScore >= pose.min_confidence;
            }

            estimationTimeEllapsed = 0f;
            PoseMatchCount = 0;
        }
    }

    //Normalizing to [-1, 1]
    Vector3 NormalizeAngles(Vector3 angles)
    {
        return (angles / 180f) - Vector3.one;
    }
}
