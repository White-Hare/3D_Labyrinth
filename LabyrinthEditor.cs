using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Labyrinth2D) , true)]
[CanEditMultipleObjects]
public class LabyrinthEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Labyrinth2D labyrinth2D = target as Labyrinth2D;


        if (GUILayout.Button("Preview Labyrinth"))
        {
            labyrinth2D.Clear();
            labyrinth2D.Generate();
        }


        if (GUILayout.Button("Clear"))
            (target as Labyrinth2D).Clear(); 
     
        
    }

}
