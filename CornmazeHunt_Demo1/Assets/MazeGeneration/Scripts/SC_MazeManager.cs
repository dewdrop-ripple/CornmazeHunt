using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.InputSystem.InputSettings;

public class SC_MazeManager : MonoBehaviour
{
    public enum GameMode
    {
        COLLECT,
        ESCAPE
    }

    private enum AreaType
    {
        NONE,
        COLLECTABLE,
        KILLER_SPAWN,
        HUNTED_SPAWN,
        SCARECROW
    }


    [SerializeField] private GameMode mGameMode = GameMode.COLLECT;

    [SerializeField] private float mBorderWidth;

    [SerializeField] private List<SC_MazeTile> mAllTiles = new List<SC_MazeTile>();

    private SC_GameManager mManager;

    [SerializeField] private GameObject mDefaultTile;

    [SerializeField] private GameObject mWalkableTilePrefab;
    [SerializeField] private GameObject mWallTilePrefab;
    [SerializeField] private GameObject mParkingLotTilePrefab;
    [SerializeField] private GameObject mHuntedSpawnTilePrefab_Collect;
    [SerializeField] private GameObject mHuntedSpawnTilePrefab_Escape;
    [SerializeField] private GameObject mKillerSpawnTilePrefab;
    [SerializeField] private GameObject mCollectableSpawnTilePrefab;
    [SerializeField] private GameObject mScarecrowTilePrefab;
    [SerializeField] private GameObject mExteriorTilePrefab;


    private Stack<int> mExplorationStack = new Stack<int>();

    private Vector2 mMazeSize;


    private void Start()
    {
        mManager = FindAnyObjectByType<SC_GameManager>();

        ClearMaze();
        GenerateObjects();

        if (mGameMode == GameMode.COLLECT)
        {
            GenerateKeyAreasCollect();
        }
        else
        {
            GenerateKeyAreasEscape();
        }

        DFS();
        ReplaceModels();

        CreateBorders();
    }

    private void ClearMaze()
    {
        for (int i = mAllTiles.Count - 1; i >= 0; i--)
        {
            Destroy(mAllTiles[i].gameObject);
        }

        mAllTiles.Clear();
    }

    private void GenerateObjects()
    {
        // The actual size of the grid, taking into account walls, will be double the number of walkable tiles in each direction + 1
        mMazeSize = mManager.GetMazePlayerSetting().GetMazeSizeTiles() * 2;
        mMazeSize.x++;
        mMazeSize.y++;

        // All tiles are the same size, so the size of one is the size of all
        float tileSize = mDefaultTile.GetComponent<SC_MazeTile>().GetTileSize().x;

        // Create tiles
        for (int i = 0; i < mMazeSize.x; i++)
        {
            for (int j = 0; j < mMazeSize.y; j++)
            {
                GameObject target = Instantiate(mDefaultTile);

                target.transform.position = new Vector3(i * tileSize, 0, j * tileSize);

                // Odd number of tiles, starting at 0, means odd/odd tiles are the base walkable tiles
                if (i % 2 == 1 && j % 2 == 1)
                {
                    target.GetComponent<SC_MazeTile>().SetType(SC_MazeTile.TileType.WALKABLE);
                    target.GetComponent<SC_MazeTile>().SetGenerationStatus(SC_MazeTile.GenerationStatus.NOT_EXPLORED);
                }
                // Other tiles are walls
                else
                {
                    target.GetComponent<SC_MazeTile>().SetType(SC_MazeTile.TileType.WALL);
                    target.GetComponent<SC_MazeTile>().SetGenerationStatus(SC_MazeTile.GenerationStatus.NOT_APPLICABLE);
                }

                target.GetComponent<SC_MazeTile>().index = mAllTiles.Count;
                mAllTiles.Add(target.GetComponent<SC_MazeTile>());
            }
        }
    }

