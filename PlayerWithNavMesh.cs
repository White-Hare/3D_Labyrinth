using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerWithNavMesh : MonoBehaviour
{
    [SerializeField] private Labyrinth2D labyrinth = null;
    private UnityEngine.AI.NavMeshAgent agent;

    
    private void Start()
    {
        transform.position = labyrinth.ToLabyrinthSpace(labyrinth.beginCell, transform.localScale.y);
        this.transform.rotation = labyrinth.transform.rotation;
        this.transform.localScale *= labyrinth.transform.localScale.x * 0.7f;

        agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = 20;
        agent.angularSpeed = 20;
        agent.acceleration = 20;

        try
        {
            agent.SetDestination(labyrinth.ToLabyrinthSpace(labyrinth.endCell, transform.localScale.y));
        }
        catch (Exception e)
        {
            Debug.LogError("Labyrinth size too small for \"Player\".");
            throw new Exception(e.Message);
        }
    }

    private void FixedUpdate()
    {

        if (Vector3.Distance(labyrinth.ToLabyrinthSpace(labyrinth.endCell), this.transform.position) < 0.5f)
            agent.SetDestination(labyrinth.ToLabyrinthSpace(labyrinth.endCell, transform.localScale.y));

        else if (Vector3.Distance(labyrinth.ToLabyrinthSpace(labyrinth.beginCell), this.transform.position) < 0.5f)
            agent.SetDestination(labyrinth.ToLabyrinthSpace(labyrinth.beginCell, transform.localScale.y));
    }
}
