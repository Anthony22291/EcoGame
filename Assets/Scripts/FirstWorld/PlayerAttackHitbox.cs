using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    public int damage = 2;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        // Intentar hacer daño al enemigo de plataforma
        EnemyAI_Plataforma enemyPlataforma = other.GetComponent<EnemyAI_Plataforma>();
        if (enemyPlataforma != null)
        {
            enemyPlataforma.TakeDamage(damage);
            Debug.Log("Golpe al enemigo de plataforma");
        }

        // Si tienes otros tipos de enemigos, puedes dejar sus llamadas, por ejemplo:
        // SlimeHealth slime = other.GetComponent<SlimeHealth>();
        // if (slime != null)
        // {
        //     slime.TakeDamage(damage);
        //     Debug.Log("Golpe al slime");
        // }
    }
}