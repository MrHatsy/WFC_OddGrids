using UnityEngine;
using Sylves;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Rendering;
// using System.Diagnostics;

public class SampleGrid : MonoBehaviour
{
    private static HexBound hexBound = new HexBound(new Vector3Int(-15,-15,-15), new Vector3Int(15,15,15));
    private IGrid grid = new HexGrid(1f, HexOrientation.FlatTopped, hexBound);
    private static Material myMaterial;
    private Camera myCamera;
    private HashSet<Cell> liveCells = new HashSet<Cell>();
    // private Dictionary<Cell, Color> canvas = new Dictionary<Cell, Color>();
    private Color deadColor = Color.white;
    private Color livingColor = Color.red;

    private Cell startCell;


   void Awake()
    {    
        myCamera = FindAnyObjectByType<Camera>();
        Time.fixedDeltaTime = 1f;
    }

    void Start()
    {
        startCell = grid.GetCellByIndex(0);
        liveCells.Add(startCell);
        // grid = grid.GetDiagonalGrid();
        //make random cells live
        RandomizeCells();
        foreach( Cell cell in grid.GetCells())
        {
            Debug.Log($"{cell}: ");
        }
    }

        void FixedUpdate()
    {
        GameOfLife();
        Debug.Log("update");
    }

    void RandomizeCells()
    {
        
        foreach( Cell cell in grid.GetCells() )
        {

            if( 1 == UnityEngine.Random.Range(0, 2))
            {
                liveCells.Add(cell);
                Debug.Log("Randomising!");
            }
        }
    }

    void GameOfLife()
    {
        foreach( Cell cell in grid.GetCells() )
        {
            //count my neighbours
            IEnumerable<Cell> myNeighbours = grid.GetNeighbours(cell);
            int livingNeighbours = 0;
            foreach( Cell neighbour in myNeighbours)
            {
                if( liveCells.Contains(neighbour) )
                {
                    livingNeighbours++;
                }
            }
           
            if (liveCells.Contains(cell))
            {
                //if i am alive, stay alive with 2 or 3 neighbours
                if( livingNeighbours < 2 || livingNeighbours > 3)
                {
                    liveCells.Remove(cell);
                }
            }
            else
            {
                //if i am dead, become alive with 3 neighbours
                if( livingNeighbours == 3)
                {
                    liveCells.Add(cell);
                }
            }
        }
    }

    //setup for rendering pipeline
    void OnEnable(){
        Debug.Log("Enable");
        RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
    }

    void OnDisable(){
        Debug.Log("Disable");
        RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
    }

    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera myCamera)
    {
        DrawGrid();
    }

    private void CreateMyMaterial()
    {
        if(!myMaterial){
            
            //Unity has a built-in shader for drawing simple colored things
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            myMaterial = new Material(shader);
            myMaterial.hideFlags = HideFlags.HideAndDontSave;
            //turn on alpha blending
            myMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            myMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            //Turn of backface culling
            myMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            //Turn off depth writes
            myMaterial.SetInt("_ZWrite", 0);
        }
    }
    ///////

    //Draw the grid
    private IEnumerable<Cell> GetVisibleCells()
    {
        var corners = new[] { new Vector3(0, 0), new Vector3(0, 1), new Vector3(1, 0), new Vector3(1, 1) }
            .Select(myCamera.GetComponent<Camera>().ViewportToWorldPoint);
        var min = corners.Aggregate(Vector3.Min);
        var max = corners.Aggregate(Vector3.Max);
        // flatten the box into the Z=0 plane, where the grid is defined
        min.z = max.z = 0;
        // get the cells
        IEnumerable<Cell> cells = grid.GetCellsIntersectsApprox(min, max).ToList();
        return cells;
    }
    void DrawGrid(){
        IEnumerable<Cell> cells = GetVisibleCells();
        // IEnumerable<Cell> cells = grid.GetCells();
        
        //setup for drawing
        CreateMyMaterial();
        GL.PushMatrix();
        myMaterial.SetPass( 0 );

        //draw cell colours
        foreach (var cell in cells)
        {
            // //get the color from the dictionary
            // Color color = canvas.TryGetValue(cell, out var c) ? c : deadColor;
            Color color = liveCells.Contains(cell) ? livingColor : deadColor;

            //read the verticies of the cell
            var verticies = grid.GetPolygon(cell);
            
            //split up the polygon into tris, and draw each one
            GL.Begin(GL.TRIANGLES);
            GL.Color(color.linear);
            for( int i = 2 ; i < verticies.Length ; i++ )
            {
                GL.Vertex(verticies[0]);
                GL.Vertex(verticies[i]);
                GL.Vertex(verticies[i-1]);
            }
            GL.End();
        }
        //draw cell outlines
        foreach (var cell in cells)
        {
            //get the color from the hashset
            Color color = Color.black;

            //read the verticies of the cell
            var verticies = grid.GetPolygon(cell);
            
            //split up the polygon into lines, and draw each one
            GL.Begin(GL.LINES);
            GL.Color(color.linear);
            for( int i = 1 ; i < verticies.Length ; i++ )
            {
                GL.Vertex(verticies[i]);
                GL.Vertex(verticies[i-1]);
                //NOTE: this will not draw one of the lines, 
                //but the tiles next to it should make up for that
            }
            GL.End();
        }
        GL.PopMatrix();
    }
    //////
}
