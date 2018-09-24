﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserScore : MonoBehaviour {

    public GameObject StarPartcilesPrefab;

    public Text UserName;
    public Image[] Stars;

    private int userIdx;
    public int UserIndex
    {
        get
        {
            return userIdx;
        }
    }
    public long UserID
    {
        get
        {
            return KinectManager.Instance.GetUserIdByIndex(userIdx);
        }
    }

    private int numConsecutivePoseMatches = 0;
    private int score = 0;

    void OnEnable()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.User_Pose_Matched, OnPoseMatched);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.User_Pose_Matched, OnPoseMatched);
    }

    public void OnPoseMatched(object[] param)
    {
        int matched_userIdx = (int)param[0];
        int poseID = (int)param[1];
        float pose_confidence = (float)param[2];

        if (matched_userIdx == userIdx)
        {
            numConsecutivePoseMatches++;
            if(numConsecutivePoseMatches > score)
            {
                score++;
                numConsecutivePoseMatches = 0;

                GameObject starParticlesGO = Instantiate(StarPartcilesPrefab, Stars[score - 1].rectTransform.position, Quaternion.identity, Stars[score - 1].rectTransform);
            }
        }
        else
        {
            numConsecutivePoseMatches = 0;
        }

        UpdateStars();
    }

    public void init(int userIdx)
    {
        this.userIdx = userIdx;
        this.score = 0;
        this.numConsecutivePoseMatches = 0;

        if (UserName)
            UserName.text = string.Format("P{0}", userIdx + 1);

        UpdateStars();
    }

    void UpdateStars()
    {
        for (int i = 0; i < Stars.Length; i++)
        {
            Stars[i].fillAmount = (i < score) ? 1f : 0f;
        }

        if(score < Stars.Length)
            Stars[score].fillAmount = ((float)numConsecutivePoseMatches / (score + 1));
    }
}
