using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;


public class Labyrinth : MonoBehaviour
{
    public class Cell
    {
        public int x, y;
        public Stack<Cell> childCells;

        public Cell(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.childCells = new Stack<Cell>();
        }

        public override bool Equals(object obj)
        {
            var cell = obj as Cell;
            return cell != null &&
                   x == cell.x &&
                   y == cell.y;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    private enum Direction { FRONT, BACK, RIGHT, LEFT };

    [SerializeField] private string seed = "";

    [SerializeField] private bool combineMeshes = true;
    [SerializeField] private bool activateNavMeshForSurface = false;//For Using Unity NavMesh
    [SerializeField] private bool showSolution = false;

    public Material wallMaterial;
    public Material surfaceMaterial;

    [Range(2, 50)] public int rows = 10, columns = 10;


    private Cell[,] cells;
    private List<Cell> walls;
    private GameObject surface;

    private Cell[] solutionArray;


    [HideInInspector] public Cell beginCell;
    [HideInInspector] public Cell endCell;

    private void OnValidate()
    {
        if (rows * columns > 35 * 35)
        {
            combineMeshes = false;
            Debug.LogError("Cannot combine meshes when labyrinth size bigger than " + 35 * 35 + " .");
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

    //Script checks components. You don't need to clear again.
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

    private void GenerateMaze()
    {
        walls = GenerateWalls();
        cells = new Cell[rows, columns];

        for (int z = 0; z < columns; z++)
            for (int x = 0; x < rows; x++)
                cells[x, z] = new Cell(x, z);


        List<Cell> closedCells = new List<Cell>();

        beginCell = new Cell(0, 0);
        Cell cc = beginCell;


        walls.Remove(new Cell(cc.x * 2 + 1, cc.y * 2)); //Beginning
        walls.Remove(new Cell(cc.x * 2 + 1, cc.y * 2 + 1));
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


        endCell = closedCells.Find(a => (a.x == rows - 1 && a.y == columns - 1));

        BuildMaze();
    }

    private List<Direction> GetAvailableDirections(ref Cell currentCell, List<Cell> closedCells)
    {
        List<Direction> avaliableDirections;
        int i = 0;

        do
        {
            if (closedCells.Count == cells.Length)
                return null;


            avaliableDirections = new List<Direction> { Direction.FRONT, Direction.BACK, Direction.RIGHT, Direction.LEFT };

            if (currentCell.x < 1 || closedCells.Contains(cells[currentCell.x - 1, currentCell.y]))
                avaliableDirections.Remove(Direction.LEFT);

            if (currentCell.x > rows - 2 || closedCells.Contains(cells[currentCell.x + 1, currentCell.y]))
                avaliableDirections.Remove(Direction.RIGHT);

            if (currentCell.y > columns - 2 || closedCells.Contains(cells[currentCell.x, currentCell.y + 1]))
                avaliableDirections.Remove(Direction.FRONT);

            if (currentCell.y < 1 || closedCells.Contains(cells[currentCell.x, currentCell.y - 1]))
                avaliableDirections.Remove(Direction.BACK);

            if (avaliableDirections.Count == 0)
            {
                currentCell = closedCells[closedCells.Count - i - 1];
                i++;
            }

        } while (avaliableDirections.Count == 0);

        return avaliableDirections;
    }

    private void CarvePath(Cell currentCell, List<Direction> avaliableDirections)
    {

        switch (avaliableDirections[(int)(Random.value * avaliableDirections.Count)])
        {
            case Direction.RIGHT://Right
                currentCell.childCells.Push(cells[currentCell.x + 1, currentCell.y]);
                walls.Remove(new Cell(currentCell.x * 2 + 2, currentCell.y * 2 + 1));
                walls.Remove(new Cell(currentCell.childCells.Peek().x * 2 + 1, currentCell.childCells.Peek().y * 2 + 1));
                break;

            case Direction.LEFT://Left
                currentCell.childCells.Push(cells[currentCell.x - 1, currentCell.y]);
                walls.Remove(new Cell(currentCell.x * 2 + 1 - 1, currentCell.y * 2 + 1));
                walls.Remove(new Cell(currentCell.childCells.Peek().x * 2 + 1, currentCell.childCells.Peek().y * 2 + 1));
                break;

            case Direction.FRONT://Up
                currentCell.childCells.Push(cells[currentCell.x, currentCell.y + 1]);
                walls.Remove(new Cell(currentCell.x * 2 + 1, currentCell.y * 2 + 2));
                walls.Remove(new Cell(currentCell.childCells.Peek().x * 2 + 1, currentCell.childCells.Peek().y * 2 + 1));
                break;

            case Direction.BACK://Down
                currentCell.childCells.Push(cells[currentCell.x, currentCell.y - 1]);
                walls.Remove(new Cell(currentCell.x * 2 + 1, currentCell.y * 2 - 1 + 1));
                walls.Remove(new Cell(currentCell.childCells.Peek().x * 2 + 1, currentCell.childCells.Peek().y * 2 + 1));
                break;
        }
    }

    private List<Cell> GenerateWalls()
    {
        List<Cell> walls = new List<Cell>();
        for (int z = 0; z < (columns) * 2 + 1; z++)
            for (int x = 0; x < (rows) * 2 + 1; x++)
                walls.Add(new Cell(x, z));

        return walls;
    }

    private void BuildMaze()
    {
        foreach (Cell w in walls)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.Translate(new Vector3(w.x - rows,
                                      transform.localScale.y / 2,
                                      w.y - columns));

            cube.transform.parent = this.transform;
            cube.GetComponent<MeshRenderer>().material = wallMaterial;
        }


        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        else if(meshFilter.sharedMesh != null)
            meshFilter.sharedMesh.Clear();

        if (combineMeshes)
        {
            MeshCollider collider = gameObject.GetComponent<MeshCollider>();
            if(collider != null)
                DestroyImmediate(collider);
                
            gameObject.AddComponent<MeshCollider>();

            CombineMeshes(gameObject);
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
        surface.transform.localScale = new Vector3(rows / 10f * 2, 1, columns / 10f * 2);
        surface.transform.parent = this.transform;
        surface.GetComponent<Renderer>().material = surfaceMaterial;
    }

    private void CombineMeshes(GameObject gameObject)
    {
        //Quaternion oldRotation = transform.rotation;
        //Vector3 oldPosition = transform.position;
        //Vector3 oldScale = transform.localScale;

        //transform.rotation = Quaternion.identity;
        //transform.position = Vector3.zero;
        //transform.localScale = Vector3.one;


        MeshFilter[] filters = gameObject.GetComponentsInChildren<MeshFilter>();

        CombineInstance[] combiners = new CombineInstance[filters.Length];

        for (int i = 0; i < filters.Length; i++)
        {
            if (filters[i] == null) continue;

            combiners[i].subMeshIndex = 0;
            combiners[i].mesh = filters[i].sharedMesh;
            combiners[i].transform = filters[i].transform.localToWorldMatrix;
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

        void Show_Solution()
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
            Show_Solution();

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
                    //Debug.Log("ADD: " + cc.x + " - " + cc.y);
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
                    //Debug.Log("REMOVE: " + checkPoints.Peek().x + " - " + checkPoints.Peek().y);
                    cc = checkPoints.Pop().childCells.Pop();
                }
            }


            //Debug.Log(cc.x + " - " + cc.y);
        }

        solution.Push(cc);


        solutionArray = new Cell[solution.Count];
        int i = 1;



        while (solution.Count > 0)
        {
            cc = solution.Pop();
            solutionArray[solutionArray.Length - i++] = cc;


            Show_Solution();
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

    public Vector3 ToLabyrinthSpace(Labyrinth.Cell cell, float height = 0f)
    {
        return this.transform.rotation * new Vector3(((cell.x - this.rows / 2) * 2 + (rows + 1) % 2) * this.transform.localScale.x,
                   height / 2, ((cell.y - this.columns / 2) * 2 + (columns + 1) % 2) * this.transform.localScale.y) + this.transform.position;
    }
}
