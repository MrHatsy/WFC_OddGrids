using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Tile : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int Type = 0; //0 = undecided
    private bool[] potentialType;
    private int entropy;
    private List<Tile> neighbours;
    private List<Tile> doubleNeighbours;
    public Color color = Color.white;

    void Start()
    {
        //TODO: fill entropy
    }

    public Tile()
    {
        
    }

    // Update is called once per frame
    void UpdateEntropy()
    {
        int entropy = 0;
        foreach( bool b in potentialType)
        {
            if(b) entropy++;
        }
    }
}
