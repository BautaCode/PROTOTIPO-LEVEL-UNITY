using UnityEngine;

public class UITestHotkeys : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) GameManager3D.I.AddPoint(1); // suma punto
        if (Input.GetKeyDown(KeyCode.W)) GameManager3D.I.AddPoint(999); // ganar
        if (Input.GetKeyDown(KeyCode.L)) GameManager3D.I.Lose(); // perder
    }
}

