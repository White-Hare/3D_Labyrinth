﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Labyrinth3D : Labyrinth2D
{
    [Range(2, 50)] public int layers = 10;
    protected new Cell[,,] cells;
    
    //Done
    private void OnValidate()
    {
        if (columns * rows > 35 * 35 && combineMeshes == true && !mergeCubes)
        {
            combineMeshes = false;
            Debug.LogError("Cannot combine meshes when labyrinth2D size is bigger than " + 35 * 35 + " .");
        }
    }

    protected override void GenerateMaze()
    {
        walls = GenerateWalls();
        cells = new Cell[columns, layers, rows];

        for (int z = 0; z < rows; z++)
            for (int y = 0; y < layers; y++)
              for (int x = 0; x < columns; x++) 
                  cells[x, y, z] = new Cell(x, y, z);


        List<Cell> closedCells = new List<Cell>();

        beginCell = new Cell(0, 0, 0);
        Cell cc = beginCell;


        walls.Remove(new Cell(1, 0, 1));//Beginning
        walls.Remove(new Cell(columns* 2 - 1, layers * 2, rows * 2 - 1));; //End

        closedCells.Add(cc);

        while (closedCells.Count < cells.Length)
        {
            Direction? direction = GetRandomDirection(ref cc, closedCells);
            if (direction == null) return;

            CarvePath(cc, (Direction)direction);

            cc = (Cell) cc.childCells.Peek();
            closedCells.Add(cc);
        }

        endCell = closedCells.Find(a => (a.x == columns - 1 && a.y == layers - 1 && a.z == rows - 1));

        BuildMaze();
    }

    //???Done???
    protected override void BuildMaze()
    {
        foreach (Cell w in walls)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.Translate(new Vector3(w.x - columns,
                w.y - layers,
                w.z - rows));

            cube.transform.parent = this.transform;
            cube.GetComponent<MeshRenderer>().material = wallMaterial;
        }


        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        else if(meshFilter.sharedMesh != null)
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
    }

    //Done
    protected override List<Cell> GenerateWalls()
    {
        List<Cell> walls = new List<Cell>();
        for (int z = 0; z < rows * 2 + 1; z++)
            for (int y = 0; y < layers * 2 + 1; y++)
                for (int x = 0; x < columns * 2 + 1; x++)
                    if(x % 2 == 0 || y % 2 == 0 || z % 2 == 0)
                        walls.Add(new Cell(x, y, z));

        return walls;

    }

    //Done
    protected override Direction? GetRandomDirection(ref Cell currentCell, List<Cell> closedCells)
    {
        List<Direction> avaliableDirections;
        int i = 0;

        do
        {
            if (closedCells.Count == cells.Length)
                return null;


            avaliableDirections = new List<Direction>(){Direction.FORWARD, Direction.BACK, Direction.RIGHT, Direction.LEFT, Direction.UP, Direction.DOWN};

            if (currentCell.x > columns - 2 || closedCells.Contains(cells[currentCell.x + 1, currentCell.y, currentCell.z]))
                avaliableDirections.Remove(Direction.RIGHT);

            if (currentCell.x < 1 || closedCells.Contains(cells[currentCell.x - 1, currentCell.y, currentCell.z]))
                avaliableDirections.Remove(Direction.LEFT);

            if (currentCell.z > rows - 2 || closedCells.Contains(cells[currentCell.x, currentCell.y, currentCell.z + 1]))
                avaliableDirections.Remove(Direction.FORWARD);

            if (currentCell.z < 1 || closedCells.Contains(cells[currentCell.x, currentCell.y, currentCell.z - 1]))
                avaliableDirections.Remove(Direction.BACK);

            if (currentCell.y > layers - 2 || closedCells.Contains(cells[currentCell.x, currentCell.y + 1, currentCell.z]))
                avaliableDirections.Remove(Direction.UP);

            if (currentCell.y < 1 || closedCells.Contains(cells[currentCell.x, currentCell.y - 1, currentCell.z]))
                avaliableDirections.Remove(Direction.DOWN);


            if (avaliableDirections.Count == 0)
            {
                currentCell = closedCells[closedCells.Count - i - 1];
                i++;
            }

        } while (avaliableDirections.Count == 0);


        int r = (int) (avaliableDirections.Count * Mathf.PerlinNoise(currentCell.x / (float)columns * noiseFrequency + noiseOffset, 
            currentCell.z / (float)rows  * noiseFrequency + noiseOffset));
        //int r = Random.Range(0, avaliableDirections.Count);

        return avaliableDirections[r];
    }

    //???Done???
    protected override void CarvePath(Cell currentCell, Direction direction)
    {
        switch (direction)
        {
            case Direction.RIGHT://Right
                currentCell.childCells.Push(cells[currentCell.x + 1, currentCell.y, currentCell.z]);
                walls.Remove(new Cell(currentCell.x * 2 + 2, currentCell.childCells.Peek().y * 2 + 1, currentCell.z * 2 + 1));
                break;

            case Direction.LEFT://Left
                currentCell.childCells.Push(cells[currentCell.x - 1, currentCell.y, currentCell.z]);
                walls.Remove(new Cell(currentCell.x * 2 + 1 - 1, currentCell.childCells.Peek().y * 2 + 1, currentCell.z * 2 + 1));
                break;

            case Direction.FORWARD://Front
                currentCell.childCells.Push(cells[currentCell.x, currentCell.y, currentCell.z + 1]);
                walls.Remove(new Cell(currentCell.x * 2 + 1, currentCell.childCells.Peek().y * 2 + 1, currentCell.z * 2 + 2));
                break;

            case Direction.BACK://Back
                currentCell.childCells.Push(cells[currentCell.x, currentCell.y, currentCell.z - 1]);
                walls.Remove(new Cell(currentCell.x * 2 + 1, currentCell.childCells.Peek().y * 2 + 1, currentCell.z * 2 - 1 + 1));
                break;

            case Direction.UP://UP
                currentCell.childCells.Push(cells[currentCell.x, currentCell.y + 1, currentCell.z]);
                walls.Remove(new Cell(currentCell.x * 2 + 1, currentCell.childCells.Peek().y * 2 + 1 - 1, currentCell.z * 2 + 1));
                break;

            case Direction.DOWN://DOWN
                currentCell.childCells.Push(cells[currentCell.x, currentCell.y - 1, currentCell.z]);
                walls.Remove(new Cell(currentCell.x * 2 + 1, currentCell.childCells.Peek().y * 2 + 1 + 1, currentCell.z * 2 + 1));
                break;
        }
    }

    //Done
    public Vector3 ToLabyrinthSpace(Labyrinth2D.Cell cell)
    {
        return this.transform.rotation * new Vector3(
            ((cell.x - this.columns / 2) * 2 + (columns + 1) % 2) * this.transform.localScale.x,
            ((cell.y - this.layers / 2) * 2 + (layers + 1) % 2) * this.transform.localScale.y,
            ((cell.z - this.rows / 2) * 2 + (rows + 1) % 2) * this.transform.localScale.z) + this.transform.position;
    }

    public override Vector3 ToLabyrinthSpace(Cell cell, float height = 0)
    {
        return ToLabyrinthSpace(cell);
    }
}
