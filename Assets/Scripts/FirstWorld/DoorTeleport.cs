using UnityEngine;

public class DoorTeleport : MonoBehaviour
{
 
    public Transform destinationPoint;
    public bool requireInteraction = true;
    public KeyCode interactionKey = KeyCode.E;
    private bool playerInRange = false;
    private GameObject currentPlayer;

    void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void Update()
    {
       
        if (requireInteraction && playerInRange && currentPlayer != null)
        {
            if (Input.GetKeyDown(interactionKey))
            {
                TeleportPlayer(currentPlayer);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si es el jugador
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            currentPlayer = other.gameObject;

            if (!requireInteraction)
            {
                TeleportPlayer(other.gameObject);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            currentPlayer = null;
        }
    }

    void TeleportPlayer(GameObject player)
    {
        if (destinationPoint != null)
        {
  
            player.transform.position = destinationPoint.position;
        }
        else
        {
            Debug.LogWarning("¡No hay punto de destino asignado en la puerta!");
        }
    }


   
}