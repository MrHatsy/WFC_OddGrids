using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Tile
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int type = 0; //0 = undecided
    private bool[] potentialType;
    private int entropy;
    private List<Tile> neighbours = new List<Tile>();
    private List<Tile> doubleNeighbours;
    private Color color = Color.white;
    public bool mark = false;

    //properties
    public Color MyColor
    {
        get{ return color; }
    }
    public int MyEntropy
    {
        get{ return entropy; }
    }

    //methods
    public Tile()
    {
        
        //TODO//
        // fill entropy
        potentialType = new bool[TileNeighbourIndex.AmountOfTileTypes];

        //cells should not be able to collapse into edge or empty tiles
        potentialType[0] = false;
        potentialType[1] = false;
        for( int i = 2; i < potentialType.Length ; i++)
        {
            potentialType[i] = true;
        }
        UpdateEntropy();
        
    }


    public void AddNeighbour(Tile newNeighbour)
    {
        neighbours.Add(newNeighbour);
    }

    public void Collapse()
    {
        List<int> possibilities = new List<int>();
        for( int i = 2; i < potentialType.Length ; i++)
        {
            if (potentialType[i])
            {
                //this is a possibility   
                possibilities.Add(i);
                potentialType[i] = false;
            }
        }
        type = possibilities[
            Random.Range(0, possibilities.Count)
            ];
        potentialType[type] = true;
        UpdateEntropy();
    }

    void UpdateEntropy()
    {
        entropy = 0;
        for( int i = 0; i < potentialType.Length ; i++)
        {
            if(potentialType[i])
            {
                entropy++;
            }
        }

        //TODO this should be moved to a better spot later
        color = TileNeighbourIndex.TileNColor(type);
    }
}
