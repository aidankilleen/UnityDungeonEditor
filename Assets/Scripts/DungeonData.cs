using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NewDungeonData", menuName ="Dungeon/Dungeon Data")]
public class DungeonData : ScriptableObject
{
    public List<DungeonCell> cells = new List<DungeonCell>();    
}

[System.Serializable]
public class DungeonCell
{
    public Vector2Int gridPosition;
    public bool hasFloor;
    public GameObject floorPrefab;
}
