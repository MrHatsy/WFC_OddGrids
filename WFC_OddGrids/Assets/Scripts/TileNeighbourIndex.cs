using UnityEngine;
using System.IO;
using System;

public class TileNeighbourIndex
{  
    //constants
    private static int amountOfTileTypes;
    private static int[] tileWeightIndex;
    private static int[] tileMinCountIndex;
    private static Color[] tileColorIndex;
    private static int[,] tileNeighbourMinIndex;
    private static int[,] tileNeighbourMaxIndex;

    //Properties
    public static int AmountOfTileTypes
    {
        get { return amountOfTileTypes; }
    }
    public static int TileNWeight( int n )
    {
        return tileWeightIndex[n];
    }
    public static Color TileNColor( int n)
    {
        return tileColorIndex[n];
    }
    public static int TileMinCount( int n)
    {
        return tileMinCountIndex[n];
    }
    public static int TileNMMin(int n, int m)
    {
        return tileNeighbourMinIndex[n,m];
    }
    public static int TileNMMax(int n, int m)
    {
        return tileNeighbourMaxIndex[n,m];
    }

    //methods
    public TileNeighbourIndex()
    {
        String line;
        StreamReader reader;
        String[] splitLine;
        int lineNum;

        //get amount of tiles from tiletypeindex.csv
        amountOfTileTypes = 0;
        reader = new StreamReader("./Assets/Data/tileTypeIndex.csv");
        line = reader.ReadLine();
        while (line != null)
        {
            amountOfTileTypes++;
            line = reader.ReadLine();
        }

        //set up arrays based on amount of tiles

        tileWeightIndex = new int[amountOfTileTypes];
        tileMinCountIndex = new int[amountOfTileTypes];
        tileColorIndex = new Color[amountOfTileTypes];
        tileNeighbourMinIndex = new int[amountOfTileTypes, amountOfTileTypes];
        tileNeighbourMaxIndex = new int[amountOfTileTypes, amountOfTileTypes];

        //fill in color index, weight index, and mincount index
        lineNum = 0;
        reader = new StreamReader("./Assets/Data/tileTypeIndex.csv");
        line = reader.ReadLine();
        while (line != null)
        {
            splitLine = line.Split(',');
            tileWeightIndex[lineNum] = Int32.Parse(splitLine[2]);
            tileMinCountIndex[lineNum] = Int32.Parse(splitLine[6]);
            tileColorIndex[lineNum] = new Color(
                Single.Parse(splitLine[3]),
                Single.Parse(splitLine[4]),
                Single.Parse(splitLine[5]));
            line = reader.ReadLine();
            lineNum++;
        }


        splitLine = new string[amountOfTileTypes];
        //fill in min index
        lineNum = 0;
        reader = new StreamReader("./Assets/Data/tileNeighbourMinIndex.csv");
        line = reader.ReadLine();
        while (line != null)
        {
            splitLine = line.Split(',');
            for (int i = 0; i < splitLine.Length; i++)
            {
                tileNeighbourMinIndex[lineNum, i] = Int32.Parse(splitLine[i]);
            }
            line = reader.ReadLine();
            lineNum++;
        }

        //fill in max index
        lineNum = 0;
        reader = new StreamReader("./Assets/Data/tileNeighbourMaxIndex.csv");
        line = reader.ReadLine();
        while (line != null)
        {
            splitLine = line.Split(',');
            for (int i = 0; i < splitLine.Length; i++)
            {
                tileNeighbourMaxIndex[lineNum, i] = Int32.Parse(splitLine[i]);
            }
            line = reader.ReadLine();
            lineNum++;
        }
        
    }
}


