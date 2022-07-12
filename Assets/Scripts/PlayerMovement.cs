using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{

    PlayerInput playerInput;
    InputAction move;
    [SerializeField]private Rigidbody rb;
    [SerializeField]private float moveSpeed;
    private void Awake()
    {
        playerInput = new PlayerInput();
        move = playerInput.movement.KB;
    }
    private void OnEnable()
    {
        move.Enable();
    }
    private void OnDisable()
    {
        move.Disable();
    }
    private void Update()
    {
        Vector2 direction = move.ReadValue<Vector2>();
        if(direction!= Vector2.zero)
        {
            rb.AddForce(direction.x * moveSpeed, 0f, direction.y * moveSpeed);
            Debug.Log("Player input: " + direction);
        }
    }
}
