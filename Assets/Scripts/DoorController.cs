using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Transform leftDoor;
    public Transform rightDoor;
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public Animator animatorRight;
    public Animator animatorLeft;
    private bool isOpen = false;
    private Quaternion leftDoorClosedRotation;
    private Quaternion rightDoorClosedRotation;
    private Quaternion leftDoorOpenRotation;
    private Quaternion rightDoorOpenRotation;

    private void Start()
    {
        leftDoorClosedRotation = leftDoor.localRotation;
        rightDoorClosedRotation = rightDoor.localRotation;

        leftDoorOpenRotation = leftDoorClosedRotation * Quaternion.Euler(0f, openAngle, 0f);
        rightDoorOpenRotation = rightDoorClosedRotation * Quaternion.Euler(0f, -openAngle, 0f);
    }

    private void Update()
    {
        leftDoor.localRotation = Quaternion.Slerp(leftDoor.localRotation, isOpen ? leftDoorOpenRotation : leftDoorClosedRotation, Time.deltaTime * openSpeed);
        rightDoor.localRotation = Quaternion.Slerp(rightDoor.localRotation, isOpen ? rightDoorOpenRotation : rightDoorClosedRotation, Time.deltaTime * openSpeed);
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        animatorLeft.SetTrigger("Open");
        animatorRight.SetTrigger("Open");
    }
}