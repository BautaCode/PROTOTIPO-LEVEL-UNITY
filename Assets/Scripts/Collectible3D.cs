using UnityEngine;

public class Collectible3D : MonoBehaviour
{
    public int value = 1;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager3D.I.AddPoint(value);
            Destroy(gameObject);
        }
    }
}


