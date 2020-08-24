using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;


public class Labyrinth2D : MonoBehaviour
{
    public class Cell
    {
        public int x, y, z;
        public Stack<Cell> childCells;

        public Cell(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.childCells = new Stack<Cell>();
        }

        public Cell(int x, int z)
        {
            this.x = x;
            this.y = 0;
            this.z = z;
            this.childCells = new Stack<Cell>();
        }

        public override bool Equals(object obj)
        {
            var cell = obj as Cell;
            return cell != null &&
                   x == cell.x &&
                   y == cell.y &&
                   z == cell.z;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public Cell Copy()
        {
            Cell c = new Cell(x, y, z);
            c.childCells = this.childCells;
            return c;
        }
    }
    protected enum Direction { FORWARD, BACK, RIGHT, LEFT, UP, DOWN};

    [SerializeField] private string seed = "";

    public Material wallMaterial;
    public Material surfaceMaterial;

    [SerializeField] private bool activateNavMeshForSurface = false;//For Using Unity NavMesh
    [SerializeField] private bool showSolution = false;
    [SerializeField] protected bool combineMeshes = true;

    [SerializeField] protected bool mergeCubes = true;

    [Range(2, 70)] public int columns = 10, rows = 10;


    protected Cell[,] cells;
    protected List<Cell> walls;
    private GameObject surface;

    private Cell[] solutionArray;


    [HideInInspector] public Cell beginCell;
    [HideInInspector] public Cell endCell;

    private void OnValidate()
    {


        if (columns * rows > 35 * 35 && combineMeshes == true && !mergeCubes)
        {
            //combineMeshes = false;
            //Debug.LogError("Cannot combine meshes when labyrinth2D size is bigger than " + 35 * 35 + " .");
            Debug.LogWarning("Labyrinth may exceed maximum vertices count.");
        }
    }

    private void Awake()
    {
        Clear();
        Generate();
    }
        
    public void Generate()//Awake
    {
        Random.InitState(seed.GetHashCode());

        Func func = () => { GenerateMaze(); GetSolution(false); };//You can delete "GetSolution()" if you want.
        TranslateMethodsToLabyrinthOrigin(func);


        if (activateNavMeshForSurface)
        {
            NavMeshSurface nav = surface.gameObject.AddComponent<NavMeshSurface>();
            nav.BuildNavMesh();
        }

        if (showSolution) 
            GetSolution(true);
    }

    public void Clear()
    {
        Transform childs = gameObject.transform; 
        while(childs.childCount > 0)
            DestroyImmediate(childs.GetChild(childs.childCount - 1).gameObject);

            

        MeshFilter filter = GetComponent<MeshFilter>();
        MeshRenderer renderer= GetComponent<MeshRenderer>();
        MeshCollider collider= GetComponent<MeshCollider>();

        if(filter != null)
            DestroyImmediate(filter);
        if(renderer != null)
            DestroyImmediate(renderer);
        if(collider != null)
            DestroyImmediate(collider);

        solutionArray = null;
    }

    protected virtual void GenerateMaze()
    {
        walls = GenerateWalls();
        cells = new Cell[columns, rows];

        for (int z = 0; z < rows; z++)
            for (int x = 0; x < columns; x++)
                cells[x, z] = new Cell(x, z);


        List<Cell> closedCells = new List<Cell>();

        beginCell = new Cell(0, 0);
        Cell cc = beginCell;


        walls.Remove(new Cell(cc.x * 2 + 1, cc.z * 2)); //Beginning
        walls.Remove(new Cell(cc.x * 2 + 1, cc.z * 2 + 1));
        walls.RemoveAt(walls.Count - 2);//End + 1

        closedCells.Add(cc);

        while (closedCells.Count < cells.Length)
        {
            List<Direction> avaliableDirections = GetAvailableDirections(ref cc, closedCells);
            if (avaliableDirections == null) return;

            CarvePath(cc, avaliableDirections);

            cc = (Cell)cc.childCells.Peek();
            closedCells.Add(cc);
        }


        endCell = closedCells.Find(a => (a.x == columns - 1 && a.z == rows - 1));

        BuildMaze();
    }

