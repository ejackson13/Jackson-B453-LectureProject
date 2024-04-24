using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LayoutGenerator : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase tile;
    [SerializeField] private int tileMapSizeX = 72; // the x dimension of the play area (in number of grids)
    [SerializeField] private int tileMapSizeY = 40; // the y dimension of the play area (in number of grids)
    [SerializeField] private Vector3Int tilemapCenterInGrid = new Vector3Int(38, 22, 0);
    [SerializeField] private int maxXIn = 14; // the max number of blocks that can jut in from the edge in the x direction
    [SerializeField] private int maxYIn = 10; // the max number of blocks that can jut in from the edge in the y direction
    [SerializeField] private bool[,] blockPlacements; // grid to track where blocks should be placed
    [SerializeField] private GameObject[] basePrefabs; // prefabs for billion bases
    private Vector3[] basePositions; // array of the bases' positions for drawing gizmos - only used for testing
    [SerializeField] private float wallDist; // radius around base spawn location where there should be be no walls for a base to be place
    [SerializeField] private float baseDist; // radius in addition to shoot range to not spawn bases within proximity to other bases


    // Start is called before the first frame update
    void Start()
    {
        baseDist = basePrefabs[0].GetComponent<BillionareBase>().shootDistance + baseDist;

        basePositions = new Vector3[basePrefabs.Length];
        blockPlacements = new bool[tileMapSizeY, tileMapSizeX];
        GenerateGrid();
        DrawLevel();
        StartCoroutine(PlaceBases());
    }


    // draw tiles into tilemap based on what's in blockPlacements - place a tile wherever the value is true
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



    // procedurally generates the layout of the level to blockPlacements
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
            if (spaceLeft < 4)
            {
                yAmt += spaceLeft;
                spaceLeft = 0;
            }
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
            if (spaceLeft < 4)
            {
                yAmt += spaceLeft;
                spaceLeft = 0;
            }

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
            if (spaceLeft < 4)
            {
                xAmt += spaceLeft;
                spaceLeft = 0;
            }

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
            if (spaceLeft < 4)
            {
                xAmt += spaceLeft;
                spaceLeft = 0;
            }

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


    // Coroutine to place bases using the AttemptToPlaceBase function - uses waits to make sure that the bases know where the others are when being placed
    IEnumerator PlaceBases()
    {
        yield return new WaitForEndOfFrame();
        AttemptToPlaceBase(0, 0, tileMapSizeY / 2, 0, 1, -1); // top left corner
        yield return new WaitForEndOfFrame();
        AttemptToPlaceBase(1, tileMapSizeY-1, tileMapSizeY / 2, 0, -1, -1); // bottom left corner
        yield return new WaitForEndOfFrame();
        AttemptToPlaceBase(2, 0, tileMapSizeY / 2, tileMapSizeX-1, 1, 1); // top right corner
        yield return new WaitForEndOfFrame();
        AttemptToPlaceBase(3, tileMapSizeY-1, tileMapSizeY / 2, tileMapSizeX-1, -1, 1); // bottom right corner
    }


    /**
     * Method that sweeps in diagonals to check for the best place to place a base
     * 
     * @param prefabIndex - the index in basePrefabs array of the base we want to place
     * @param startingY - the index in blockPlacements of the y value we want to start the sweep at, this will be either 0 or tileMapSizeY-1 to indicate starting at either the top or bottom
     * @param endingY - the index in blockPlacements of the y value we want to end the sweep at, this will generally be tileMapSizeY/2
     * @param startingX - the index in blockPlacements of the x value we want to start at, this will be either 0 or tileMapSizeX-1 to indicate starting at either the left or right edge
     * @param yDir - the direction we are moving through indices in the y direction - 1 if we are going top to bottom, -1 if we are going bottom to top
     * @param xDir - the direction we are moving through indices in the x direction - 1 if we are going left to right, -1 if we are going right to left;
     */
    private void AttemptToPlaceBase(int prefabIndex, int startingY, int endingY, int startingX, int yDir, int xDir)
    {
        // used as a failsafe so that the base will still spawn if there is not enough distance from other bases - will try to spawn as far away from an enemy base as possible
        Vector3 bestAvailablePosition = Vector3.zero;
        bestAvailablePosition.z = -100;
        float maxDistFromBase = 0;

        //Debug.Log($"Scanning for {prefabIndex}");

        // try placing it in every tile starting from the top left and moving in diagonals until we get to the vertical center;
        bool placed = false;
        for (int i = 0; i != endingY-startingY; i += yDir)
        {
            int r = startingY;
            int c = startingX - (Mathf.Abs(i) * xDir);
            while (r != startingY+i + yDir)
            {
                //Debug.Log($"{r}, {c}");

                // check that there isn't a wall there
                if (!blockPlacements[r, c])
                {
                    // get the worldspace position of the current tile
                    Vector3 attemptedPlacePos = tilemap.GetCellCenterWorld(new Vector3Int(c - tilemapCenterInGrid.x + 1, tilemapCenterInGrid.y - r - 2, 0));

                    // check that the base will not be too close to a wall
                    List<Collider2D> overlaps = new List<Collider2D>();
                    ContactFilter2D cf = new ContactFilter2D();
                    Physics2D.OverlapCircle(attemptedPlacePos, wallDist, cf.NoFilter(), overlaps);

                    bool overlapsWall = false;
                    for (int j = 0; j < overlaps.Count; j++)
                    {
                        if (overlaps[j].gameObject.CompareTag("wall"))
                        {
                            overlapsWall = true;
                            break;
                        }
                    }

                    if (!overlapsWall)
                    {
                        Debug.Log("Passed first check");

                        // check that the base will not be too close to another base
                        overlaps = new List<Collider2D>();
                        Physics2D.OverlapCircle(attemptedPlacePos, baseDist, cf.NoFilter(), overlaps);

                        bool overlapsBase = false;
                        for (int j = 0; j < overlaps.Count; j++)
                        {
                            if (overlaps[j].gameObject.CompareTag("base"))
                            {
                                overlapsBase = true;

                                // check if this is the farthest possible distance from another base
                                if (Vector3.Distance(attemptedPlacePos, overlaps[j].gameObject.transform.position) > maxDistFromBase)
                                {
                                    bestAvailablePosition = attemptedPlacePos;
                                    maxDistFromBase = Vector3.Distance(attemptedPlacePos, overlaps[j].gameObject.transform.position);
                                }
                            }
                        }

                        // if it isn't too close to a base, spawn base and exit loop
                        if (!overlapsBase)
                        {
                            Debug.Log($"Base {prefabIndex} placed properly");

                            // instantiate base
                            GameObject createdBase = Instantiate(basePrefabs[prefabIndex]);
                            createdBase.transform.position = attemptedPlacePos;
                            basePositions[prefabIndex] = attemptedPlacePos;
                            //Debug.Log(attemptedPlacePos);

                            placed = true;
                            return;
                        }
                    }
                }

                // move in a diagonal
                r += yDir;
                c += xDir;
            }
        }


        // continue moving through the quadrant in diagonals
        for (int i = 0; i != tileMapSizeX/2 - Mathf.Abs(endingY - startingY); i += 1)
        {
            int r = startingY;
            int c = startingX - ((i+Mathf.Abs(endingY-startingY)) * (xDir));
            while (r != endingY+yDir)
            {
                //Debug.Log($"{r}, {c}");

                // check that there isn't a wall there
                if (!blockPlacements[r, c])
                {
                    // get the worldspace position of the current tile
                    Vector3 attemptedPlacePos = tilemap.GetCellCenterWorld(new Vector3Int(c - tilemapCenterInGrid.x + 1, tilemapCenterInGrid.y - r - 2, 0));

                    // check that the base will not be too close to a wall
                    List<Collider2D> overlaps = new List<Collider2D>();
                    ContactFilter2D cf = new ContactFilter2D();
                    Physics2D.OverlapCircle(attemptedPlacePos, wallDist, cf.NoFilter(), overlaps);

                    bool overlapsWall = false;
                    for (int j = 0; j < overlaps.Count; j++)
                    {
                        if (overlaps[j].gameObject.CompareTag("wall"))
                        {
                            overlapsWall = true;
                            break;
                        }
                    }

                    if (!overlapsWall)
                    {
                        //Debug.Log("Passed first check");

                        // check that the base will not be too close to another base
                        overlaps = new List<Collider2D>();
                        Physics2D.OverlapCircle(attemptedPlacePos, baseDist, cf.NoFilter(), overlaps);

                        bool overlapsBase = false;
                        for (int j = 0; j < overlaps.Count; j++)
                        {
                            if (overlaps[j].gameObject.CompareTag("base"))
                            {
                                overlapsBase = true;

                                // check if this is the farthest possible distance from another base
                                if (Vector3.Distance(attemptedPlacePos, overlaps[j].gameObject.transform.position) > maxDistFromBase)
                                {
                                    bestAvailablePosition = attemptedPlacePos;
                                    maxDistFromBase = Vector3.Distance(attemptedPlacePos, overlaps[j].gameObject.transform.position);
                                }
                            }
                        }

                        // if it isn't too close to a base, spawn base and exit loop
                        if (!overlapsBase)
                        {
                            Debug.Log($"Base {prefabIndex} placed properly");

                            // instantiate base
                            GameObject createdBase = Instantiate(basePrefabs[prefabIndex]);
                            createdBase.transform.position = attemptedPlacePos;
                            basePositions[prefabIndex] = attemptedPlacePos;
                            //Debug.Log(attemptedPlacePos);

                            placed = true;
                            return;
                        }
                    }
                }

                // move in a diagonal
                r += yDir;
                c += xDir;
            }
        }


            // if the base wasn't placed, put it at the best position possible
            if (!placed)
        {
            Debug.Log($"Base {prefabIndex} placed with best guess");

            GameObject createdBase = Instantiate(basePrefabs[prefabIndex]);
            createdBase.transform.position = bestAvailablePosition;
            basePositions[prefabIndex] = bestAvailablePosition;
        }
    }


    // Draw gizmos to visualize radius around bases to see if they are placed appropriately
    private void OnDrawGizmos()
    {
        if (basePositions == null)
        {
            return;
        }

        for (int i=0; i<basePositions.Length; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(basePositions[i], wallDist);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(basePositions[i], baseDist);

        }
    }
}
