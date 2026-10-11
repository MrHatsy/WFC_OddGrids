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
    public Color color = Color.white;

    public Tile()
    {
        //TODO//
        // fill entropy
    }


    public void AddNeighbour(Tile newNeighbour)
    {
        neighbours.Add(newNeighbour);
    }

    // Update 
    void UpdateEntropy()
    {
        int entropy = 0;
        foreach( bool b in potentialType)
        {
            if(b) entropy++;
        }
        color = TileNeighbourIndex.TileNColor(type);
    }
}
