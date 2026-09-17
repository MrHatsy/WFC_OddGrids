using UnityEngine;
using Sylves;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Rendering;


public class LangtonsAnt : MonoBehaviour
{
    private static IGrid grid = new HexGrid(1f, HexOrientation.FlatTopped);
    private HashSet<Cell> blackCells = new HashSet<Cell>();
    private static Cell startCell = new Cell(0, 0, 0);
    private static FTHexDir startDir = FTHexDir.Up;
    private Walker ant = new Walker(grid, startCell, (CellDir)startDir);

    Dictionary<Cell, Color> canvas = new Dictionary<Cell, Color>();
    Color defaultColor = Color.white;
    Color paintColor = Color.black;
    private static Material myMaterial;
    private Camera camera;
    
    static void CreateMyMaterial()
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

    void Start()
    {
        camera = FindAnyObjectByType<Camera>();
    }

    void Update()
    {
        Walk();
    }

    void Walk()
    {
        Debug.Log($"Ant is at {ant.Cell} and is facing {(FTHexDir)ant.Dir}");
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

    void OnEnable(){
        Debug.Log("Enable");
        RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
    }

    void OnDisable(){
        Debug.Log("Disable");
        RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
    }

    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        Draw();
    }

    void Draw(){

        var corners = new[] { new Vector3(0, 0), new Vector3(0, 1), new Vector3(1, 0), new Vector3(1, 1) }
            .Select(camera.GetComponent<Camera>().ViewportToWorldPoint);
        var min = corners.Aggregate(Vector3.Min);
        var max = corners.Aggregate(Vector3.Max);
        // flatten the box into the Z=0 plane, where the grid is defined
        min.z = max.z = 0;
        // get the cells
        var cells = grid.GetCellsIntersectsApprox(min, max).ToList();

        //set up appropriate drawing context

        CreateMyMaterial();
        GL.PushMatrix();
        myMaterial.SetPass( 0 );

        //iterate over the cells
        foreach (var cell in cells)
        {
            //get the color from the hashset
            Color color;
            if(blackCells.Contains(cell))
            {
                color = paintColor;
            }
            else
            {
                color = defaultColor;
            }
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
                Debug.Log("Im drawing a shape!");
            }
            GL.End();
        }

        GL.PopMatrix();

    }

}
