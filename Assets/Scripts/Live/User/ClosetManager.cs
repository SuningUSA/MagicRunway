﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosetManager : MonoBehaviour {

    public static int NUMBER_CLOSET_ITEMS = 4;

    public Closet ClosetLeft;
    public Closet ClosetRight;

    private List<Closet> userClosets = new List<Closet>();

    Outfits outfits;

    KinectManager kinect;
    float Dir_lElbow;
    float Dir_rElbow;

    void OnEnable ()
    {
        EventMsgDispatcher.Instance.registerEvent(EventDef.Live_Mode_Set_Up, OnEnterLiveMode);
        EventMsgDispatcher.Instance.registerEvent(EventDef.User_Gender_Selected, OnUserGenderSelected);
        EventMsgDispatcher.Instance.registerEvent(EventDef.Kinect_User_Lost, OnUserLost);
    }

    void OnDisable()
    {
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.Live_Mode_Set_Up, OnEnterLiveMode);
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.User_Gender_Selected, OnUserGenderSelected);
        EventMsgDispatcher.Instance.unRegisterEvent(EventDef.Kinect_User_Lost, OnUserLost);
    }

    public void OnEnterLiveMode(object [] param)
    {
        if (!kinect)
            kinect = KinectManager.Instance;

        if (outfits == null)
            outfits = MRData.Instance.outfits;

        ClosetLeft.Clear();
        ClosetRight.Clear();
    }

    public void OnUserGenderSelected(object[] param)
    {
        long userID = (long)param[0];
        User.Gender userGender = (User.Gender)param[1];

        List<Outfit> userOutfits = userGender == User.Gender.Female ? outfits.femaleOutfits : outfits.maleOutfits;
        if (userClosets.Count == 0)
            ClosetLeft.SetCloset(userID, userGender, userOutfits);  
        else if (userClosets.Count == 1)
             ClosetRight.SetCloset(userID, userGender, userOutfits);
        userClosets.Add(ClosetLeft);
    }

    public void OnUserLost(object [] param)
    {
        long userID = (long)param[0];

        foreach(Closet c in userClosets)
        {
            if(c.OwnerID == userID)
            {
                userClosets.Remove(c);
                c.Clear();
                break;
            }
        }
    }

    void Start ()
    {
        ClosetLeft.Clear();
        ClosetRight.Clear();
    }

    void Update()
    {
        if (kinect && kinect)
        {
            if(ClosetLeft.IsActive && ClosetRight.IsActive)
            {
                long userLID = ClosetLeft.OwnerID;
                long userRID = ClosetRight.OwnerID;
                if (kinect.IsUserTracked(userLID) && kinect.IsUserTracked(userRID))
                {
                    Vector3 userLPos = kinect.GetUserPosition(userLID);
                    Vector3 userRPos = kinect.GetUserPosition(userRID);
                    if(userRPos.x < userLPos.x)
                    {
                        long closetLOwnerID = ClosetLeft.OwnerID;
                        User.Gender closetLGender = ClosetLeft.OwnerGender;
                        List<Outfit> closetLOutfits = ClosetLeft.Outfits;
                        int closetLOutfitPageIndex = ClosetLeft.OutfitPageIndex;
                        ClosetLeft.ResetCloset();
                        ClosetLeft.SetCloset(ClosetRight.OwnerID, ClosetRight.OwnerGender, ClosetRight.Outfits, ClosetRight.OutfitPageIndex);
                        ClosetRight.ResetCloset();
                        ClosetRight.SetCloset(closetLOwnerID, closetLGender, closetLOutfits, closetLOutfitPageIndex);
                    }
                }
            }

            if(ClosetLeft.IsActive)
            {
                long userID = ClosetLeft.OwnerID;
                if (kinect.IsUserTracked(userID))
                {
                    Dir_lElbow = Mathf.Lerp(Dir_lElbow, kinect.GetJointDirection(userID, (int)KinectInterop.JointType.ElbowLeft).y, 0.25f);
                    if (Dir_lElbow >= -0.2f && Dir_lElbow < -0.15f)
                    {
                        ClosetLeft.OnBottomArrowHover();
                    }
                    else if (Dir_lElbow >= -0.15f && Dir_lElbow < -0.075f)
                    {
                        ClosetLeft.OnOutfitItemHover(3);
                    }
                    else if (Dir_lElbow >= -0.075f && Dir_lElbow < 0f)
                    {
                        ClosetLeft.OnOutfitItemHover(2);
                    }
                    else if (Dir_lElbow >= 0f && Dir_lElbow < 0.075f)
                    {
                        ClosetLeft.OnOutfitItemHover(1);
                    }
                    else if (Dir_lElbow >= 0.075f && Dir_lElbow < 0.15f)
                    {
                        ClosetLeft.OnOutfitItemHover(0);
                    }
                    else if (Dir_lElbow >= 0.15f && Dir_lElbow < 2f)
                    {
                        ClosetLeft.OnTopArrowHover();
                    }
                    else
                    {
                        ClosetLeft.OnUnselectAll();
                    }
                }
            }

            if (ClosetRight.IsActive)
            {
                long userID = ClosetRight.OwnerID;
                if (kinect.IsUserTracked(userID))
                {
                    Dir_rElbow = Mathf.Lerp(Dir_lElbow, kinect.GetJointDirection(userID, (int)KinectInterop.JointType.ElbowRight).y, 0.25f);
                    if (Dir_rElbow >= -0.2f && Dir_rElbow < -0.15f)
                    {
                        ClosetRight.OnBottomArrowHover();
                    }
                    else if (Dir_rElbow >= -0.15f && Dir_rElbow < -0.075f)
                    {
                        ClosetRight.OnOutfitItemHover(3);
                    }
                    else if (Dir_rElbow >= -0.075f && Dir_rElbow < 0f)
                    {
                        ClosetRight.OnOutfitItemHover(2);
                    }
                    else if (Dir_rElbow >= 0f && Dir_rElbow < 0.075f)
                    {
                        ClosetRight.OnOutfitItemHover(1);
                    }
                    else if (Dir_rElbow >= 0.075f && Dir_rElbow < 0.15f)
                    {
                        ClosetRight.OnOutfitItemHover(0);
                    }
                    else if (Dir_rElbow >= 0.15f && Dir_rElbow < 2f)
                    {
                        ClosetRight.OnTopArrowHover();
                    }
                    else
                    {
                        ClosetRight.OnUnselectAll();
                    }
                }
            }
        }
    }
}



