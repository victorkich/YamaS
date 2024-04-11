using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MAData", menuName = "ScriptableObjects/MAData", order = 1)]
public class MAData : ScriptableObject
{
    public bool teste = true;
    public List<AreaData> areaDataList = new List<AreaData>();
    public List<DoorData> doorDataList = new List<DoorData>();
    public List<ObjectData> objectDataList = new List<ObjectData>();
}
