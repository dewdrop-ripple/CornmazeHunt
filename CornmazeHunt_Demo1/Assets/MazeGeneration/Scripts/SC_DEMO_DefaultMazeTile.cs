using UnityEngine;

public class SC_DEMO_DefaultMazeTile : MonoBehaviour
{
    private Vector3 baseSize;
    private Vector3 wallSize;

    [SerializeField] private Material floorMaterial;
    [SerializeField] private Material wallMaterial;

    private SC_MazeTile mazeTile;

    private void Awake()
    {
        mazeTile = gameObject.GetComponent<SC_MazeTile>();
        baseSize = mazeTile.GetTileSize();
        wallSize = new Vector3(baseSize.x, baseSize.y * 5, baseSize.z);
    }

    private void Update()
    {
        if (mazeTile.GetTileType() == SC_MazeTile.TileType.WALL)
        {
            mazeTile.SetTileSize(wallSize);
            mazeTile.GetAttachedObject().GetComponent<Renderer>().material = wallMaterial;
        }
        else
        {
            mazeTile.SetTileSize(baseSize);
            mazeTile.GetAttachedObject().GetComponent<Renderer>().material = floorMaterial;
        }
    }
}
