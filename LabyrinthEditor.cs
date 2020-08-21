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
        EditorUtility.SetDirty(this.target);

        if (GUILayout.Button("Generate Labyrinth"))
        {
            (target as Labyrinth).Clear();
            (target as Labyrinth).Generate();
        }

        if (GUILayout.Button("Generate Solution"))
        {
            Labyrinth mLabyrinth = target as Labyrinth;
            if (mLabyrinth.HasGenerated)
                mLabyrinth.GetSolution(true);
            else
                Debug.Log("First Generate Labyrinth");
            
        }

        if (GUILayout.Button("Clear"))
            (target as Labyrinth).Clear(); 
     
        
    }

}
