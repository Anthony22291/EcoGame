using UnityEngine;

public class Door : MonoBehaviour
{
    public string requiredKeyId;
    public GameObject mensajeLlaveIncorrecta;

    bool isOpen = false;

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isOpen) return;

        if (!Input.GetKeyDown(KeyCode.E)) return;

        PlayerKeys pk = other.GetComponent<PlayerKeys>();
        if (pk == null) return;

        if (pk.currentKeyId == requiredKeyId)
        {
            isOpen = true;
            gameObject.SetActive(false);
        }
        else
        {
            if (mensajeLlaveIncorrecta != null)
            {
                mensajeLlaveIncorrecta.SetActive(true);
            }
        }
    }
}
