using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LLMSettings", menuName = "ScriptableObjects/LLMSettings", order = 1)]
public class LLMSettings : ScriptableObject
{
    public bool EnableLLM = true;
    public String APIKey = "";
    public String API = "";
    public String APIModel = "";
    public String AvailableFunctions = "";

}