    protected virtual List<Direction> GetAvailableDirections(ref Cell currentCell, List<Cell> closedCells)
    {
        List<Direction> avaliableDirections;
        int i = 0;

        do
        {
            if (closedCells.Count == cells.Length)
                return null;


            avaliableDirections = new List<Direction>(){Direction.FORWARD, Direction.BACK, Direction.RIGHT, Direction.LEFT};

            if (currentCell.x < 1 || closedCells.Contains(cells[currentCell.x - 1, currentCell.z]))
                avaliableDirections.Remove(Direction.LEFT);

            if (currentCell.x > columns - 2 || closedCells.Contains(cells[currentCell.x + 1, currentCell.z]))
                avaliableDirections.Remove(Direction.RIGHT);

            if (currentCell.z > rows - 2 || closedCells.Contains(cells[currentCell.x, currentCell.z + 1]))
                avaliableDirections.Remove(Direction.FORWARD);

            if (currentCell.z < 1 || closedCells.Contains(cells[currentCell.x, currentCell.z - 1]))
                avaliableDirections.Remove(Direction.BACK);

            if (avaliableDirections.Count == 0)
            {
                currentCell = closedCells[closedCells.Count - i - 1];
                i++;
            }

        } while (avaliableDirections.Count == 0);

        return avaliableDirections;
    }

    protected virtual void CarvePath(Cell currentCell, List<Direction> avaliableDirections)
    {
        //int r = (int) (avaliableDirections.Count * Mathf.PerlinNoise(currentCell.x / 4f, currentCell.z / 4f));
        int r = Random.Range(0, avaliableDirections.Count);
        switch (avaliableDirections[r])
        {
            case Direction.RIGHT://Right
                currentCell.childCells.Push(cells[currentCell.x + 1, currentCell.z]);
                walls.Remove(new Cell(currentCell.x * 2 + 2, currentCell.z * 2 + 1));
                break;

            case Direction.LEFT://Left
                currentCell.childCells.Push(cells[currentCell.x - 1, currentCell.z]);
                walls.Remove(new Cell(currentCell.x * 2 + 1 - 1, currentCell.z * 2 + 1));
                break;

            case Direction.FORWARD://Front
                currentCell.childCells.Push(cells[currentCell.x, currentCell.z + 1]);
                walls.Remove(new Cell(currentCell.x * 2 + 1, currentCell.z * 2 + 2));
                break;

            case Direction.BACK://Back
                currentCell.childCells.Push(cells[currentCell.x, currentCell.z - 1]);
                walls.Remove(new Cell(currentCell.x * 2 + 1, currentCell.z * 2 - 1 + 1));
                break;
        }

        walls.Remove(new Cell(currentCell.childCells.Peek().x * 2 + 1, currentCell.childCells.Peek().z * 2 + 1));
    }

    protected virtual List<Cell> GenerateWalls()
    {
        List<Cell> walls = new List<Cell>();
        for (int z = 0; z < (rows) * 2 + 1; z++)
            for (int x = 0; x < (columns) * 2 + 1; x++)
                walls.Add(new Cell(x, z));

        return walls;
    }

    protected virtual void BuildMaze()
    {
        foreach (Cell w in walls)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.Translate(new Vector3(w.x - columns,
                transform.localScale.y / 2,
                w.z - rows));

            cube.transform.parent = this.transform;
            cube.GetComponent<MeshRenderer>().material = wallMaterial;
        }


        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        else if (meshFilter.sharedMesh != null)
            meshFilter.sharedMesh.Clear();



        if (mergeCubes)
        {

            int i = 0;
            while (i < gameObject.transform.childCount)
            {
                //if (gameObject.transform.GetChild(i).name == gameObject.name) continue;

                int j = i + 1;
                while (j < gameObject.transform.childCount)
                {
                    GameObject m = gameObject.transform.GetChild(i).gameObject;
                    if (MergeCubes(ref m, gameObject.transform.GetChild(j).gameObject))
                    {
                        DestroyImmediate(gameObject.transform.GetChild(j).gameObject);
                        //i--;
                        //break;
                    }

                    else j++;
                }

                i++;
            }
        }




