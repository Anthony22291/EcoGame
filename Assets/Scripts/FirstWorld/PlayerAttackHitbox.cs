using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    public int damage = 2;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        SlimeHealth slime = other.GetComponent<SlimeHealth>();
        if (slime != null)
        {
            slime.TakeDamage(damage);
            Debug.Log("Golpe al slime");
        }
    }
}
