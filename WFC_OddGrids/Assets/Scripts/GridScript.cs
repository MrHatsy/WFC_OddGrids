using UnityEngine;
using Sylves;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using System.Dynamic;
using System.IO;
using UnityEditor.ShaderGraph.Internal;
using Unity.VisualScripting;
// using System.Numerics;
// using System.Numerics;
// using System.Drawing;
// using System.Diagnostics;

public class GridScript : MonoBehaviour
{
    //tunable vars
    [SerializeField] private int cellCount = 50 ;
    [SerializeField] private float gridWidth = 10f ;
    [SerializeField] private float gridHeight = 10f ;
    [SerializeField] private int boundSize = 15;
    [SerializeField] private float hexDisturbance = 0.2f;
    [SerializeField] private int lloydRelaxations = 5;
    
    //input vars
    private InputActions inputs;
    private InputAction regenerateGrid;
    private InputAction nextGridType;
    private InputAction prevGridType;
    

    //constants
    private int noOfGrids = 6;
    private Camera myCamera;
    private static Material myMaterial;
    public TileNeighbourIndex tileNeighbourIndex;
    private bool drawOutlines = true;

    //variables
    private int gridTypeID = 0; // 0 = voronoi, 1= perturbed hex grid
    private IGrid grid;
    private VoronoiGridOptions voronoiGridOptions = new VoronoiGridOptions();
    private List<Vector2> voroPointList = new List<Vector2>();
    

    private Dictionary<Cell, Tile> tileList = new Dictionary<Cell, Tile>();



   void Awake()
    {    
        inputs = new InputActions();
        regenerateGrid = inputs.MapGen.Regenerate;
        nextGridType = inputs.MapGen.NextGridType;
        prevGridType = inputs.MapGen.PrevGridType;
        tileNeighbourIndex = new TileNeighbourIndex();
        
        myCamera = FindAnyObjectByType<Camera>();
        CreateGrid();
    }

    void Update()
    {
        //Input detections
        DetectInputs();

        WaveFunctionCollapse();
    }

    void CreateGrid()
    {
        MeshData meshData;
        int[] array = new int[5];
        HexBound hexBound = new HexBound(
                    new Vector3Int(-boundSize,-boundSize,-boundSize), 
                    new Vector3Int(boundSize,boundSize,boundSize));
        if(gridTypeID >= noOfGrids) gridTypeID = 0;
        if(gridTypeID < 0) gridTypeID = noOfGrids-1;

        switch (gridTypeID)
        {
            case 0: // Voronoi Grid
                //first, get the points
                voroPointList.Clear();
                for( int i = 0; i < cellCount ; i++ ){
                    Vector2 newPoint = new Vector2( 
                        UnityEngine.Random.Range(-1 * gridWidth, gridWidth), 
                        UnityEngine.Random.Range(-1 * gridHeight, gridHeight)
                    );
                    voroPointList.Add( newPoint );
                }

                //set up the grid options
                //lloyd relaxation makes the tiles more evenly spread
                voronoiGridOptions.LloydRelaxationIterations = lloydRelaxations;
                //border relaxation type Pin stopps the lloyd lloydRelaxations 
                    //from bringing the edges of the grid inwards
                voronoiGridOptions.BorderRelaxation = BorderRelaxation.Relax;

                voronoiGridOptions.ClipMax = new Vector2(gridWidth, gridHeight);
                voronoiGridOptions.ClipMin = new Vector2(-gridWidth, -gridHeight);

                //then, make the grid
                grid = new VoronoiGrid( voroPointList, voronoiGridOptions );
            break;
            case 1: //townscaper
                int triBoundSize = boundSize/2; 
                //because we're subdividing this, we'll use half the bound size
                TriangleBound bigTriBound = new TriangleBound(
                        new Vector3Int(-triBoundSize,-triBoundSize,-triBoundSize), 
                        new Vector3Int(triBoundSize,triBoundSize,triBoundSize));
                grid = new TriangleGrid(2f, TriangleOrientation.FlatSides, bigTriBound);
                //we need to turn the grid into a mesh so we can mess with it
                meshData = grid.ToMeshData();
                meshData = meshData.RandomPairing();
                meshData = ConwayOperators.Ortho(meshData);
                //glue duplicate points together                
                meshData = meshData.Weld(out array, 0.2f);
                meshData = meshData.Relax();

                grid = new MeshGrid( meshData, null );
            break;
            case 2: //perturbed hex grid
                
                grid = new HexGrid(1f, HexOrientation.FlatTopped, hexBound);
                //we need to turn the grid into a mesh so that we can shift the points
                meshData = grid.ToMeshData();
                meshData = meshData.Weld(out array, 0.2f);
                for( int i = 0; i < meshData.vertices.Length ; i++ )
                {
                    meshData.vertices[i].x += UnityEngine.Random.Range(-hexDisturbance, hexDisturbance);
                    meshData.vertices[i].y += UnityEngine.Random.Range(-hexDisturbance, hexDisturbance);
                }
                grid = new MeshGrid( meshData, null );
            break;
            case 3: //square grid
                grid = new SquareGrid(1f, new SquareBound(
                    new Vector2Int(-12, -7), 
                    new Vector2Int(12, 7)
                    ));
            break;
            case 4: //hex grid
                grid = new HexGrid(1f, HexOrientation.FlatTopped, hexBound);
            break;
            case 5: //tri grid
                TriangleBound triBound = new TriangleBound(
                    new Vector3Int(-boundSize,-boundSize,-boundSize), 
                    new Vector3Int(boundSize,boundSize,boundSize));
                grid = new TriangleGrid(1f, TriangleOrientation.FlatTopped, triBound);
            break;
        }


        tileList.Clear();
        //initialize every tile
        foreach( Cell c in grid.GetCells() )
        {
            tileList.Add(c, new Tile());
        }
        //let each tile know its neighbours
        foreach( Cell c in grid.GetCells() )
        {
            foreach( Cell n in grid.GetNeighbours(c) )
            {
                tileList[c].AddNeighbour(tileList[n]);
            }
        }
        
    }

