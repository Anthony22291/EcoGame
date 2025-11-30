using UnityEngine;

public class KeyItem : MonoBehaviour
{
    public string keyId; // ej: "roja", "azul"

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerKeys pk = other.GetComponent<PlayerKeys>();
        if (pk == null) return;

        pk.currentKeyId = keyId;
        Destroy(gameObject);
    }
}
