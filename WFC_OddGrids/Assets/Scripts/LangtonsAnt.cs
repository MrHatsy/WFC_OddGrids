using UnityEngine;
using Sylves;
using System.Collections.Generic;
using System;
using System.Linq;


public class LangtonsAnt : MonoBehaviour
{
    private static IGrid grid = new SquareGrid(1f);
    private HashSet<Cell> blackCells = new HashSet<Cell>();
    private static Cell startCell = new Cell(0, 0);
    private static var startDir = SquareDir.Up;
    private Walker ant = new Walker(grid, startCell, (CellDir)startDir);

    Dictionary<Cell, Color> canvas = new Dictionary<Cell, Color>();
    Color defaultColor = Color.white();
    Color paintColor = Color.black();
    

    void Start()
    {
        
    }

    void Update()
    {
        // Walk();
        DrawGrid();
    }

    void Walk()
    {
        // Debug.Log($"Ant is at {ant.Cell} and is facing {(SquareDir)ant.Dir}");
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

    void DrawGrid()
    {
        Debug.Log("Draw function");
        //
    }

}
