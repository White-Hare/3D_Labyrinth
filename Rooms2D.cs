using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Rooms2D : Labyrinth2D
{
    [SerializeField] private int numberOfRooms = 3;
    private Transform solution;

    protected override void GenerateMaze()
    {
        base.GenerateMaze();


        int roomFrequency = rows * columns / numberOfRooms;
          

        Stack<List<Cell>> rooms = new Stack<List<Cell>>();


        Color[] colors = new Color[]{Color.black, Color.cyan, Color.blue, Color.red, Color.magenta, Color.yellow};
        Color color = colors[0];

        if (showSolution)
            solution = new GameObject("Solution").transform;

        int i = 0;
        void CreateRoom(Cell currentCell, List<Cell> room = null)
        {
            while (currentCell != null)
            {
                if (i++ % roomFrequency == 0)
                { 
                    room = new List<Cell>();
                    rooms.Push(room);
                }


                room.Add(currentCell);

                if (currentCell.childCells.Count > 1)
                    while (currentCell.childCells.Count > 0)
                        CreateRoom(currentCell.childCells.Pop(), room);

                if (currentCell.childCells.Count == 0)
                    break;

                if (showSolution)
                {
                    GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    s.transform.parent = solution;
                    s.transform.position = ToLabyrinthSpace(currentCell, s.transform.lossyScale.y);
                    s.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
                    s.GetComponent<MeshRenderer>().sharedMaterial.color = colors[rooms.Count % colors.Length];
                }


                currentCell = currentCell.childCells.Pop();
            }
        }

        CreateRoom(beginCell);

        

        //Create Room
        void Carve(List<Cell> room, Cell cell, int xOffset = 0, int yOffset = 0)
        {
            if (room.Contains(cell))
                walls.Remove(new Cell(cell.x * 2 + xOffset, cell.z * 2 + yOffset));
        }

        bool IsSide(List<Cell> room, Cell wall)
        {
            int xOdd = wall.x % 2;
            int zOdd = wall.z % 2;


            return !(room.Contains(new Cell(wall.x / 2 - 1 - xOdd, wall.z / 2 - 1 - zOdd)) &&
                     room.Contains(new Cell(wall.x / 2 - 1 - xOdd, wall.z / 2 - zOdd)) &&
                     room.Contains(new Cell(wall.x / 2 - xOdd, wall.z / 2 - 1 - zOdd)) &&
                     room.Contains(new Cell(wall.x / 2 - xOdd, wall.z / 2 - zOdd)));
        }


        foreach (var room in rooms)
        foreach (Cell c in room)
        {
            
            if (c.x < columns - 1) Carve(room, cells[c.x + 1, c.z], 0, 1);
            if (c.x > 0) Carve(room, cells[c.x - 1, c.z], 2, 1);
            if (c.z < rows - 1) Carve(room, cells[c.x, c.z + 1], 1, 0);
            if (c.z > 0) Carve(room, cells[c.x, c.z - 1], 1, 2);



            if (c.x > 0 && c.z > 0 && !IsSide(room, new Cell((c.x - 1) * 2, (c.z - 1) * 2)))
                Carve(room, cells[c.x - 1, c.z - 1]);
            if (c.x > 0 && c.z < rows - 1 && !IsSide(room, new Cell((c.x - 1) * 2, (c.z + 1) * 2)))
                Carve(room, cells[c.x - 1, c.z + 1]);
            if (c.x < columns - 1 && c.z > 0 && !IsSide(room, new Cell((c.x + 1) * 2, (c.z - 1) * 2)))
                Carve(room, cells[c.x + 1, c.z - 1]);
            if (c.x < columns - 1 && c.z < rows - 1 && !IsSide(room, new Cell((c.x + 1) * 2, (c.z + 1) * 2)))
                Carve(room, cells[c.x + 1, c.z + 1]);
        }



        showSolution = false; //For Safety
    }
}
