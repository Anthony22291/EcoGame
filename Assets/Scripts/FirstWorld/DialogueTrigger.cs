using UnityEngine;
using TMPro;

public class DialogueTrigger : MonoBehaviour
{
    [Header("UI Referencias")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Diálogo")]
    [TextArea(3, 10)]
    public string[] dialogueLines;

    private int currentLine = 0;
    private bool playerInRange = false;
    private bool dialogueActive = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            CloseDialogue();
        }
    }

    void Update()
    {
      
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !dialogueActive)
        {
            StartDialogue();
        }

       
        if (dialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            NextLine();
        }
    }

    void StartDialogue()
    {
        dialogueActive = true;
        dialoguePanel.SetActive(true);
        currentLine = 0;
        dialogueText.text = dialogueLines[currentLine];
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLine];
        }
        else
        {
            CloseDialogue();
        }
    }

    void CloseDialogue()
    {
        dialogueActive = false;
        dialoguePanel.SetActive(false);
        currentLine = 0;
    }
}
