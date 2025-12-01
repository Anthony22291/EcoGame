using UnityEngine;

public class KeyItem : MonoBehaviour
{
    public int keyId;   // 1,2,3,4

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerKeys pk = other.GetComponent<PlayerKeys>();
        if (pk == null) return;

        pk.AddKey(keyId);   // se marca como obtenida
        Destroy(gameObject);
    }
}