    private void GenerateKeyAreasEscape()
    {
        List<Vector2> possibleGridSpaces = new List<Vector2>();
        for (int i = 0; i < mManager.GetMazePlayerSetting().GetGridSpaces().x; i++)
        {
            for (int j = 0; j < mManager.GetMazePlayerSetting().GetGridSpaces().y; j++)
            {
                possibleGridSpaces.Add(new Vector2(i, j));
            }
        }

        // CORNER AREA 1 & 2
        int rand = Mathf.FloorToInt(Random.Range(0, 1.99f));

        if (rand == 0)
        {
            CreateArea(new Vector2(0, 0), (int)mManager.GetMazePlayerSetting().GetCornerAreaSize().x, 2, AreaType.SCARECROW);
            possibleGridSpaces.Remove(new Vector2(0, 0));

            CreateArea(new Vector2(0, mManager.GetMazePlayerSetting().GetGridSpaces().y - 1), (int)mManager.GetMazePlayerSetting().GetCornerAreaSize().x, 2, AreaType.KILLER_SPAWN);
            possibleGridSpaces.Remove(new Vector2(0, mManager.GetMazePlayerSetting().GetGridSpaces().y - 1));
        }
        else
        {
            CreateArea(new Vector2(0, 0), (int)mManager.GetMazePlayerSetting().GetCornerAreaSize().x, 2, AreaType.KILLER_SPAWN);
            possibleGridSpaces.Remove(new Vector2(0, 0));

            CreateArea(new Vector2(0, mManager.GetMazePlayerSetting().GetGridSpaces().y - 1), (int)mManager.GetMazePlayerSetting().GetCornerAreaSize().x, 2, AreaType.SCARECROW);
            possibleGridSpaces.Remove(new Vector2(0, mManager.GetMazePlayerSetting().GetGridSpaces().y - 1));
        }

        // CENTER AREA 
        CreateArea(new Vector2((int)mManager.GetMazePlayerSetting().GetGridSpaces().x / 2, (int)mManager.GetMazePlayerSetting().GetGridSpaces().y / 2), (int)mManager.GetMazePlayerSetting().GetCenterAreaSize().x, 4, AreaType.HUNTED_SPAWN);
        possibleGridSpaces.Remove(new Vector2((int)mManager.GetMazePlayerSetting().GetGridSpaces().x / 2, (int)mManager.GetMazePlayerSetting().GetGridSpaces().y / 2));

        // EXITS
        List<Vector2> exitTiles = new List<Vector2>();

        for (int i = 0; i < mManager.GetMazePlayerSetting().GetGridSpaces().y; i++)
        {
            exitTiles.Add(new Vector2(mManager.GetMazePlayerSetting().GetGridSpaces().x - 1, i));
        }

        for (int i = 0; i < 3; i++)
        {
            int target = Mathf.FloorToInt(Random.Range(0, exitTiles.Count - 0.01f));
            CreateExit(exitTiles[target]);
            possibleGridSpaces.Remove(exitTiles[target]);
            exitTiles.Remove(exitTiles[target]);
        }

        // COLLECTABLE AREAS
        for (int i = 0; i < Mathf.FloorToInt(mManager.GetMazePlayerSetting().GetNumCollectableAreas() / 2); i++)
        {
            int target = Mathf.FloorToInt(Random.Range(0, possibleGridSpaces.Count - 0.01f));
            CreateArea(possibleGridSpaces[target], (int)mManager.GetMazePlayerSetting().GetCollectableAreaSize().x, 1, AreaType.NONE);
            possibleGridSpaces.Remove(possibleGridSpaces[target]);
        }
        for (int i = 0; i < Mathf.CeilToInt(mManager.GetMazePlayerSetting().GetNumCollectableAreas() / 2); i++)
        {
            int target = Mathf.FloorToInt(Random.Range(0, possibleGridSpaces.Count - 0.01f));
            CreateArea(possibleGridSpaces[target], (int)mManager.GetMazePlayerSetting().GetCollectableAreaSize().x, 1, AreaType.SCARECROW);
            possibleGridSpaces.Remove(possibleGridSpaces[target]);
        }
    }

