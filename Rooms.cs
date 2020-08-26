using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Rooms : Labyrinth2D
{
    protected override void GenerateMaze()
    {
        base.GenerateMaze();
        showSolution = false; //For Safety


        Stack<List<Cell>> rooms = new Stack<List<Cell>>();


        void CreateRoom(Cell currentCell)
        {
            List<Cell> room = new List<Cell>();
            rooms.Push(room);


            while (currentCell != null)
            {
                room.Add(currentCell);

                if (currentCell.childCells.Count > 1)
                    while (currentCell.childCells.Count > 0)
                        CreateRoom(currentCell.childCells.Pop());

                if (currentCell.childCells.Count == 0)
                    break;
                else
                    currentCell = currentCell.childCells.Pop();
            }
        }

        CreateRoom(beginCell);

        Debug.Log(rooms.Count);


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
    }
}
