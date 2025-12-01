using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    public int requiredKeyId;
    public GameObject mensajeLlaveIncorrecta;
    bool isOpen = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || isOpen) return;

        PlayerKeys pk = collision.gameObject.GetComponent<PlayerKeys>();
        if (pk == null) return;

        if (pk.HasKey(requiredKeyId))
        {
            isOpen = true;
            gameObject.SetActive(false);
        }
        else
        {
            if (mensajeLlaveIncorrecta != null)
            {
                mensajeLlaveIncorrecta.SetActive(true);
                StartCoroutine(EsconderMensaje());
            }
        }
    }

    IEnumerator EsconderMensaje()
    {
        yield return new WaitForSeconds(2f);
        if (mensajeLlaveIncorrecta != null)
            mensajeLlaveIncorrecta.SetActive(false);
    }
}