    private void GenerateKeyAreasCollect()
    {
        List<Vector2> possibleGridSpaces = new List<Vector2>();
        for (int i = 0; i < mManager.GetMazePlayerSetting().GetGridSpaces().x; i++)
        {
            for (int j = 0; j < mManager.GetMazePlayerSetting().GetGridSpaces().y; j++)
            {
                possibleGridSpaces.Add(new Vector2(i, j));
            }
        }

        // CORNER AREA 1 & 2
        CreateArea(new Vector2(0, 0), (int)mManager.GetMazePlayerSetting().GetCornerAreaSize().x, 2, AreaType.SCARECROW);
        possibleGridSpaces.Remove(new Vector2(0, 0));

        CreateArea(new Vector2(0, mManager.GetMazePlayerSetting().GetGridSpaces().y - 1), (int)mManager.GetMazePlayerSetting().GetCornerAreaSize().x, 2, AreaType.SCARECROW);
        possibleGridSpaces.Remove(new Vector2(0, mManager.GetMazePlayerSetting().GetGridSpaces().y - 1));

        // CENTER AREA 
        CreateArea(new Vector2((int)mManager.GetMazePlayerSetting().GetGridSpaces().x / 2, (int)mManager.GetMazePlayerSetting().GetGridSpaces().y / 2), (int)mManager.GetMazePlayerSetting().GetCenterAreaSize().x, 4, AreaType.KILLER_SPAWN);
        possibleGridSpaces.Remove(new Vector2((int)mManager.GetMazePlayerSetting().GetGridSpaces().x / 2, (int)mManager.GetMazePlayerSetting().GetGridSpaces().y / 2));

        // EXITS
        List<Vector2> exitTiles = new List<Vector2>();

        for (int i = 0; i < mManager.GetMazePlayerSetting().GetGridSpaces().y; i++)
        {
            exitTiles.Add(new Vector2(mManager.GetMazePlayerSetting().GetGridSpaces().x - 1, i));
        }

        for (int i = 0; i < 3; i++)
        {
            int target = Mathf.FloorToInt(Random.Range(0, exitTiles.Count - 0.01f));
            CreateExit(exitTiles[target]);
            possibleGridSpaces.Remove(exitTiles[target]);
            exitTiles.Remove(exitTiles[target]);
        }

        // COLLECTABLE AREAS
        for (int i = 0; i < mManager.GetMazePlayerSetting().GetNumCollectableAreas(); i++)
        {
            int target = Mathf.FloorToInt(Random.Range(0, possibleGridSpaces.Count - 0.01f));
            CreateArea(possibleGridSpaces[target], (int)mManager.GetMazePlayerSetting().GetCollectableAreaSize().x, 1, AreaType.COLLECTABLE);
            possibleGridSpaces.Remove(possibleGridSpaces[target]);
        }
    }

