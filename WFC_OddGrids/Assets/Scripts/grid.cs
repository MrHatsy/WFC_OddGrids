using UnityEngine;

public class grid : MonoBehaviour
{
   void Start()
    {
        // Create a 10x10 grid of squares of size 1
        var grid = new SquareGrid(1, new SquareBound(0, 0, 10, 10));
        // List all 100 cells
        var cells = grid.GetCells();
        // Print the centers of each cell
        foreach (var cell in cells)
        {
            Debug.Log($"{cell}: {grid.GetCellCenter(cell)}");
        }
    }

        void Update()
    {
        
    }
}
