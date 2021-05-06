using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildInfo
{
    public int x;
    public int y;
    public string area;
    // Tile locations around
    public List<Tilemap> tileLocationsAround;
    // Result check adjacent with block areas; Ex : Water -> True
    public Dictionary<string, bool> adjacentBlockAreas;
    public BuildInfo()
    {
        // Empty construction
    }

    public BuildInfo(int x, int y, string area) : this(x, y, area, new Dictionary<string, bool>())
    {
    }

    public BuildInfo(int x, int y, string area, Dictionary<string, bool> adjacentBlockAreas) : this(x, y, area, new List<Tilemap>(), adjacentBlockAreas)
    {
        
    }

    public BuildInfo(int x, int y, string area, List<Tilemap> tileLocationsAround) : this (x, y, area, tileLocationsAround, new Dictionary<string, bool>())
    {
    }

    public BuildInfo(int x, int y, string area, List<Tilemap> tileLocationsAround, Dictionary<string, bool> adjacentBlockAreas)
    {
        this.x = x;
        this.y = y;
        this.area = area;
        this.tileLocationsAround = tileLocationsAround;
        this.adjacentBlockAreas = adjacentBlockAreas;
    }

}
