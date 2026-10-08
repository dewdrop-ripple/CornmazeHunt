using UnityEngine;

public class SC_TileMapData : MonoBehaviour
{
    [SerializeField] public GameObject mBlock;
    [SerializeField] public GameObject mMapColor;
    [SerializeField] public Material mCollectedCollectableColor;

    [SerializeField] public SC_MazeTile mAttachedTile;

    private void Update()
    {
        if (mAttachedTile == null)
        {
            return;
        }

        if (mAttachedTile.IsFilledIn())
        {
            mBlock.GetComponent<Renderer>().enabled = false;
        }
        else
        {
            mBlock.GetComponent<Renderer>().enabled = true;
        }

        if (mAttachedTile.GetTileType() == SC_MazeTile.TileType.COLLECTABLE_COLLECTED)
        {
            mMapColor.GetComponent<Renderer>().material = mCollectedCollectableColor;
        }
    }

    public void SetAttachedTile(SC_MazeTile tile) { mAttachedTile = tile; }
}
