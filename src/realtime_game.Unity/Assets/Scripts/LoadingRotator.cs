using UnityEngine;

public class LoadingRotator : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 180f; // “x/•b

    void Update()
    {
        transform.Rotate(0f, 0f, -rotateSpeed * Time.deltaTime);
    }
}
