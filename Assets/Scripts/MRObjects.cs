﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class DesignerCollection : System.Object
{
    public string id;
    public string name;
    public string type;
    public string order;

}

[System.Serializable]
public class Outfits : System.Object
{
    public List<Outfit> outfits;

    public Dictionary<string, Outfit> to_dict()
    {
        Dictionary<string, Outfit> dict = new Dictionary<string, Outfit>();

        for (int i = 0; i < outfits.Count; i++)
        {
            dict.Add(outfits[i].id, outfits[i]);
        }

        return dict;
    }
}

[System.Serializable]
public class Outfit : System.Object
{
    public string id;
    public string name;
    public string sex;
    public List<string> wearableids;
    public List<Wearable> wearables;
}

[System.Serializable]
public class Wearables : System.Object
{
    public List<Wearable> wearables;

    public Dictionary<string, Wearable> to_dict()
    {
        Dictionary<string, Wearable> dict = new Dictionary<string, Wearable>();

        for (int i = 0; i < wearables.Count; i++)
        {
            dict.Add(wearables[i].id, wearables[i]);
        }

        return dict;
    }
}

[System.Serializable]
public class Wearable : System.Object
{
    public string id;
    public string name;
    public string img;
    public string sex;
}