using UnityEngine;

public class Fragmento : MonoBehaviour
{
    public string fragmentID = "FragmentoMundo";

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Reproducir sonido de recogida
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            PlayerPrefs.SetInt(fragmentID, 1);
            PlayerPrefs.Save();
            Debug.Log("Fragmento recogido!");

            Destroy(gameObject);
        }
    }
}
