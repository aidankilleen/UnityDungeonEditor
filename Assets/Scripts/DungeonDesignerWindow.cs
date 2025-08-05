using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class DungeonDesignerWindow : EditorWindow
{

    private DungeonData dungeonData;

    private int xCoord;
    private int zCoord;

    private GameObject floorPrefab;
    private float cellSize = 1f;

    [MenuItem("Tools/Dungeon Designer")]
    public static void ShowWindow() 
    { 
        GetWindow<DungeonDesignerWindow>("Dungeon Designer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Dungeon Designer Tool", EditorStyles.boldLabel);

        dungeonData = (DungeonData)EditorGUILayout.
                            ObjectField("DungeonData", dungeonData, typeof(DungeonData), false) as DungeonData;

        GameObject newPrefab = (GameObject)EditorGUILayout.ObjectField("Floor Prefab", floorPrefab, typeof(GameObject), false);
        if (newPrefab != floorPrefab)
        {
            floorPrefab = newPrefab;
            if (floorPrefab != null)
                cellSize = GetPrefabSize(floorPrefab);
        }

        GUILayout.Space(10);
        GUILayout.Label("Add Cell by Coordinates", EditorStyles.boldLabel);
        xCoord = EditorGUILayout.IntField("X Coordinate", xCoord);
        zCoord = EditorGUILayout.IntField("Z Coordinate", zCoord);


        if (GUILayout.Button("Press Me")) 
        {
            Debug.Log("Button pressed");

            Debug.Log($"there are {dungeonData.cells.Count} cells in the dungeon");
        }

        if (GUILayout.Button("Add cell"))
        {
            Debug.Log("add cell pressed");

            AddCellAtCoordinate(xCoord, zCoord, floorPrefab);



        }
        
    }
    private float GetPrefabSize(GameObject prefab)
    {
        // Try MeshRenderer first
        MeshRenderer renderer = prefab.GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
            return renderer.bounds.size.x; // Assuming square tiles

        // Fallback to Collider
        Collider collider = prefab.GetComponentInChildren<Collider>();
        if (collider != null)
            return collider.bounds.size.x;

        Debug.LogWarning("Could not detect prefab size, defaulting to 1.");
        return 1f;
    }

    public void AddCellAtCoordinate(int xCoord, int zCoord, GameObject floorPrefab)
    {
        if (dungeonData == null || floorPrefab == null)
        {
            Debug.LogWarning("Please assign DungeonData and FloorPrefab first!");
            return;
        }

        Vector2Int gridPos = new Vector2Int(xCoord, zCoord);


        GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(floorPrefab);
        tile.transform.position = new Vector3(gridPos.x * cellSize, 0, gridPos.y * cellSize);


    }
}
