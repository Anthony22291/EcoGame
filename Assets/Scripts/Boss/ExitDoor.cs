using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [SerializeField] private string sceneName; // Nombre de la escena a cargar

    [Header("Estado de la Puerta")]
    [SerializeField] private bool isActive = false;

    [Header("Sprites")]
    [SerializeField] private SpriteRenderer doorSprite;
    [SerializeField] private Sprite closedSprite; // Sprite de puerta cerrada
    [SerializeField] private Sprite openSprite;   // Sprite de puerta abierta

    [Header("UI")]
    [SerializeField] private GameObject interactPrompt; // UI "Presiona E"

    [Header("Efectos Opcionales")]
    [SerializeField] private AudioClip doorOpenSound; // Sonido al abrir (opcional)
    private AudioSource audioSource;

    private bool playerNearby = false;

    void Start()
    {
        // Inicialmente la puerta está cerrada
        if (doorSprite != null && closedSprite != null)
        {
            doorSprite.sprite = closedSprite;
        }

        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        // Obtener AudioSource si existe
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Solo permitir interacción si la puerta está activa y el jugador está cerca
        if (isActive && playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            LoadNextScene();
        }
    }

    // Método público para activar la puerta cuando el boss muera
    public void ActivateDoor()
    {
        isActive = true;
        OpenDoor();
        Debug.Log("¡Puerta activada y abierta!");
    }

    void OpenDoor()
    {
        // Cambiar al sprite de puerta abierta
        if (doorSprite != null && openSprite != null)
        {
            doorSprite.sprite = openSprite;
        }

        // Reproducir sonido de apertura (opcional)
        if (audioSource != null && doorOpenSound != null)
        {
            audioSource.PlayOneShot(doorOpenSound);
        }
    }

    void LoadNextScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("¡No hay nombre de escena asignado!");
            return;
        }

        Debug.Log("Cargando escena: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;

            // Mostrar prompt solo si la puerta está activa
            if (isActive && interactPrompt != null)
                interactPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (interactPrompt != null)
                interactPrompt.SetActive(false);
        }
    }
}
