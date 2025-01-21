using UnityEngine;

public struct MovementInputData
{
    public Vector3 inputDirection;
    public bool isJumping;
    public bool isCrouching;
    public bool isSprinting;
    public bool didSwitchCamera;
}

public class MovementInput : MonoBehaviour
{
    public MovementInputData PollInput()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");
        float moveVertical = Input.GetAxisRaw("Vertical");

        bool jumping = Input.GetKey(KeyCode.Space);
        bool crouching = Input.GetKey(KeyCode.LeftControl);
        bool sprinting = Input.GetKey(KeyCode.LeftShift);
        bool switchedCamera = Input.GetKeyDown(KeyCode.Tab);

        return new MovementInputData
        {
            inputDirection = new Vector3(moveHorizontal, 0, moveVertical),
            isJumping = jumping,
            isCrouching = crouching,
            isSprinting = sprinting,
            didSwitchCamera = switchedCamera
        };

    }
}
