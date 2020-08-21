using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(Labyrinth))]
public class LabyrinthEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        if (GUILayout.Button("Preview Labyrinth"))
        {
            Labyrinth labyrinth = target as Labyrinth;
            labyrinth.Clear();
            labyrinth.Generate();
        }


        if (GUILayout.Button("Clear"))
            (target as Labyrinth).Clear(); 
     
        
    }

}
