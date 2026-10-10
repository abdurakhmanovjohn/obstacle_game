using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] float moveSpeed = 10f;

    void Start()
    {
        PrintInstruction();
    }

    void Update()
    {
        MovePlayer();
    }

    void PrintInstruction()
    {
        Debug.Log("Welcome to the game!");
        Debug.Log("Move using arrow keys or wasd");
        Debug.Log("Don't bump into objects!");
    }

    void MovePlayer()
    {
        // Keyboard + screen buttons (TouchControls). Clamp keeps the sum between -1 and 1.
        float horizontal = Mathf.Clamp(Input.GetAxis("Horizontal") + TouchControls.Horizontal, -1f, 1f);
        float vertical = Mathf.Clamp(Input.GetAxis("Vertical") + TouchControls.Vertical, -1f, 1f);

        float xValue = horizontal * Time.deltaTime * moveSpeed;
        float yValue = 0f;
        float zValue = vertical * Time.deltaTime * moveSpeed;
        transform.Translate(xValue, yValue, zValue);
    }
}
