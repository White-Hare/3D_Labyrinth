using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Labyrinth2D _labyrinth2D;
    public float speed = 10f;

    private Labyrinth2D.Cell[] solution;
    private Vector3 currentTarget;
    private int currentTargetsIndex;
    private bool reverseMovement = false;



    private void Start()
    {
        Transform lt = _labyrinth2D.transform;

        transform.position = _labyrinth2D.ToLabyrinthSpace(_labyrinth2D.beginCell, transform.localScale.y);
        this.transform.rotation = lt.rotation;
        this.transform.localScale *= lt.localScale.x * 0.7f;


        Labyrinth2D.Func func = () => { solution = _labyrinth2D.GetSolution(false);};
        _labyrinth2D.TranslateMethodsToLabyrinthOrigin(func);

        currentTarget = _labyrinth2D.ToLabyrinthSpace(solution[0], transform.localScale.y);
        currentTargetsIndex = -1;
    }

    void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, currentTarget) < 0.5f)
            if ((currentTargetsIndex < solution.Length - 1 && !reverseMovement) || (currentTargetsIndex > 0 && reverseMovement))
            {
                if(!reverseMovement)
                    currentTarget = _labyrinth2D.ToLabyrinthSpace(solution[++currentTargetsIndex], transform.localScale.y);

                else
                    currentTarget = _labyrinth2D.ToLabyrinthSpace(solution[--currentTargetsIndex], transform.localScale.y);

            }
            else
                reverseMovement = !reverseMovement;
            

        transform.position = Vector3.MoveTowards(transform.position, currentTarget, speed * Time.deltaTime);
    }
}
