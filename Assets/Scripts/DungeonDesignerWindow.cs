using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class DungeonDesignerWindow : EditorWindow
{

    private DungeonData dungeonData;

    private int xCoord;
    private int zCoord;

    private bool northWall;
    private bool southWall;
    private bool westWall;
    private bool eastWall;

    private GameObject floorPrefab;
    private GameObject wallPrefab;
    private float cellSize = 1f;

    private const string dungeonParentName = "Dungeon";

    private string dungeonFileName = "dungeon";
    private string SaveFilePath => Path.Combine(Application.dataPath, dungeonFileName + ".json");


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

        GameObject newFloorPrefab = (GameObject)EditorGUILayout.ObjectField("Floor Prefab", floorPrefab, typeof(GameObject), false);
        if (newFloorPrefab != floorPrefab)
        {
            floorPrefab = newFloorPrefab;
            if (floorPrefab != null)
                cellSize = GetPrefabSize(floorPrefab);
        }

        GameObject newWallPrefab = (GameObject)EditorGUILayout.ObjectField("Wall Prefab", wallPrefab, typeof(GameObject), false);
        if (newWallPrefab != wallPrefab)
        {
            wallPrefab = newWallPrefab;
          
        }

        EditorGUILayout.LabelField("Walls for New Cell", EditorStyles.boldLabel);
        northWall = EditorGUILayout.Toggle("North Wall", northWall);
        southWall = EditorGUILayout.Toggle("South Wall", southWall); 
        eastWall = EditorGUILayout.Toggle("East Wall", eastWall); 
        westWall = EditorGUILayout.Toggle("West Wall", westWall);

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

            AddCellAtCoordinate(xCoord, zCoord, floorPrefab, wallPrefab, northWall, southWall, eastWall, westWall);
        }

        if (GUILayout.Button("Clear Dungeon"))
        {
            ClearDungeon();
        }


        GUILayout.Space(10);
        GUILayout.Label("Dungeon File", EditorStyles.boldLabel);
        dungeonFileName = EditorGUILayout.TextField("Filename (no extension)", dungeonFileName);

        if (GUILayout.Button("Save Dungeon to JSON"))
        {
            SaveDungeonToJson();
        }
        if (GUILayout.Button("Load Dungeon"))
        {
            LoadDungeonFromJson();
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

    public void AddCellAtCoordinate(int xCoord, int zCoord, 
                GameObject floorPrefab, GameObject wallPrefab, 
                bool northWall, bool southWall, bool eastWall, bool westWall)
    {
        if (dungeonData == null || floorPrefab == null || wallPrefab == null)
        {
            Debug.LogWarning("Please assign DungeonData and FloorPrefab and WallPrefab first!");
            return;
        }

        Vector2Int gridPos = new Vector2Int(xCoord, zCoord);

        // Check if cell already exists
        if (!dungeonData.cells.Exists(c => c.gridPosition == gridPos))
        {
            // Add new cell to data
            dungeonData.cells.Add(new DungeonCell
            {
                gridPosition = gridPos,
                floorPrefab = floorPrefab, 
                wallPrefab = wallPrefab,
                northWall = northWall, 
                southWall = southWall,
                eastWall = eastWall,
                westWall = westWall,
            });

            GameObject parent = GetOrCreateDungeonParent();
            GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(floorPrefab);
            tile.transform.SetParent(parent.transform);

            if (northWall)
            {
                GameObject northWallGameObject = CreateWall(tile.transform.position, 0, wallPrefab);
                northWallGameObject.transform.SetParent(tile.transform);
            }


            tile.transform.position = new Vector3(gridPos.x * cellSize, 0, gridPos.y * cellSize);
        } else
        {
            Debug.Log($"Cell already at {gridPos.x}, {gridPos.y}");
        }

    }

    private GameObject CreateWall(Vector3 position, int direction, GameObject wallPrefab)
    {
        if (wallPrefab == null)
        {
            return null;
        }


        GameObject wall = PrefabUtility.InstantiatePrefab(wallPrefab) as GameObject;
        wall.transform.position = position;

        return wall;

    }

    private GameObject GetOrCreateDungeonParent()
    {
        GameObject parent = GameObject.Find(dungeonParentName);
        if (parent == null)
        {
            parent = new GameObject(dungeonParentName);
            Undo.RegisterCreatedObjectUndo(parent, "Create Dungeon Parent");
        }
        return parent;
    }

    private void ClearDungeon()
    {
        GameObject parent = GetOrCreateDungeonParent();
        Undo.DestroyObjectImmediate(parent);

        dungeonData.cells.Clear();

        EditorUtility.SetDirty(this);
    }
    private void SaveDungeonToJson()
    {
        if (dungeonData == null)
        {
            Debug.LogWarning("No DungeonData to save.");
            return;
        }

        DungeonSaveData saveData = new DungeonSaveData();

        foreach (var cell in dungeonData.cells)
        {
            string prefabPath = AssetDatabase.GetAssetPath(cell.floorPrefab);
            string prefabGuid = AssetDatabase.AssetPathToGUID(prefabPath);

            saveData.cells.Add(new DungeonCellSave
            {
                x = cell.gridPosition.x,
                z = cell.gridPosition.y,
                prefabGuid = prefabGuid,

            });
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SaveFilePath, json);
        Debug.Log($"Dungeon saved to {SaveFilePath}");
    }

    private void LoadDungeonFromJson()
    {
        if (!File.Exists(SaveFilePath))
        {
            Debug.LogWarning("No dungeon file found to load.");
            return;
        }

        ClearDungeon();


        string json = File.ReadAllText(SaveFilePath);
        DungeonSaveData saveData = JsonUtility.FromJson<DungeonSaveData>(json);

        foreach (var cellSave in saveData.cells)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(cellSave.prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            //TBD fix this
            //AddCellAtCoordinate(cellSave.x, cellSave.z, prefab);
        }


    }
}