    private void CreateArea(Vector2 location, int size, int exits, AreaType type)
    {
        int gridSize = (mManager.GetMazePlayerSetting().GetMazeGridSize() * 2) - 1;

        Vector2 minSpace = new Vector2(location.x * (gridSize + 1) + 1, location.y * (gridSize + 1) + 1);
        Vector2 maxSpace = new Vector2(minSpace.x + (gridSize - size) - 1, minSpace.y + (gridSize - size) - 1);

        Vector2 topLeft = new Vector2(Mathf.FloorToInt(Random.Range(minSpace.x, maxSpace.x + 0.99f)),
            Mathf.FloorToInt(Random.Range(minSpace.y, maxSpace.y + 0.99f)));
        if (topLeft.x % 2 == 0) { topLeft.x++; }
        if (topLeft.y % 2 == 0) { topLeft.y++; }

        // Tiles
        for (int i = (int)topLeft.x; i < (int)topLeft.x + size; i++)
        {
            for (int j = (int)topLeft.y; j < (int)topLeft.y + size; j++)
            {
                int target = (int)(i * mMazeSize.y) + j;

                mAllTiles[target].SetType(SC_MazeTile.TileType.WALKABLE);
                mAllTiles[target].SetGenerationStatus(SC_MazeTile.GenerationStatus.FULLY_EXPLORED);
            }
        }

        // Special Tiles
        if (type != AreaType.NONE)
        {
            int x = (int)topLeft.x + (size / 2);
            int y = (int)topLeft.y + (size / 2);

            switch (type)
            {
                case AreaType.SCARECROW:
                    mAllTiles[(int)(x * mMazeSize.y) + y].SetType(SC_MazeTile.TileType.SCARECROW);
                    break;

                case AreaType.KILLER_SPAWN:
                    mAllTiles[(int)(x * mMazeSize.y) + y].SetType(SC_MazeTile.TileType.KILLER_SPAWN);
                    break;

                case AreaType.HUNTED_SPAWN:
                    mAllTiles[(int)(x * mMazeSize.y) + y].SetType(SC_MazeTile.TileType.HUNTED_SPAWN);
                    break;

                case AreaType.COLLECTABLE:
                default:
                    mAllTiles[(int)(x * mMazeSize.y) + y].SetType(SC_MazeTile.TileType.COLLECTABLE_SPAWN);
                    break;
            }

            
        }

        List<int> edges = new List<int>();

        // Edges
        for (int i = (int)topLeft.x - 1; i < (int)topLeft.x + size; i++)
        {
            int j = (int)topLeft.y - 1;

            if ((i % 2 == 1 || j % 2 == 1) && !IsEdgeWall((int)(i * mMazeSize.y) + j))
            {
                edges.Add((int)(i * mMazeSize.y) + j);
            }
        }

        for (int i = (int)topLeft.x - 1; i < (int)topLeft.x + size; i++)
        {
            int j = (int)(topLeft.y + size);

            if ((i % 2 == 1 || j % 2 == 1) && !IsEdgeWall((int)(i * mMazeSize.y) + j))
            {
                edges.Add((int)(i * mMazeSize.y) + j);
            }
        }

        for (int j = (int)topLeft.y - 1; j < (int)topLeft.y + size; j++)
        {
            int i = (int)topLeft.x - 1;

            if ((i % 2 == 1 || j % 2 == 1) && !IsEdgeWall((int)(i * mMazeSize.y) + j))
            {
                edges.Add((int)(i * mMazeSize.y) + j);
            }
        }

        for (int j = (int)topLeft.y - 1; j < (int)topLeft.y + size; j++)
        {
            int i = (int)(topLeft.x + size);

            if ((i % 2 == 1 || j % 2 == 1) && !IsEdgeWall((int)(i * mMazeSize.y) + j))
            {
                edges.Add((int)(i * mMazeSize.y) + j);
            }
        }

        // Exits
        for (int i = 0; i < exits; i++)
        {
            int target = Mathf.FloorToInt(Random.Range(0, edges.Count - 0.01f));
            mAllTiles[edges[target]].SetType(SC_MazeTile.TileType.WALKABLE);
            mAllTiles[edges[target]].SetGenerationStatus(SC_MazeTile.GenerationStatus.FULLY_EXPLORED);
            edges.RemoveAt(target);
        }
    }

    private void CreateExit(Vector2 location)
    {
        int target;
        int gridSize = (mManager.GetMazePlayerSetting().GetMazeGridSize() * 2) - 1;

        Vector2 minSpace = new Vector2(location.x * (gridSize + 1) + 1, location.y * (gridSize + 1) + 1);
        Vector2 maxSpace = new Vector2(minSpace.x + (gridSize - gridSize) - 1, minSpace.y + (gridSize - gridSize) - 1);

        Vector2 topLeft = new Vector2(Mathf.FloorToInt(Random.Range(minSpace.x, maxSpace.x + 0.99f)),
            Mathf.FloorToInt(Random.Range(minSpace.y, maxSpace.y + 0.99f)));
        if (topLeft.x % 2 == 0) { topLeft.x++; }
        if (topLeft.y % 2 == 0) { topLeft.y++; }

        List<int> edges = new List<int>();

        // Edges
        for (int j = (int)topLeft.y - 1; j < (int)topLeft.y + gridSize; j++)
        {
            int i = (int)(topLeft.x + gridSize);

            if ((i % 2 == 1 || j % 2 == 1) && IsEdgeWall((int)(i * mMazeSize.y) + j))
            {
                edges.Add((int)(i * mMazeSize.y) + j);
            }
        }

        target = Mathf.FloorToInt(Random.Range(0, edges.Count - 0.01f));

        mAllTiles[edges[target]].SetType(SC_MazeTile.TileType.WALKABLE);
        mAllTiles[edges[target]].SetGenerationStatus(SC_MazeTile.GenerationStatus.FULLY_EXPLORED);
    }

