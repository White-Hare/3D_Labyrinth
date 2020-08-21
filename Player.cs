using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Labyrinth labyrinth;
    public float speed = 10f;

    private Labyrinth.Cell[] solution;
    private Vector3 currentTarget;
    private int currentTargetsIndex;
    private bool reverseMovement = false;



    private void Start()
    {
        Transform lt = labyrinth.transform;

        transform.position = labyrinth.ToLabyrinthSpace(labyrinth.beginCell, transform.localScale.y);
        this.transform.rotation = lt.rotation;
        this.transform.localScale *= lt.localScale.x * 0.7f;


        Labyrinth.Func func = () => { solution = labyrinth.GetSolution(false);};
        labyrinth.TranslateMethodsToLabyrinthOrigin(func);

        currentTarget = labyrinth.ToLabyrinthSpace(solution[0], transform.localScale.y);
        currentTargetsIndex = -1;
    }

    void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, currentTarget) < 0.5f)
            if ((currentTargetsIndex < solution.Length - 1 && !reverseMovement) || (currentTargetsIndex > 0 && reverseMovement))
            {
                if(!reverseMovement)
                    currentTarget = labyrinth.ToLabyrinthSpace(solution[++currentTargetsIndex], transform.localScale.y);

                else
                    currentTarget = labyrinth.ToLabyrinthSpace(solution[--currentTargetsIndex], transform.localScale.y);

            }
            else
                reverseMovement = !reverseMovement;
            

        transform.position = Vector3.MoveTowards(transform.position, currentTarget, speed * Time.deltaTime);
    }
}
