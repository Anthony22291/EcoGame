using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private string firstScene = "Level1";

    void Start()
    {
        // Si no existe partida guardada, ocultamos Continuar
        if (!PlayerPrefs.HasKey("SavedScene"))
        {
            GameObject btn = GameObject.Find("Button_Continuar");
            if (btn != null)
                btn.SetActive(false);
        }
    }

    // BOTÓN JUGAR ? REINICIAR Y MANDAR SIEMPRE A LEVEL1
    public void Jugar()
    {
        // Borrar partida previa
        PlayerPrefs.DeleteKey("SavedScene");

        Debug.Log("Iniciando nuevo juego ? Level1");

        // Cargar Level1 siempre
        SceneManager.LoadScene(firstScene);
    }

    // BOTÓN CONTINUAR ? SOLO SI HAY PARTIDA GUARDADA
    public void Continuar()
    {
        if (!PlayerPrefs.HasKey("SavedScene"))
        {
            Debug.LogError("No hay escena guardada.");
            return;
        }

        string sceneToLoad = PlayerPrefs.GetString("SavedScene");
        Debug.Log("Continuando partida ? " + sceneToLoad);

        // Verificar que la escena exista
        if (SceneExists(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
        else
            Debug.LogError("La escena guardada NO existe en Build Settings.");
    }

    public void Salir()
    {
        Application.Quit();
    }

    // -------------------
    //   VERIFICAR ESCENA
    // -------------------
    bool SceneExists(string name)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);

            if (sceneName == name)
                return true;
        }
        return false;
    }
}
