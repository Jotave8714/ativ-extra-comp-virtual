using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(PlayerMotor))]
public class PlayerPlatformTracker : MonoBehaviour
{
    private CharacterController controller;
    private PlayerMotor motor;

    private Transform activePlatform;
    private Vector3 lastPlatformPosition;
    private Quaternion lastPlatformRotation;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        motor = GetComponent<PlayerMotor>();
    }

    void LateUpdate()
    {
        Transform currentGround = motor.CurrentGroundTransform;

        if (currentGround != null)
        {
            // Se continuamos na mesma plataforma do frame anterior, aplicamos a diferença de movimento
            if (currentGround == activePlatform)
            {
                Vector3 deltaPosition = activePlatform.position - lastPlatformPosition;
                if (deltaPosition != Vector3.zero)
                {
                    controller.Move(deltaPosition);
                }

                // Se a plataforma também girar, gira o player junto
                Quaternion deltaRotation = activePlatform.rotation * Quaternion.Inverse(lastPlatformRotation);
                deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 0.01f)
                {
                    transform.RotateAround(activePlatform.position, axis, angle);
                }
            }

            activePlatform = currentGround;
            lastPlatformPosition = activePlatform.position;
            lastPlatformRotation = activePlatform.rotation;
        }
        else
        {
            activePlatform = null;
        }
    }
}