using System.Collections.Generic;
using System.Threading;
using System.Timers;
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
    public int MyType
    {
        get{ return type; }
    }
    public int MyEntropy
    {
        get { return entropy; }
    }
    public List<Tile> Neighbours
    {
        get{ return neighbours; }
    }
    public bool[] PotentialType
    {
        get{ return potentialType; }
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
        for (int i = 2; i < potentialType.Length; i++)
        {
            potentialType[i] = true;
        }
        UpdateEntropy();

    }

    public void Propogate()
    {
        //for each neighbour
        foreach (Tile n in neighbours)
        {
            n.PropogateForNeighbours();
        }
    }

    public void PropogateForNeighbours()
    {
        for (int i = 0; i < potentialType.Length; i++)
        {
            //for each type still potentially valid
            if (potentialType[i])
            {
                //check if this type is still valid
                if (!IsThisTypeValid(i))
                {
                    //if invalid, mark as invalid and mark this tile for update
                    potentialType[i] = false;
                    mark = true;
                    UpdateEntropy();
                }
                if (entropy == 1)
                {
                    Collapse();
                }
            }       
        }
    }

    public bool IsThisTypeValid(int typeID)
    {
        //First, check if i fit my neighbours max relations
        //for each type i could have as a n, count all my ns, if i have the max then remove this
        int countOfThisType;
        for (int i = 0; i < TileNeighbourIndex.AmountOfTileTypes; i++)
        {
            countOfThisType = 0;
            foreach (Tile n in neighbours)
            {
                if (n.MyType == i)
                {
                    countOfThisType++;
                }
            }
            if (countOfThisType > TileNeighbourIndex.TileNMMax(typeID, i))
            {
                //too many neighbours to be of this type! we cant be this!
                Debug.Log("return false from max!! " + "type: " + typeID + " neigh: " + i);
                return false;
            }
        }

        //check if i fit my neighbours min relations
        //if my matching neighbours + potential matching neighbours => min, we are good
        for (int i = 0; i < TileNeighbourIndex.AmountOfTileTypes; i++)
        {
            countOfThisType = 0;
            foreach (Tile n in neighbours)
            {
                if (n.PotentialType[i] == true)
                {
                    countOfThisType++;
                }
            }
            if (countOfThisType < TileNeighbourIndex.TileNMMin(typeID, i))
            {
                // Debug.Log("return false from MIN?? "  + "type: " + typeID + " neigh: " + i);
                // Debug.Log("Count: "  + countOfThisType + " neigh: " + i);
                //not enough neighbours to be of this type! we cant be this!                
                return false;
            }
        }
                    ;
        return true;
    }


    public void AddNeighbour(Tile newNeighbour)
    {
        neighbours.Add(newNeighbour);
    }

    public void Collapse()
    {
        List<int> possibilities = new List<int>();
        for (int i = 2; i < potentialType.Length; i++)
        {
            if (potentialType[i])
            {
                //this is a possibility   
                possibilities.Add(i);
                potentialType[i] = false;
            }
        }
        // Debug.Log("possibilites count: " + possibilities.Count );
        type = possibilities[
            Random.Range(0, possibilities.Count)
            ];
        potentialType[type] = true;
        UpdateEntropy();
        color = TileNeighbourIndex.TileNColor(type);
    }

    private void UpdateEntropy()
    {
        entropy = 0;
        for( int i = 0; i < potentialType.Length ; i++)
        {
            if(potentialType[i])
            {
                entropy++;
            }
        }
    }
}
