using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DungeonDesignerWindow : EditorWindow
{

    private DungeonData dungeonData;

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


        if (GUILayout.Button("Press Me")) 
        {
            Debug.Log("Button pressed");

            Debug.Log($"there are {dungeonData.cells.Count} cells in the dungeon");
        }

        if (GUILayout.Button("Add cell"))
        {
            Debug.Log("add cell pressed");
        }
        
    }
}
