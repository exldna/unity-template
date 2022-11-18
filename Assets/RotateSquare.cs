using UnityEngine;

public class RotateSquare : MonoBehaviour {
    void Update() {
        transform.Rotate(0, 0, 50 * Time.deltaTime);
    }
}