    void WaveFunctionCollapse()
    {
        //find the lowest entropy cell, and lock in its type
        WFCObserve();

        //propogate changes now that a cell has been collapsed
        WFCPropogate();
        
    }

    void WFCObserve()
    {
        Debug.Log("Observing...");
        Tile minEntrTile = FindLowestEntropy();
        if (minEntrTile == null)
        {
            //TODO TODO all tiles are collapsed, we are ready to end generation
            Debug.Log("Ready to end generation");
        }
        else
        {
            //collapse the tile and mark it for update during propogation
            minEntrTile.Collapse();
            minEntrTile.mark = true;
        }
    }

    Tile FindLowestEntropy()
    {
        //if entropy == 1, tile is collapsed, if e > 1, tile not collapsed
        int minEntropy = 100;
        List<Tile> minEntropyTileList = new List<Tile>();
        Tile minEntropyTile;
        foreach( Cell c in grid.GetCells() )
        {
            int myEntropy = tileList[c].MyEntropy;
            if (myEntropy == 0)
            {
                // TODO: ERROE
                //ERROR ERROR ERROE this grid is invalid as this tile cannot collapse
            }
            else if (myEntropy < minEntropy && myEntropy > 1)
            {
                minEntropy = myEntropy;
                minEntropyTileList.Clear();
                minEntropyTileList.Add(tileList[c]);
            }
            else if (myEntropy == minEntropy)
            {
                //make a list of all the tiles with min entropy so we can pick
                //a random tile if there are multiple
                minEntropyTileList.Add(tileList[c]);
            }
        }
        int randomItem = UnityEngine.Random.Range(0, minEntropyTileList.Count);
        // Debug.Log("Random item: " + randomItem + " List size: " + minEntropyTileList.Count);
        
        minEntropyTile = minEntropyTileList[randomItem];
        if(minEntropy == 100)
        {
            //all tiles are collapsed
            return null;    
        }
        return minEntropyTile;
        
    }

    void WFCPropogate()
    {
        Debug.Log("Propogating...");
        bool keepPropogating = true;
        while (keepPropogating )
        {
            keepPropogating = false;
            foreach (Cell c in grid.GetCells())
            {
                Tile t = tileList[c];
                //for each cell that has been marked to update
                if (t.mark)
                {
                    t.mark = false;
                    //if there are cells that need to be updated, keep propgating
                    t.Propogate();
                }
            }

            foreach (Cell c in grid.GetCells())
            {
                if (tileList[c].mark)
                {
                    keepPropogating = true;
                }
            }
        }


        
    }

    void DetectInputs()
    {
        if (regenerateGrid.triggered)
        {
            CreateGrid();
        }
        if (nextGridType.triggered)
        {
            gridTypeID++;
            CreateGrid();
        }
        if (prevGridType.triggered)
        {
            gridTypeID--;
            CreateGrid();
        }
    }

    //setup for rendering pipeline and inputs
    void OnEnable(){
        Debug.Log("Enable");
        RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
        regenerateGrid.Enable();
        nextGridType.Enable();
        prevGridType.Enable();
        regenerateGrid.Enable();
        // regenerateGrid.started += RegenerateGrid();
    }

    void OnDisable(){
        Debug.Log("Disable");
        RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
        regenerateGrid.Disable();
        nextGridType.Disable();
        prevGridType.Disable();
        // regenerateGrid.started -= RegenerateGrid();
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
        var corners = new[] { 
            new Vector3(0, 0), new Vector3(0, 1), 
            new Vector3(1, 0), new Vector3(1, 1) 
        }.Select(myCamera.GetComponent<Camera>().ViewportToWorldPoint);
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
            // Color color = tileList.TryGetValue(cell, out var c) ? c : deadColor;
            Color color = tileList.TryGetValue(cell, out var tile) ? tile.MyColor : Color.magenta;

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
        if (drawOutlines = true)
        {
            foreach (var cell in cells)
            {
                //get the color from the hashset
                Color color = Color.black;

                //read the verticies of the cell
                var verticies = grid.GetPolygon(cell);

                //split up the polygon into lines, and draw each one
                GL.Begin(GL.LINES);
                GL.Color(color.linear);
                for (int i = 1; i < verticies.Length; i++)
                {
                    GL.Vertex(verticies[i]);
                    GL.Vertex(verticies[i - 1]);
                    //NOTE: this will not draw one of the lines, 
                    //but the tiles next to it should make up for that
                }
                GL.End();
            }
        }
        
        GL.PopMatrix();
    }
    //////
}
