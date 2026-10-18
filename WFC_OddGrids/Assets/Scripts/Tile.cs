using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using Mono.Cecil;
using Unity.VisualScripting;
using UnityEngine;

public class Tile
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int type = 0; //0 = undecided
    private bool[] potentialType;
    private int entropy;
    private int entropyWeight;
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
    public int MyEntropyWeight
    {
        get { return entropy; }
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

    public void AddNeighbour(Tile newNeighbour)
    {
        neighbours.Add(newNeighbour);
    }


    public void Propogate()
    {
        //for each neighbour (thats not already collapsed)
        foreach (Tile n in neighbours)
        {
            if(n.MyEntropy > 1)
            {
                n.PropogateForNeighbours();    
            }
            
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
        int max;
        int min;
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

            //we use -x when the limit is neighbourcount-x, as the neighbourcount is inconsisten
            max = TileNeighbourIndex.TileNMMax(typeID, i);
            if (max < 0)
            {
                max = neighbours.Count - max;
            }

            if (countOfThisType > max)
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

            //we use -x when the limit is neighbourcount-x, as the neighbourcount is inconsisten
            min = TileNeighbourIndex.TileNMMin(typeID, i);
            if (min < 0)
            {
                min = neighbours.Count - min;
            }


            if (countOfThisType < min)
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

    void CheckMins()
    {
        List<int> unmetMins = new List<int>();
        bool[] potentialTypeTemp = potentialType;

        int countOfThisType;
        int min;

        //for each neighbour
        foreach( Tile n in neighbours)
        {
            //check if n is collapsed
            if(n.MyType != 0)
            {
                //check if my mins are good
                //for each type
                for (int i = 0; i < TileNeighbourIndex.AmountOfTileTypes; i++)
                {
                    countOfThisType = 0;
                    foreach (Tile nn in n.neighbours)
                    {
                        if (nn.MyType == i)
                        {
                            countOfThisType++;
                        }
                    }
                    //we use -x when the limit is neighbourcount-x, as the neighbourcount is inconsisten
                    min = TileNeighbourIndex.TileNMMin(n.MyType, i);
                    if (min < 0)
                    {
                        min = n.neighbours.Count - min;
                    }

                    //if mins not good, keep this type
                    if (countOfThisType < min)
                    {
                        unmetMins.Add(i);
                    }
                }
            }
        }
        
        //if no unmet mins, we're good to go, else,
        if(unmetMins.Count != 0)
        {
            //keep only types that are kept AND that are potententially us
            //since they're already true, we NAND to find the falses
            for( int i = 0 ; i < potentialType.Length ; i++)
            {
                if( !unmetMins.Contains(i) )
                {
                    potentialTypeTemp[i] = false;
                }
            } 
        }

        // if the new list is Empty, dont use it
        for( int i = 0 ; i < potentialType.Length ; i++)
        {
            if (potentialTypeTemp[i])
            {
                potentialType = potentialTypeTemp;
                return;
            }
        }    
            
    }

    public void Collapse()
    {
        // if( type != 0)
        // {
        //     entropy = 1;
        //     return;
        // }
        //get total possible weights

        //make sure we dont contradict our neighbours minimums
        CheckMins();

        int totalWeight = 0;
        for (int i = 2; i < potentialType.Length; i++)
        {
            if (potentialType[i])
            {
                //this is a possibility   
                totalWeight += TileNeighbourIndex.TileNWeight(i);
            }
        }
        //get random from 0 to total weight
        int randomBasedOnWeight = Random.Range(0, totalWeight);
        //subtract weights until random < 0;
        for (int i = 2; i < potentialType.Length && randomBasedOnWeight >= 0; i++)
        {
            if (potentialType[i])
            {
                randomBasedOnWeight -= TileNeighbourIndex.TileNWeight(i);
                type = i;
                potentialType[i] = false;
            }
        }

        // Debug.Log("possibilites count: " + possibilities.Count );
        
        potentialType[type] = true;
        entropy = 1;
        color = TileNeighbourIndex.TileNColor(type);
    }

    private void UpdateEntropy()
    {
        entropy = 0;
        entropyWeight = 0;
        for( int i = 0; i < potentialType.Length ; i++)
        {
            if(potentialType[i])
            {
                entropy++;
                entropyWeight += TileNeighbourIndex.TileNWeight(i);
            }
        }
    }
}
