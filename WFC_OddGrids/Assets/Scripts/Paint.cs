using UnityEngine;
using Sylves;
using System;
using System.Linq;
using System.Collections.Generic;

public class Paint : MonoBehaviour
{
    IGrid grid = new HexGrid(1);
    Dictionary<Cell, Color> canvas = new Dictionary<Cell, Color>();
    Color defaultColor = Color.white;
    Color paintColor = Color.black;
    static Material myMaterial;

    static void CreateMyMaterial()
    {
        if(!myMaterial){
            
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            myMaterial = new Material(shader);
            myMaterial.hideFlags = hideFlags.HideAndDontSave;
            //turn on alpha blending
            myMaterial.setInt("")
        }
    }

    void Start()
    {
        canvas[new Cell(0, 0, 0)] = Color.red;
        canvas[new Cell(1, -1, 0)] = Color.green;
        canvas[new Cell(0, 1, -1)] = Color.blue;

        
    }

    
    void Update()
    {
        
    }

    void CellsInBoundingBox(){
    
        var corners = new[] { new Vector3(0, 0), new Vector3(0, 1), new Vector3(1, 0), new Vector3(1, 1) }
            .Select(camera.ViewportToWorldPoint);
        var min = corners.Aggregate(Vector3.Min);
        var max = corners.Aggregate(Vector3.Max);
        // flatten the box into the Z=0 plane, where the grid is defined
        min.z = max.z = 0;
        // get the cells
        var cells = grid.GetCellsIntersectsApprox(min, max).ToList();

        //set up appropriate drawing context


        //iterate over the cells




    }
}