        if (combineMeshes)
        {
            MeshCollider collider = gameObject.GetComponent<MeshCollider>();
            if (collider != null)
                DestroyImmediate(collider);

            CombineMeshes(gameObject);
            gameObject.AddComponent<MeshCollider>();
        }
        else
        {
            DestroyImmediate(GetComponent<MeshRenderer>());
            DestroyImmediate(GetComponent<MeshFilter>());
            DestroyImmediate(GetComponent<MeshCollider>());
        }


        //Don't add plane to combined meshes
        surface = GameObject.CreatePrimitive(PrimitiveType.Plane);
        surface.name = "Surface";
        surface.transform.localScale = new Vector3(columns / 10f * 2, 1, rows / 10f * 2);
        surface.transform.parent = this.transform;
        surface.GetComponent<MeshRenderer>().material = surfaceMaterial;
    }

    protected bool MergeCubes(ref GameObject c1, GameObject c2)
    {

        Vector3 d = c2.transform.localPosition - c1.transform.localPosition;
        if (d.magnitude * 2f != (c1.transform.localScale.x + c2.transform.localScale.x) &&
            d.magnitude * 2f != (c1.transform.localScale.y + c2.transform.localScale.y) &&
            d.magnitude * 2f != (c1.transform.localScale.z + c2.transform.localScale.z) ) return false;


        Vector3 scale1 = c1.gameObject.transform.localScale;
        Vector3 scale2 = c2.gameObject.transform.localScale;

        if      (Mathf.Abs(d.x) * 2 == (scale1.x + scale2.x) && scale1.y == scale2.y && scale1.z == scale2.z)
            scale1.x = c1.transform.localScale.x + c2.transform.localScale.x;
        else if (Mathf.Abs(d.y) * 2 == (scale1.y + scale2.y) && scale1.x == scale2.x && scale1.z == scale2.z) 
            scale1.y = c1.transform.localScale.y + c2.transform.localScale.y;
        else if (Mathf.Abs(d.z) * 2 == (scale1.z + scale2.z) && scale1.x == scale2.x && scale1.y == scale2.y)
            scale1.z = c1.transform.localScale.z + c2.transform.localScale.z;
        else return false;


        float mass1 = c1.transform.localScale.x * c1.transform.localScale.y * c1.transform.localScale.z;
        float mass2 = c2.transform.localScale.x * c2.transform.localScale.y * c2.transform.localScale.z;


        c1.gameObject.transform.localPosition = (c1.transform.localPosition * mass1  + c2.transform.localPosition * mass2) / (mass1 + mass2);
        c1.gameObject.transform.localScale = scale1;

        return true;

    }

