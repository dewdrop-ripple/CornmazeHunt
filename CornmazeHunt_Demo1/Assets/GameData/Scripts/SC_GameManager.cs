using UnityEngine;

public class SC_GameManager : MonoBehaviour
{
    [SerializeField] private GameObject mPlayerPrefab;

    private SC_MazePlayerSetting mMazeSettings;
    

    private void Update()
    {
        SC_PlayerMovement[] players = FindObjectsByType<SC_PlayerMovement>();

        if (players.Length < /*mMazeSettings.GetNumPlayers()*/ 1)
        {
            GameObject[] spawns = GameObject.FindGameObjectsWithTag("HuntedSpawn");
            int r = Mathf.FloorToInt(Random.Range(0, spawns.Length - 0.01f));

            GameObject temp = Instantiate(mPlayerPrefab, spawns[r].transform);
        }
    }
}
