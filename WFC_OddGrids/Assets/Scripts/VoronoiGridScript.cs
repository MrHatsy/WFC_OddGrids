using UnityEngine;
using Sylves;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
// using System.Numerics;
// using System.Drawing;
// using System.Diagnostics;

public class VoronoiGridScript : MonoBehaviour
{
    //tunable vars
    [SerializeField] private int cellCount = 50 ;
    
    //input vars
    private InputActions inputs;
    private InputAction regenerateGrid;
    

    //
    
    private VoronoiGridOptions voronoiGridOptions = new VoronoiGridOptions();
    private IGrid grid;
    
    private List<Vector2> pointList = new List<Vector2>();
    private static Material myMaterial;
    private Camera camera;
    private HashSet<Cell> liveCells = new HashSet<Cell>();
    // private Dictionary<Cell, Color> canvas = new Dictionary<Cell, Color>();
    private Color deadColor = Color.white;
    private Color livingColor = Color.red;

    private Cell startCell;


   void Awake()
    {    
        inputs = new InputActions();
        regenerateGrid = inputs.MapGen.Regenerate;
        
        camera = FindAnyObjectByType<Camera>();
        Time.fixedDeltaTime = 1f;
        CreateGrid();
    }

    void CreateGrid()
    {
        //first, get the points
        pointList.Clear();
        for( int i = 0; i < cellCount ; i++ ){
            Vector2 newPoint = new Vector2( UnityEngine.Random.Range(-5f, 5f), 
                UnityEngine.Random.Range(-5f, 5f));
            pointList.Add( newPoint );
        }

        grid = new VoronoiGrid( pointList, voronoiGridOptions );

    }

    void Start()
    {
        startCell = grid.GetCellByIndex(0);
        liveCells.Add(startCell);
        foreach( Cell cell in grid.GetCells())
        {
            Debug.Log($"{cell}: ");
        }
    }

        void Update()
    {
        if (regenerateGrid.triggered)
        {
            CreateGrid();
        }
    }

    //setup for rendering pipeline
    void OnEnable(){
        Debug.Log("Enable");
        RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
        regenerateGrid.Enable();
        // regenerateGrid.started += RegenerateGrid();
    }

    void OnDisable(){
        Debug.Log("Disable");
        RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
        regenerateGrid.Disable();
        // regenerateGrid.started -= RegenerateGrid();
    }

    

    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
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
        var corners = new[] { 
            new Vector3(0, 0), new Vector3(0, 1), 
            new Vector3(1, 0), new Vector3(1, 1) 
        }.Select(camera.GetComponent<Camera>().ViewportToWorldPoint);
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
