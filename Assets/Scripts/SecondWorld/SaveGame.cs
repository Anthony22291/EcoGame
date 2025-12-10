using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveGame : MonoBehaviour
{
    public void Guardar()
    {
        string escenaActual = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("SavedScene", escenaActual);
        PlayerPrefs.Save();
        Debug.Log("Juego guardado en la escena: " + escenaActual);
    }
}
