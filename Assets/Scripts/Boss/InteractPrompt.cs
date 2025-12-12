using UnityEngine;
using UnityEngine.UI;

public class InteractPrompt : MonoBehaviour
{
    [SerializeField] private Text promptText;
    [SerializeField] private string message = "Presiona 'E' para continuar";

    void Start()
    {
        if (promptText != null)
            promptText.text = message;
    }
}
