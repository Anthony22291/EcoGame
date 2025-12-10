using UnityEngine;

public class Fragmento : MonoBehaviour
{
    public string fragmentID = "FragmentoMundo";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerPrefs.SetInt(fragmentID, 1);
            PlayerPrefs.Save();
            Debug.Log("Fragmento recogido!");

            Destroy(gameObject);
        }
    }
}
