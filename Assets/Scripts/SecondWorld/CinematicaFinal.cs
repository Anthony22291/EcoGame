using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CinematicaFinal : MonoBehaviour
{
    public VideoPlayer player;
    public string menuFinal = "MenuPrincipal";

    void Start()
    {
        player.loopPointReached += EndReached;
    }

    void EndReached(VideoPlayer vp)
    {
        SceneManager.LoadScene(menuFinal);
    }
}
