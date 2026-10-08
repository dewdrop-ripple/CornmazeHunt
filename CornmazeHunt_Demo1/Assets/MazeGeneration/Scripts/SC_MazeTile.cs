using System.Linq;
using UnityEngine;

public class SC_MazeTile : MonoBehaviour
{
    public enum TileType // Used to determine the attached object
    {
        WALKABLE = 0,
        WALL,
        PARKING_LOT,
        HUNTED_SPAWN,
        KILLER_SPAWN,
        COLLECTABLE_SPAWN,
        COLLECTABLE_COLLECTED,
        SCARECROW
    }

    public enum GenerationStatus // Used during DFS generation
    { 
        NOT_EXPLORED = 0,
        VISITED,
        FULLY_EXPLORED,
        NOT_APPLICABLE
    }

    [SerializeField] public int index;

    [SerializeField] private TileType mType = TileType.WALL;
    [SerializeField] private GenerationStatus mGenerationStatus = GenerationStatus.NOT_APPLICABLE;

    [SerializeField] private GameObject mAttachedObject;

    [SerializeField] private bool mIsFilledIn = false;

    public void SetType(TileType type) { mType = type; }
    public TileType GetTileType() { return mType; }

    public void SetGenerationStatus(GenerationStatus status) { mGenerationStatus = status; }
    public GenerationStatus GetGenerationStatus() { return mGenerationStatus; }

    public Vector3 GetTileSize() { return mAttachedObject.transform.lossyScale; }
    public void SetTileSize(Vector3 size) { mAttachedObject.transform.localScale = size; }
    public GameObject GetAttachedObject() { return mAttachedObject; }

    public bool IsFilledIn() { return mIsFilledIn; }
    public void SetFilledIn(bool isFilledIn) { mIsFilledIn = isFilledIn; }
}