    private void DFS()
    {
        for (int i = 0; i < mAllTiles.Count; i++)
        {
            if (mAllTiles[i].GetGenerationStatus() == SC_MazeTile.GenerationStatus.NOT_EXPLORED)
            {
                mExplorationStack.Push(i);
                break;
            }
        }

        while (mExplorationStack.Count > 0)
        {
            int target = mExplorationStack.Peek();
            SC_MazeTile targetObject = mAllTiles[target];

            targetObject.SetGenerationStatus(SC_MazeTile.GenerationStatus.VISITED);

            List<int> neighbors = GetNeighbors(target);

            for (int i = neighbors.Count - 1; i >= 0; i--)
            {
                if (mAllTiles[neighbors[i]].GetGenerationStatus() != SC_MazeTile.GenerationStatus.NOT_EXPLORED)
                {
                    neighbors.RemoveAt(i);
                }
            }

            if (neighbors.Count == 0)
            {
                targetObject.SetGenerationStatus(SC_MazeTile.GenerationStatus.FULLY_EXPLORED);
                mExplorationStack.Pop();
            }
            else
            {
                int nextTile = (int)Mathf.Floor(Random.Range(0, neighbors.Count - 0.01f));

                int wall = GetWallBetween(target, neighbors[nextTile]);
                mAllTiles[wall].SetType(SC_MazeTile.TileType.WALKABLE);

                mExplorationStack.Push(neighbors[nextTile]);
            }
        }
    }

    private List<int> GetNeighbors(int tile)
    {
        List<int> result = new List<int>();

        if (tile % mMazeSize.y != 1) { result.Add(tile - 2); }
        if (tile % mMazeSize.y != mMazeSize.y - 2) { result.Add(tile + 2); }
        if (tile > mMazeSize.y * 2) { result.Add(tile - (int)(mMazeSize.y * 2)); }
        if (tile < mAllTiles.Count - (mMazeSize.y * 2)) { result.Add(tile + (int)(mMazeSize.y * 2)); }

        return result;
    }

    private bool IsEdgeWall(int tile)
    {
        return (tile % mMazeSize.y == 0) || (tile % mMazeSize.y == mMazeSize.y - 1)
            || (tile <= mMazeSize.y) || (tile >= mAllTiles.Count - (mMazeSize.y));
    }

    private int GetWallBetween(int targetTile, int otherTile)
    {
        if (otherTile == targetTile - 2) { return targetTile - 1; }
        if (otherTile == targetTile + 2) { return targetTile + 1; }
        if (otherTile == targetTile - (int)(mMazeSize.y * 2)) { return targetTile - (int)(mMazeSize.y); }
        if (otherTile == targetTile + (int)(mMazeSize.y * 2)) { return targetTile + (int)(mMazeSize.y); }

        return -1;
    }

    private void ReplaceModels()
    {
        for (int i = 0; i < mAllTiles.Count; i++)
        {
            Destroy(mAllTiles[i].GetAttachedObject());

            GameObject temp;
            float rotation;

            switch (mAllTiles[i].GetTileType())
            {
                case SC_MazeTile.TileType.WALL:
                    temp = Instantiate(mWallTilePrefab, mAllTiles[i].gameObject.transform);
                    break;

                case SC_MazeTile.TileType.HUNTED_SPAWN:
                    temp = Instantiate(mHuntedSpawnTilePrefab_Escape, mAllTiles[i].gameObject.transform);

                    rotation = Mathf.Floor(Random.Range(0, 3.99f)) * 90;
                    temp.transform.rotation = Quaternion.Euler(new Vector3(0, rotation, 0));
                    break;

                case SC_MazeTile.TileType.KILLER_SPAWN:
                    temp = Instantiate(mKillerSpawnTilePrefab, mAllTiles[i].gameObject.transform);

                    rotation = Mathf.Floor(Random.Range(0, 3.99f)) * 90;
                    temp.transform.rotation = Quaternion.Euler(new Vector3(0, rotation, 0));
                    break;

                case SC_MazeTile.TileType.COLLECTABLE_SPAWN:
                    temp = Instantiate(mCollectableSpawnTilePrefab, mAllTiles[i].gameObject.transform);
                    break;

                case SC_MazeTile.TileType.SCARECROW:
                    temp = Instantiate(mScarecrowTilePrefab, mAllTiles[i].gameObject.transform);

                    rotation = Mathf.Floor(Random.Range(0, 3.99f)) * 90;
                    temp.transform.rotation = Quaternion.Euler(new Vector3(0, rotation, 0));
                    break;

                case SC_MazeTile.TileType.WALKABLE:
                default:
                    temp = Instantiate(mWalkableTilePrefab, mAllTiles[i].gameObject.transform);
                    break;
            }

            temp.GetComponent<SC_TileMapData>().SetAttachedTile(mAllTiles[i]);
        }
    }

