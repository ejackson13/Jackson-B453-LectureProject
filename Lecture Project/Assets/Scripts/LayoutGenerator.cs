using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LayoutGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase tile;
    public int tileMapSizeX = 72; // the x dimension of the play area (in number of grids)
    public int tileMapSizeY = 40; // the y dimension of the play area (in number of grids)
    public Vector3Int tilemapCenterInGrid = new Vector3Int(38, 22, 0);
    public int maxXIn = 14; // the max number of blocks that can jut in from the edge in the x direction
    public int maxYIn = 10; // the max number of blocks that can jut in from the edge in the y direction
    bool[,] blockPlacements; // grid to track where blocks should be placed

    // Start is called before the first frame update
    void Start()
    {
        blockPlacements = new bool[tileMapSizeY, tileMapSizeX];
        GenerateGrid();
        DrawLevel();
    }

    // Update is called once per frame
    void Update()
    {
    }


    void DrawLevel()
    {
        //tilemap.SetTile(new Vector3Int(0, 0, 0), tile); // used to find center

        for (int r = 0; r < tileMapSizeY; r++)
        {
            for (int c = 0; c < tileMapSizeX; c++)
            {
                if (blockPlacements[r,c])
                {
                    tilemap.SetTile(new Vector3Int(c-tilemapCenterInGrid.x+1, tilemapCenterInGrid.y-r-2, 0), tile);
                }  
            }
        }
    }



    void GenerateGrid()
    {
        int xAmt = 0;
        int yAmt = 0;

        // left edge
        int spaceLeft = tileMapSizeY; // amount of space left on edge
        Debug.Log("Left");
        for (int i=0; i<tileMapSizeY; i++)
        {
            // get area of square to fill
            xAmt = UnityEngine.Random.Range(0, maxXIn + 1);
            //yAmt = UnityEngine.Random.Range(1, spaceLeft + 1);
            yAmt = UnityEngine.Random.Range(Mathf.Min(spaceLeft, 6), Mathf.Min(spaceLeft + 1, 13));
            spaceLeft -= yAmt;
            Debug.Log($"Y - {yAmt} | X - {xAmt}");

            // fill area in grid
            for (int j=i; j<i+yAmt; j++)
            {
                for (int k=0; k<xAmt; k++)
                {
                    blockPlacements[j,k] = true;
                }
            }
            i += (yAmt-1);
        }

        //PrintGrid();

        // right edge
        spaceLeft = tileMapSizeY; // amount of space left on edge
        Debug.Log("Right");
        for (int i = 0; i < tileMapSizeY; i++)
        {
            // get area of square to fill
            xAmt = UnityEngine.Random.Range(0, maxXIn + 1);
            //yAmt = UnityEngine.Random.Range(1, spaceLeft + 1);
            yAmt = UnityEngine.Random.Range(Mathf.Min(spaceLeft, 6), Mathf.Min(spaceLeft + 1, 13));
            spaceLeft -= yAmt;

            Debug.Log($"Y - {yAmt} | X - {xAmt}");

            // fill area in grid
            for (int j = i; j < i + yAmt; j++)
            {
                for (int k = tileMapSizeX - xAmt; k < tileMapSizeX;  k++)
                {
                    blockPlacements[j, k] = true;
                }
            }
            i += (yAmt - 1);
        }


        // find edges of top (so we can add the new shapes at the top between the ones on the left and right edges)
        int leftEdge = 0; // the first index of the top edge that does not already have a block on it
        int rightEdge = 0; // the last index of the top edge that does not already have a block on it
        for (int i=0; i < tileMapSizeX; i++)
        {
            if (!blockPlacements[0, i])
            {
                leftEdge = i;
                break;
            }
        }
        for (int i=tileMapSizeX-1; i >= 0; i--)
        {
            if (!blockPlacements[0, i])
            {
                rightEdge = i + 1;
                break;
            }
        }

        // top edge
        spaceLeft = rightEdge-leftEdge; // amount of space left on edge
        Debug.Log("Top");
        for (int c = leftEdge; c < rightEdge; c++)
        {
            // get area of square to fill
            yAmt = UnityEngine.Random.Range(0, maxYIn + 1);
            //xAmt = UnityEngine.Random.Range(1, spaceLeft + 1);
            xAmt = UnityEngine.Random.Range(Mathf.Min(spaceLeft, 12), Mathf.Min(spaceLeft + 1, 24));
            spaceLeft -= xAmt;

            Debug.Log($"Y - {yAmt} | X - {xAmt}");

            // fill area in grid
            for (int j = 0; j < yAmt; j++)
            {
                for (int k = c; k < c + xAmt; k++)
                {
                    blockPlacements[j, k] = true;
                }
            }
            c += (xAmt - 1);
        }




        // find edges of bottom (so we can add the new shapes at the bottom between the ones on the left and right edges)
        for (int i = 0; i < tileMapSizeX; i++)
        {
            if (!blockPlacements[tileMapSizeY-1, i])
            {
                leftEdge = i;
                break;
            }
        }
        for (int i = tileMapSizeX - 1; i >= 0; i--)
        {
            if (!blockPlacements[tileMapSizeY-1, i])
            {
                rightEdge = i + 1;
                break;
            }
        }

        // bottom edge
        spaceLeft = rightEdge - leftEdge; // amount of space left on edge
        Debug.Log("Bottom");
        for (int c = leftEdge; c < rightEdge; c++)
        {
            // get area of square to fill
            yAmt = UnityEngine.Random.Range(0, maxYIn + 1);
            //xAmt = UnityEngine.Random.Range(1, spaceLeft + 1);
            xAmt = UnityEngine.Random.Range(Mathf.Min(spaceLeft, 12), Mathf.Min(spaceLeft + 1, 24));
            spaceLeft -= xAmt;

            Debug.Log($"Y - {yAmt} | X - {xAmt}");

            // fill area in grid
            for (int j = tileMapSizeY - yAmt; j < tileMapSizeY; j++)
            {
                for (int k = c; k < c + xAmt; k++)
                {
                    blockPlacements[j, k] = true;
                }
            }
            c += (xAmt - 1);
        }
    }


    void PlaceMiddleBlocks()
    {

    }


    void PrintGrid()
    {
        string grid = "";
        for (int r = 0; r < tileMapSizeY; r++)
        {
            for (int c = 0; c < tileMapSizeX; c++)
            {
                grid += blockPlacements[r, c] + " ";
            }
            grid += "\n";
        }
        Debug.Log(grid);
    }
}
