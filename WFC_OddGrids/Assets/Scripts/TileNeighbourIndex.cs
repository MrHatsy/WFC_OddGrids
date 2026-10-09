using UnityEngine;
using System.IO;
using System;

public class TileNeighbourIndex
{
    private static int amountOfTileTypes;
    static int[] tileWeightIndex;
    static int[] tileColorIndex;
    static int[,] tileNeighbourMinIndex;
    static int[,] tileNeighbourMaxIndex;

    public TileNeighbourIndex()
    {
        String line;
        StreamReader reader;
        String[] splitLine;
        int lineNum;
        
        //get amouunt of tiles from tiletypeindex.csv
        amountOfTileTypes = 0; 
        reader = new StreamReader("../Data/tileTypeIndex.csv");
        line = reader.ReadLine();
        while( line != null)
        {
            amountOfTileTypes++;
            line = reader.ReadLine();
        }

        //set up arrays based on amount of tiles

        tileWeightIndex = new int[amountOfTileTypes];
        tileColorIndex = new int[amountOfTileTypes];
        tileNeighbourMinIndex = new int[amountOfTileTypes, amountOfTileTypes];
        tileNeighbourMaxIndex = new int[amountOfTileTypes, amountOfTileTypes];

        //TODO fill in weigh index
        
        //TODO: Color index as well;

        splitLine = new string[amountOfTileTypes];
        //fill in min index
        lineNum = 0;
        reader = new StreamReader("../Data/tileNeighbourMinIndex.csv");
        line = reader.ReadLine();
        while( line != null)
        {            
            splitLine = line.Split(',');
            for( int i = 0 ; i < splitLine.Length ; i++)
            {
                tileNeighbourMinIndex[lineNum, i] = Int32.Parse(splitLine[i]);
            }
            line = reader.ReadLine();
            lineNum++;
        }

        //fill in max index
        lineNum = 0;
        reader = new StreamReader("../Data/tileNeighbourMaxIndex.csv");
        line = reader.ReadLine();
        while( line != null)
        {            
            splitLine = line.Split(',');
            for( int i = 0 ; i < splitLine.Length ; i++)
            {
                tileNeighbourMaxIndex[lineNum, i] = Int32.Parse(splitLine[i]);
            }
            line = reader.ReadLine();
            lineNum++;
        }
    }
}


