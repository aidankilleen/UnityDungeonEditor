using System;
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

[Serializable]
public class DungeonCellSave
{
    public int x;
    public int z;
    public string prefabGuid; // instead of prefabName
}

[Serializable]
public class DungeonSaveData
{
    public List<DungeonCellSave> cells = new List<DungeonCellSave>();
}