    private void CreateBorders()
    {
        float tileSize = mDefaultTile.GetComponent<SC_MazeTile>().GetAttachedObject().transform.lossyScale.x;
        float mazeWidth = (mManager.GetMazePlayerSetting().GetMazeSizeTiles().y * 2 + 1);
        float mazeHeight = (mManager.GetMazePlayerSetting().GetMazeSizeTiles().x * 2 + 1);

        GameObject temp;

        // Left area
        temp = Instantiate(mExteriorTilePrefab);
        temp.transform.localScale = new Vector3(mazeHeight + mBorderWidth + 2, 1, mBorderWidth);
        temp.transform.position = new Vector3((mazeHeight + mBorderWidth + 1) * (tileSize / 2), 0,(mBorderWidth + 1) * (tileSize / -2));

        // Right area
        temp = Instantiate(mExteriorTilePrefab);
        temp.transform.localScale = new Vector3(mazeHeight + mBorderWidth + 2, 1, mBorderWidth);
        temp.transform.position = new Vector3((mazeHeight + mBorderWidth + 1) * (tileSize / 2), 0, ((2 * mazeWidth) + mBorderWidth - 1) * (tileSize / 2));

        // Top area
        temp = Instantiate(mExteriorTilePrefab);
        temp.transform.localScale = new Vector3(mBorderWidth, 1, mazeWidth + (mBorderWidth * 2));
        temp.transform.position = new Vector3((mBorderWidth + 1) * (tileSize / -2), 0, (mazeWidth - 1) * (tileSize / 2));

        // Bottom area 1
        temp = Instantiate(mParkingLotTilePrefab);
        temp.transform.localScale = new Vector3(1, 1, mazeWidth);
        temp.transform.position = new Vector3(mazeHeight * tileSize, 0, (mazeWidth - 1) * (tileSize / 2));

        // Bottom area 2
        temp = Instantiate(mExteriorTilePrefab);
        temp.transform.localScale = new Vector3(3, 1, mazeWidth);
        temp.transform.position = new Vector3((mBorderWidth + mazeHeight) * tileSize, 0, (mazeWidth - 1) * (tileSize / 2));

        // Parking area 1
        if (mGameMode == GameMode.ESCAPE)
        {
            temp = Instantiate(mParkingLotTilePrefab);
            temp.transform.localScale = new Vector3(1, 1, mazeWidth);
            temp.transform.position = new Vector3(tileSize * (mazeHeight + 1), 0, (mazeWidth - 1) * (tileSize / 2));
        }
        else
        {
            float w = ((mazeWidth - 3) / 4);
            float x = tileSize * (mazeHeight + 1);

            float currentW = (w - 1) * tileSize / 2;

            for (int i = 0; i < 3; i++)
            {
                temp = Instantiate(mParkingLotTilePrefab);
                temp.transform.localScale = new Vector3(1, 1, w);
                temp.transform.position = new Vector3(x, 0, currentW);

                currentW += tileSize * (1 + w);

                temp = Instantiate(mHuntedSpawnTilePrefab_Collect);
                temp.transform.position = new Vector3(x, 0, currentW - ((tileSize / 2) * (w + 1)));
            }

            temp = Instantiate(mParkingLotTilePrefab);
            temp.transform.localScale = new Vector3(1, 1, w);
            temp.transform.position = new Vector3(x, 0, currentW);
        }

        // Parking area 2
        temp = Instantiate(mParkingLotTilePrefab);
        temp.transform.localScale = new Vector3(mBorderWidth - 3, 1, mazeWidth);
        temp.transform.position = new Vector3((tileSize / 2) * (mBorderWidth + (mazeHeight * 2)), 0, (mazeWidth - 1) * (tileSize / 2));
    }

    public float GetTileSize() { return mDefaultTile.GetComponent<SC_MazeTile>().GetAttachedObject().transform.lossyScale.x; }
}
