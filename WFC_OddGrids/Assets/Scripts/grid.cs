using Sylves;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class grid : MonoBehaviour
{
    // Create a 10x10 grid of squares of size 1
    IGrid grid = new SquareGrid(1, new SquareBound(0, 0, 10, 10));
    // List all 100 cells
    HashSet<Cell> cells = grid.GetCells();
    // Print the centers of each cell

   void Start()
    {    
        foreach (var cell in cells)
        {
            Debug.Log($"{cell}: {grid.GetCellCenter(cell)}");
        }

        DrawCells();
    }

        void Update()
    {
        
    }

    // void DrawCells(){
    //     foreach (var cell in cells)
    //     {
    //         // Read the color from the canvas

    //         // Read the vertices of the cell
    //         var verticies = grid.GetPoygon(cell);
    //         //Split up the polygon into tris, and draw each one of the



    //     }
    // }
}
