using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameMap : MonoBehaviour
{

    public Tilemap[] tileAreas;
    public Tilemap[] tileLocations;

    public Tilemap playerMovement;

    public string[] blockAreas;

    private List<string> allowedAreas = new List<string>();

    [HideInInspector]
    public PhotonView photonView;

    void Awake()
    {
        SetAllowedAreas("All");
        // Getting photon view from map
        photonView = transform.GetComponent<PhotonView>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public void SetAllowedAreas(params string[] allowedAreas) {
        this.allowedAreas = allowedAreas.ToList();
    }

    public BuildInfo Build(int playerId, Vector3 mousePosition, bool adjacent)
    {
        foreach (Tilemap tilemap in tileAreas)
        {
            // Skip if area is blocked
            if (blockAreas.Contains(tilemap.name)) {
                continue;
            }
            
            // Skip if area is not allowed
            if (allowedAreas.Count > 0 && (!allowedAreas.Contains("All") && !allowedAreas.Contains(tilemap.name))) {
                Debug.Log(string.Format("Not allowed to set a tile at area {0}", tilemap.name));
                continue;
            }

            Vector3Int point = GetSelectedPoint(tilemap, mousePosition);
            // Skip if not have any building around
            if (adjacent && !HasBuildingAround(playerMovement, point)) {
                Debug.Log(string.Format("Not allowed to set a tile at area {0} in ({1}, {2}). Must adjacent with other build tile", tilemap.name, point.x, point.y));
                continue;
            }

            // Check if can move into this tile
            if (tilemap.HasTile(point) && !playerMovement.HasTile(point) && !HasLocationTiles(point))
            {
                // Game sync, publish a set tile into game server
                photonView.RPC("SetSettlement", RpcTarget.All, point.x, point.y, playerId);

                // Search for location tiles around
                List<Tilemap> tileLocations = GetTileLocationAround(point);
                if (tileLocations.Count > 0)
                    return new BuildInfo(point.x, point.y, tilemap.name, tileLocations);
                else 
                    return new BuildInfo(point.x, point.y, tilemap.name);
            }
        }
        // Cant't found the tile
        return null;
    }

    [PunRPC]
    public void SetSettlement(int x, int y, int playerId, PhotonMessageInfo info) {
        Debug.LogFormat("Message Info: {0} {1} {2}", info.Sender, info.photonView, info.SentServerTime);

        GameObject playerObject = PlayerRegister.GetPlayer(playerId);
        Player player = playerObject.GetComponent<Player>();
        if (player != null) 
        {
            Vector3Int playerPosition = new Vector3Int(x, y, 0); // Ingore "z" value

            Tile settlementTile = player.playerTile;
            // Adjust saturation color
            settlementTile.color = AdjustColor(player.playerColor, 0.3f, 1);
            playerMovement.SetTile(playerPosition, settlementTile);
        }
        else {
            Debug.Log(string.Format("Can't set settlement in ({0}, {1}). Player id {2} is not found", x, y, playerId));
        }
    }

    [PunRPC]
    public void RemoveSettlements(int[] x, int[] y, PhotonMessageInfo info)
    {
        Debug.LogFormat("Message Info: {0} {1} {2}", info.Sender, info.photonView, info.SentServerTime);

        if (x.Length != y.Length)
        {
            Debug.Log("Can't remove settlements. X and Y positions is not match");
        }
        else {
            for (int i = 0; i < x.Length; i++) {
                Vector3Int playerPosition = new Vector3Int(x[i], y[i], 0); // Ignore "z" value
                if (!playerMovement.HasTile(playerPosition))
                {
                    continue;
                }
                // Remove tile at Player Position
                playerMovement.SetTile(playerPosition, null);
            }
        
        }
    }

    [PunRPC]
    public void RemoveAllSettlements(PhotonMessageInfo info)
    {
        Debug.LogFormat("Message Info: {0} {1} {2}", info.Sender, info.photonView, info.SentServerTime);

        playerMovement.ClearAllTiles();
    }

    public List<BuildInfo> GetSettlements(Color playerColor)
    {
        List<BuildInfo> result = new List<BuildInfo>();
        if (playerMovement == null)
        {
            return result;
        }

        BoundsInt bounds = playerMovement.cellBounds;
        for (int x = bounds.x; x < bounds.x + bounds.size.x; x++)
        {
            for (int y = bounds.y; y < bounds.y + bounds.size.y; y++)
            {
                Vector3Int point = new Vector3Int(x, y, 0);
                if (HasPlayerStep(playerColor, point)) {
                    List<Tilemap> tileLocations = GetTileLocationAround(point);
                    // Get all adjacent block area for this step
                    Dictionary<string, bool> adjacentBlockAreas = GetAdjacentBlockAreaAround(point);
                    // Get area for this step
                    string area = GetTileArea(point);

                    BuildInfo settlement;
                    if (tileLocations.Count > 0)
                        settlement = new BuildInfo(point.x, point.y, area, tileLocations, adjacentBlockAreas);
                    else
                        settlement = new BuildInfo(point.x, point.y, area, adjacentBlockAreas);

                    result.Add(settlement);
                }
            }
        }
        return result;
    }

    private bool HasPlayerStep(Color playerColor, Vector3Int position)
    {
        if (playerColor == null) return false;
        return playerMovement.HasTile(position) && GetHue(playerMovement.GetColor(position)) == GetHue(playerColor);
    }

    private Dictionary<string, bool> GetAdjacentBlockAreaAround(Vector3Int playerPosition) {
        Dictionary<string, bool> result = new Dictionary<string, bool>();
        foreach (Tilemap tilemap in tileAreas) {
            // Skip if area is non blocked
            if (!blockAreas.Contains(tilemap.name))
            {
                continue;
            }
            // Check if adjacent with block area
            if (HasBuildingAround(tilemap, playerPosition))
            {
                result.Add(tilemap.name, true);
            }
            else {
                result.Add(tilemap.name, false);
            }
        }
        return result;
    }

    private List<Tilemap> GetTileLocationAround(Vector3Int point) {
        List<Tilemap> resulTileLocations = new List<Tilemap>();
        foreach (Tilemap tileLocation in tileLocations) {
            if (HasBuildingAround(tileLocation, point)) {
                resulTileLocations.Add(tileLocation);
            }
        
        }
        return resulTileLocations;
    }

    private string GetTileArea(Vector3Int point) {
        foreach (Tilemap tileArea in tileAreas) {
            if (tileArea.HasTile(point)) return tileArea.name;
        }
        // Area is not found
        return "";
    }

    private bool HasBuildingAround(Tilemap tilemap, Vector3Int point) {
        // Check the left side
        Vector3Int pointLeft = point + Vector3Int.left;
        bool left = tilemap.HasTile(pointLeft);
        // Check the right side
        Vector3Int pointRight = point + Vector3Int.right;
        bool right = tilemap.HasTile(pointRight);
        // Check the other sides
        bool upperLeft, bottomLeft, upperRight, bottomRight;
        if (point.y % 2 == 0)
        {
            upperLeft = tilemap.HasTile(pointLeft + Vector3Int.up);
            bottomLeft = tilemap.HasTile(pointLeft + Vector3Int.down);
            upperRight = tilemap.HasTile(point + Vector3Int.up);
            bottomRight = tilemap.HasTile(point + Vector3Int.down);
        }
        else 
        { 
            upperLeft = tilemap.HasTile(point + Vector3Int.up);
            bottomLeft = tilemap.HasTile(point + Vector3Int.down);
            upperRight = tilemap.HasTile(pointRight + Vector3Int.up);
            bottomRight = tilemap.HasTile(pointRight + Vector3Int.down);
        }

        return left || upperLeft || bottomLeft || right || upperRight || bottomRight;
    }

    private bool HasLocationTiles(Vector3Int point) {
        foreach (Tilemap tilemap in tileLocations) {
            if (tilemap.HasTile(point)) return true;
        }

        return false;
    }

    private Vector3Int GetSelectedPoint(Tilemap tilemap, Vector3 mousePosition)
    {
        // Get cell position from mouse position
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
        // Set player tile at the cell position
        return new Vector3Int(cellPosition.x, cellPosition.y, 0); // Ignore "z" value
    }

    private Color AdjustColor(Color color, float saturation, float brightness)
    {
        float H, S, V;
        Color.RGBToHSV(color, out H, out S, out V);
        return Color.HSVToRGB(H, S * saturation, V * brightness);
    }

    public int GetHue(Color color)
    {

        float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);
        float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
        if (min == max)
        {
            return 0;
        }
        float hue;
        if (max == color.r)
        {
            hue = (color.g - color.b) / (max - min);
        }
        else if (max == color.g)
        {
            hue = 2f + (color.b - color.r) / (max - min);
        }
        else
        {
            hue = 4f + (color.r - color.g) / (max - min);
        }

        hue = hue * 60;
        if (hue < 0) hue = hue + 360;

        return Mathf.RoundToInt(hue);
    }

    // Update is called once per frame
    void Update()
    {

    }

}
