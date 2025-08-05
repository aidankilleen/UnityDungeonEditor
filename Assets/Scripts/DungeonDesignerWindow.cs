using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class DungeonDesignerWindow : EditorWindow
{

    private DungeonData dungeonData;
    private GameObject floorPrefab;

    private int xCoord;
    private int zCoord;
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

        GameObject newPrefab = (GameObject)EditorGUILayout.ObjectField("Floor Prefab", floorPrefab, typeof(GameObject), false);
        if (newPrefab != floorPrefab)
        {
            floorPrefab = newPrefab;
            if (floorPrefab != null)
                cellSize = GetPrefabSize(floorPrefab);
        }

        cellSize = EditorGUILayout.FloatField("Cell Size", cellSize);

        GUILayout.Space(10);
        GUILayout.Label("Add Cell by Coordinates", EditorStyles.boldLabel);
        xCoord = EditorGUILayout.IntField("X Coordinate", xCoord);
        zCoord = EditorGUILayout.IntField("Z Coordinate", zCoord);

        if (GUILayout.Button("Press Me")) 
        {
            Debug.Log("Button pressed");

            Debug.Log($"there are {dungeonData.cells.Count} cells in the dungeon");
        }

        if (GUILayout.Button("Add Cell"))
        {
            AddCellAtCoordinates();
        }

        GUILayout.Space(10);
        // Reset Cells Button
        if (GUILayout.Button("Reset All Cells"))
        {
            if (EditorUtility.DisplayDialog("Reset Dungeon",
                "Are you sure you want to delete all cells from the scene?",
                "Yes", "Cancel"))
            {
                ResetCells();
            }
        }

        GUILayout.Space(10);

        GUILayout.Space(10);
        GUILayout.Label("Dungeon File", EditorStyles.boldLabel);
        dungeonFileName = EditorGUILayout.TextField("Filename (no extension)", dungeonFileName);

        if (GUILayout.Button("Save Dungeon to JSON"))
        {
            SaveDungeonToJson();
        }

        if (GUILayout.Button("Load Dungeon from JSON"))
        {
            LoadDungeonFromJson();
        }


    }
    private void AddCellAtCoordinates()
    {
        if (dungeonData == null || floorPrefab == null)
        {
            Debug.LogWarning("Please assign DungeonData and FloorPrefab first!");
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
                floorPrefab = floorPrefab
            });

            // Instantiate the prefab in the scene
            GameObject parent = GetOrCreateDungeonParent();
            GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(floorPrefab);
            tile.transform.SetParent(parent.transform);
            tile.transform.position = new Vector3(gridPos.x * cellSize, 0, gridPos.y * cellSize);
            Undo.RegisterCreatedObjectUndo(tile, "Add Cell");

            EditorUtility.SetDirty(dungeonData);
            Debug.Log($"Added cell at {gridPos}");
        }
        else
        {
            Debug.LogWarning($"Cell at {gridPos} already exists!");
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

    private void ResetCells()
    {
        if (dungeonData == null)
        {
            Debug.LogWarning("DungeonData is not assigned!");
            return;
        }

        // Delete all instantiated cells in the scene
        foreach (var cell in dungeonData.cells)
        {
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                if (PrefabUtility.GetCorrespondingObjectFromSource(obj) == cell.floorPrefab)
                {
                    Undo.DestroyObjectImmediate(obj);
                }
            }
        }

        // Clear the list of cells
        dungeonData.cells.Clear();
        EditorUtility.SetDirty(dungeonData);
        Debug.Log("All dungeon cells have been reset.");
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
                prefabGuid = prefabGuid
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

        ResetCells();

        string json = File.ReadAllText(SaveFilePath);
        DungeonSaveData saveData = JsonUtility.FromJson<DungeonSaveData>(json);

        foreach (var cellSave in saveData.cells)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(cellSave.prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogWarning($"Prefab with GUID {cellSave.prefabGuid} not found. Skipping cell.");
                continue;
            }

            dungeonData.cells.Add(new DungeonCell
            {
                gridPosition = new Vector2Int(cellSave.x, cellSave.z),
                floorPrefab = prefab
            });

            GameObject parent = GetOrCreateDungeonParent();
            GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            tile.transform.SetParent(parent.transform);
            tile.transform.position = new Vector3(cellSave.x * cellSize, 0, cellSave.z * cellSize);
            Undo.RegisterCreatedObjectUndo(tile, "Load Cell");
        }

        EditorUtility.SetDirty(dungeonData);
        Debug.Log("Dungeon loaded from JSON.");
    }


}
