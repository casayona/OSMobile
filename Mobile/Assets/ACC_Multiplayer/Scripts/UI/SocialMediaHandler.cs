using UnityEngine;

public class SocialMediaHandler : MonoBehaviour
{
    public void OpenInstagram()
    {
        Application.OpenURL("https://www.instagram.com/casayonastudio//");
    }

    public void OpenYoutube()
    {
        Application.OpenURL("https://www.youtube.com/@casayonastudio");
    }

    public void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/7zBP88jqHk");
    }
}
