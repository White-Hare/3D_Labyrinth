using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Rooms3D : Labyrinth3D
{
      [SerializeField] private int numberOfRooms = 3;
    private Transform solution;

    protected override void GenerateMaze()
    {
        base.GenerateMaze();


        int roomFrequency = rows * columns * layers / numberOfRooms;


        Stack<List<Cell>> rooms = new Stack<List<Cell>>();


        Color[] colors = new Color[]
            {Color.black, Color.cyan, Color.blue, Color.red, Color.magenta, Color.yellow, Color.grey};
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
        bool Carve(List<Cell> room, Cell cell, int xOffset = 0, int yOffset = 0, int zOffset = 0)
        {
            bool b = room.Contains(cell);
            if (b)
                walls.Remove(new Cell(cell.x * 2 + xOffset, cell.y * 2 + yOffset, cell.z * 2 + zOffset));

            return b;
        }

        bool IsCorner(List<Cell> room, Cell wall)
        {
            int xOdd = wall.x % 2;
            int yOdd = wall.y % 2;
            int zOdd = wall.z % 2;


            return !(room.Contains(new Cell(wall.x / 2 - 1 - xOdd, wall.y / 2 - 1 - yOdd, wall.z / 2 - 1 - zOdd)) &&
                     room.Contains(new Cell(wall.x / 2 - 1 - xOdd, wall.y / 2 - 1 - yOdd, wall.z / 2 - zOdd)) &&
                     room.Contains(new Cell(wall.x / 2 - xOdd, wall.y / 2 - 1 - yOdd, wall.z / 2 - 1 - zOdd)) &&
                     room.Contains(new Cell(wall.x / 2 - xOdd, wall.y / 2 - 1 - yOdd, wall.z / 2 - zOdd)) &&

                     room.Contains(new Cell(wall.x / 2 - 1 - xOdd, wall.y / 2 - yOdd, wall.z / 2 - 1 - zOdd)) &&
                     room.Contains(new Cell(wall.x / 2 - 1 - xOdd, wall.y / 2 - yOdd, wall.z / 2 - zOdd)) &&
                     room.Contains(new Cell(wall.x / 2 - xOdd, wall.y / 2 - yOdd, wall.z / 2 - 1 - zOdd)) &&
                     room.Contains(new Cell(wall.x / 2 - xOdd, wall.y / 2 - yOdd, wall.z / 2 - zOdd)));
        }

        void CarveSides(List<Cell> room, Cell cell, int xOffset = 0, int yOffset = 0, int zOffset = 0)
        {
            int x = (xOffset - 1);
            int y = (yOffset - 1);
            int z = (zOffset - 1);


            if (room.Contains(new Cell(cell.x + x, cell.y + y, cell.z + z)))
                Carve(room, cell, xOffset, yOffset, zOffset);

        }


        foreach (var room in rooms)
        foreach (Cell c in room)
        {
            bool xm = false, xp = false, ym = false, yp = false, zm = false, zp = false;

            //Faces
            if (c.x < columns - 1) xp = Carve(room, cells[c.x + 1, c.y, c.z], 0, 1, 1);
            if (c.x > 0) xm = Carve(room, cells[c.x - 1, c.y, c.z], 2, 1, 1);

            if (c.y < layers - 1) yp = Carve(room, cells[c.x, c.y + 1, c.z], 1, 0, 1);
            if (c.y > 0) ym = Carve(room, cells[c.x, c.y - 1, c.z], 1, 2, 1);

            if (c.z < rows - 1) zp = Carve(room, cells[c.x, c.y, c.z + 1], 1, 1, 0);
            if (c.z > 0) zm = Carve(room, cells[c.x, c.y, c.z - 1], 1, 1, 2);


            //Corners
            for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
            for (int z = 0; z < 2; z++)
            {
                bool xb = x > 0 ? c.x < columns - 1 : c.x > 0;
                bool yb = y > 0 ? c.y < layers - 1 : c.y > 0;
                bool zb = z > 0 ? c.z < rows - 1 : c.z > 0;

                if (xb && yb && zb && !IsCorner(room, new Cell(c.x * 2, c.y * 2, c.z * 2)))
                    Carve(room, cells[c.x + x, c.y + y, c.z + z]);
            }

            //Sides
            if(xm && ym && room.Contains(new Cell(c.x - 1, c.y - 1, c.z))) CarveSides(room, c, 0, 0, 1);
            if(zm && ym && room.Contains(new Cell(c.x, c.y - 1, c.z - 1))) CarveSides(room, c, 1, 0, 0);
            if(xp && ym && room.Contains(new Cell(c.x + 1, c.y - 1, c.z))) CarveSides(room, c, 2, 0, 1);
            if(zp && ym && room.Contains(new Cell(c.x, c.y - 1, c.z + 1))) CarveSides(room, c, 1, 0, 2);

            if(xm && yp && room.Contains(new Cell(c.x - 1, c.y + 1, c.z))) CarveSides(room, c, 0, 2, 1);
            if(zm && yp && room.Contains(new Cell(c.x, c.y + 1, c.z - 1))) CarveSides(room, c, 1, 2, 0);
            if(xp && yp && room.Contains(new Cell(c.x + 1, c.y + 1, c.z))) CarveSides(room, c, 2, 2, 1);
            if(zp && yp && room.Contains(new Cell(c.x, c.y + 1, c.z + 1))) CarveSides(room, c, 1, 2, 2);

            if(xm && zm && room.Contains(new Cell(c.x - 1, c.y, c.z - 1))) CarveSides(room, c, 0, 1, 0);
            if(xp && zm && room.Contains(new Cell(c.x + 1, c.y, c.z - 1))) CarveSides(room, c, 2, 1, 0);
            if(xm && zp && room.Contains(new Cell(c.x - 1, c.y, c.z + 1))) CarveSides(room, c, 0, 1, 2);
            if(xp && zp && room.Contains(new Cell(c.x + 1, c.y, c.z + 1))) CarveSides(room, c, 2, 1, 2);

        }




        showSolution = false; //For Safety
    }
}
