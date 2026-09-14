using UnityEngine;
using Sylves;
using System.Collections.Generic;


public class LangtonsAnt : MonoBehaviour
{
    private static SquareGrid grid = new SquareGrid(1f);
    private HashSet<Cell> blackCells = new HashSet<Cell>();
    private static Cell startCell = new Cell(0, 0);
    private static SquareDir startDir = SquareDir.Up;
    private Walker ant = new Walker(grid, startCell, (CellDir)startDir);


    void Start()
    {
        
    }

    void Update()
    {
        Walk();
    }

    void Walk()
    {
        Debug.Log($"Ant is at {ant.Cell} and is facing {(SquareDir)ant.Dir}");
        ant.MoveForward();
        bool isCurrentCellBlack = blackCells.Contains(ant.Cell);
        if( isCurrentCellBlack )
        {
            ant.TurnRight();
            blackCells.Remove(ant.Cell);
        }
        else
        {
            ant.TurnLeft();
            blackCells.Add(ant.Cell);
        }
    }

}
