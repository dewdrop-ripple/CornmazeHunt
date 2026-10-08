using UnityEngine;

public class SC_MapCamera : MonoBehaviour
{
    [SerializeField] private GameObject mAttachedPlayer;

    private void Update()
    {
        transform.position = new Vector3(mAttachedPlayer.transform.position.x, 100, mAttachedPlayer.transform.position.z);
    }
}
