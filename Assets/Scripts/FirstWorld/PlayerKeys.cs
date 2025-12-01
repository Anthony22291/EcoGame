using UnityEngine;

public class PlayerKeys : MonoBehaviour
{
    // Índices 1..4 indican si tiene esa llave
    public bool[] hasKey = new bool[5]; // [0] no se usa

    public void AddKey(int id)
    {
        if (id >= 1 && id < hasKey.Length)
            hasKey[id] = true;
    }

    public bool HasKey(int id)
    {
        return (id >= 1 && id < hasKey.Length) && hasKey[id];
    }
}
