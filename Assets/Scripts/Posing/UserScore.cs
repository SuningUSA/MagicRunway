﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserScore : MonoBehaviour {

    public Text UserName;
    public GameObject ScoreTextPrefab;
    public PoseFeedbackLightraysFX lightRaysFX;

    long userID;

    public void DebugScoreText()
    {
        GenerateEffects();
    }

    public void Init(long user)
    {
        userID = user;
        UserName.text = user.ToString();
    }

    void OnEnable()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.User_Pose_Detected, OnUserPoseMatched);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.User_Pose_Detected, OnUserPoseMatched);
    }

    public void OnUserPoseMatched(object param, object paramEx)
    {
        long matchedUserID = (long)param;
        if (matchedUserID == userID)
        {
            GenerateEffects();
        }
    }

    public void GenerateEffects()
    {
        lightRaysFX.StartFX();
        GenerateScoreText();
    }

    public void GenerateScoreText()
    {
        Instantiate(ScoreTextPrefab, this.transform);
    }
}
