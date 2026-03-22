using UnityEngine;

public class LeaderboardUIHandler : MonoBehaviour
{
    private void OnEnable()
    {
        if (PlayfabManager.instance != null)
        {
            Debug.Log("Sýralama paneli açýldý, veriler çekiliyor...");
            PlayfabManager.instance.GetLeaderboard();
        }
    }
}
