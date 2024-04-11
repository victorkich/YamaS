using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SAData", menuName = "ScriptableObjects/SAData", order = 1)]
public class SAData : ScriptableObject
{
    public bool teste = true;
    public Vector2 roomSize = new Vector2(12, 16); 
    public int numberOfRedAreas = 1;
    public int numberOfGreenAreas = 1;
    public int numberOfObjects = 10;
}
