using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal2 : MonoBehaviour
{
    public string fragmentID = "FragmentoMundo";
    public string cinematicScene = "CinematicaFinal";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (PlayerPrefs.GetInt(fragmentID, 0) == 1)
            {
                SceneManager.LoadScene(cinematicScene);
            }
            else
            {
                Debug.Log("No tienes el fragmento para activar el portal.");
            }
        }
    }
}
