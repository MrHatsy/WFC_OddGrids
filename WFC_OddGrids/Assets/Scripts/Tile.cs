using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Tile
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int Type = 0; //0 = undecided
    private bool[] potentialType;
    private int entropy;
    private List<Tile> neighbours = new List<Tile>();
    private List<Tile> doubleNeighbours;
    public Color color = Color.white;

    public Tile()
    {
        //TODO: fill entropy
    }


    public void AddNeighbour(Tile newNeighbour)
    {
        neighbours.Add(newNeighbour);
        switch (neighbours.Count)
        {
            case <4:
            color = Color.blue;
            break;
            case 4:
            color = Color.red;
            break;
            case 5:
            color = Color.green;
            break;
            case 6:
            color = Color.yellow;
            break;
            case 7:
            color = Color.magenta;
            break;
            case 8:
            color = Color.cyan;
            break;
            case >9:
            color = Color.mediumTurquoise;
            break;
        }
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
