using UnityEngine;

public class SC_MazePlayerSetting : MonoBehaviour
{
    public enum Difficulty
    {
        EASY = 0,
        MEDIUM,
        HARD
    }

    [SerializeField] private int mPlayers;
    [SerializeField] private Difficulty mDifficulty;

    private const int CM_MIN_PLAYERS = 2;
    private const int CM_MAX_PLAYERS = 8;

    [SerializeField] private int CM_EASY_GRID_SIZE = 6;
    [SerializeField] private int CM_MEDIUM_GRID_SIZE = 8;
    [SerializeField] private int CM_HARD_GRID_SIZE = 10;

    [SerializeField] private Vector2 CM_2_PLAYER_SIZE = new Vector2(3, 4);
    [SerializeField] private Vector2 CM_3_PLAYER_SIZE = new Vector2(4, 4);
    [SerializeField] private Vector2 CM_4_PLAYER_SIZE = new Vector2(4, 5);
    [SerializeField] private Vector2 CM_5_PLAYER_SIZE = new Vector2(4, 5);
    [SerializeField] private Vector2 CM_6_PLAYER_SIZE = new Vector2(5, 5);
    [SerializeField] private Vector2 CM_7_PLAYER_SIZE = new Vector2(5, 5);
    [SerializeField] private Vector2 CM_8_PLAYER_SIZE = new Vector2(5, 6);

    [SerializeField] private Vector2 CM_COLLECTABLE_AREA_SIZE;
    [SerializeField] private Vector2 CM_CENTER_AREA_SIZE;
    [SerializeField] private Vector2 CM_CORNER_AREA_SIZE;

    // ----------
    // UTILITY FUNCTIONS

    public void SetNumPlayers(int numPlayers)
    {
        if (numPlayers < CM_MIN_PLAYERS) 
        { 
            mPlayers = CM_MIN_PLAYERS;
            return;
        }

        if (numPlayers > CM_MAX_PLAYERS) 
        { 
            mPlayers = CM_MAX_PLAYERS;
            return;
        }

        mPlayers = numPlayers;
    }

    public int GetNumPlayers() { return mPlayers; }

    public void SetDifficulty(Difficulty difficulty) { mDifficulty = difficulty; }
    public Difficulty GetDifficulty() { return mDifficulty; }

    // Returns the size in tiles of the large grid tiles (square, so width and height are the same)
    public int GetMazeGridSize()
    {
        switch (mDifficulty)
        {
            case Difficulty.HARD:
                return CM_HARD_GRID_SIZE;

            case Difficulty.MEDIUM:
                return CM_MEDIUM_GRID_SIZE;

            case Difficulty.EASY:
            default:
                return CM_EASY_GRID_SIZE;
        }
    }

    public Vector2 GetGridSpaces()
    {
        switch (mPlayers)
        {
            case 8:
                return CM_8_PLAYER_SIZE;

            case 7:
                return CM_7_PLAYER_SIZE;

            case 6:
                return CM_6_PLAYER_SIZE;

            case 5:
                return CM_5_PLAYER_SIZE;

            case 4:
                return CM_4_PLAYER_SIZE;

            case 3:
                return CM_3_PLAYER_SIZE;

            case 2:
            default:
                return CM_2_PLAYER_SIZE;
        }
    }

    // Returns the dimensions of the maze in tiles
    public Vector2 GetMazeSizeTiles()
    {
        return GetGridSpaces() * GetMazeGridSize();
    }

    public Vector2 GetCollectableAreaSize() { return CM_COLLECTABLE_AREA_SIZE; }
    public Vector2 GetCenterAreaSize() { return CM_CENTER_AREA_SIZE; }
    public Vector2 GetCornerAreaSize() { return CM_CORNER_AREA_SIZE; }

    public int GetNumCollectableAreas() { return mPlayers * 2; }
}