    protected void CombineMeshes(GameObject gameObject)
    {
        //Quaternion oldRotation = transform.rotation;
        //Vector3 oldPosition = transform.position;
        //Vector3 oldScale = transform.localScale;

        //transform.rotation = Quaternion.identity;
        //transform.position = Vector3.zero;
        //transform.localScale = Vector3.one;


        List<MeshFilter> meshFilters = new List<MeshFilter>(gameObject.GetComponentsInChildren<MeshFilter>());
        CombineInstance[] combiners = new CombineInstance[meshFilters.Count];

        for (int i = 0; i < meshFilters.Count; i++)
        {
            if (meshFilters[i] == null) continue;


            combiners[i].subMeshIndex = 0;
            combiners[i].mesh = meshFilters[i].sharedMesh;
            combiners[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        Mesh finalMesh = new Mesh();

        finalMesh.CombineMeshes(combiners);
        
 
        
        MeshFilter gameObjectMF = gameObject.GetComponent<MeshFilter>();
        if (gameObjectMF == null)
            gameObjectMF = gameObject.AddComponent<MeshFilter>();

        gameObjectMF.mesh = finalMesh;

        
        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider != null)
            collider.sharedMesh = finalMesh;


        MeshRenderer gameObjectMR = gameObject.GetComponent<MeshRenderer>();
        if (gameObjectMR == null)
            gameObjectMR = gameObject.AddComponent<MeshRenderer>();

        gameObjectMR.sharedMaterial = wallMaterial;//filters[0].GetComponent<MeshRenderer>().sharedMaterial;


        //transform.rotation = oldRotation;
        //transform.position = oldPosition;
        //transform.localScale = oldScale;



        while (0 < gameObject.transform.childCount)
            DestroyImmediate(gameObject.transform.GetChild(gameObject.transform.childCount - 1).gameObject);
        
    }

    public Cell[] GetSolution(bool showSolution = false)
    {
        Transform parent = null;
        if (showSolution)
        {
            parent = new GameObject("Solution").transform;
            parent.parent = this.transform;
        }

        void ShowSolution()
        {
            if (showSolution)
            {
                GameObject sphere = null;


                foreach (Cell s in solutionArray)
                {


                    sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                    sphere.transform.parent = parent;
                    sphere.transform.localScale = 0.6f * this.transform.localScale;
                    sphere.transform.localPosition = ToLabyrinthSpace(s, sphere.transform.localScale.y);

                    sphere.GetComponent<Collider>().isTrigger = true;

                }

                sphere.GetComponent<MeshRenderer>().sharedMaterial.color = Color.red;
            }
        }

        if (solutionArray != null)
        {
            ShowSolution();

            return solutionArray;
        }

        Stack<Cell> solution = new Stack<Cell>();

        Cell cc = beginCell;
        Stack<Cell> checkPoints = new Stack<Cell>();


        while (!cc.Equals(endCell) && !(cc.childCells.Count < 1 && checkPoints.Count < 1))
        {
            if (cc.childCells.Count >= 1)
            {
                if (cc.childCells.Count > 1)
                {
                    checkPoints.Push(cc);
                    //Debug.Log("ADD: " + cc.x + " - " + cc.z);
                }
                solution.Push(cc);
                cc = cc.childCells.Pop();
            }

            else if (cc.childCells.Count == 0 && checkPoints.Count > 0)
            {
                while (!checkPoints.Peek().Equals(solution.Peek())) { solution.Pop(); }


                if (checkPoints.Peek().childCells.Count > 1)
                    cc = checkPoints.Peek().childCells.Pop();

                else
                {
                    //Debug.Log("REMOVE: " + checkPoints.Peek().x + " - " + checkPoints.Peek().z);
                    cc = checkPoints.Pop().childCells.Pop();
                }
            }


            //Debug.Log(cc.x + " - " + cc.z);
        }

        solution.Push(cc);


        solutionArray = new Cell[solution.Count];
        int i = 1;



        while (solution.Count > 0)
        {
            cc = solution.Pop();
            solutionArray[solutionArray.Length - i++] = cc;


            ShowSolution();
        }

        //CombineMeshes(parent.gameObject);
        return solutionArray;
    }

    public delegate void Func();

    public void TranslateMethodsToLabyrinthOrigin(params Func[] functions){
        Vector3 oldPosition = transform.localPosition;
        Quaternion oldRotation = transform.rotation;
        Vector3 oldScale = transform.localScale;

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        foreach (Func f in functions)
            f();

        transform.position = oldPosition;
        transform.rotation = oldRotation;
        transform.localScale = oldScale;
    }

    public virtual Vector3 ToLabyrinthSpace(Labyrinth2D.Cell cell, float height = 0)
    {
        return this.transform.rotation * new Vector3(((cell.x - this.columns / 2) * 2 + (columns + 1) % 2) * this.transform.localScale.x,
                   height / 2,
                   ((cell.z - this.rows / 2) * 2 + (rows + 1) % 2) * this.transform.localScale.z) + this.transform.position;
    }
}