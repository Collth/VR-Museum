using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class DesktopPlayerController : MonoBehaviour
{
    //settings you can change in the Inspector
    [SerializeField] private float eyeHeight = 1.65f;
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 4.5f;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float interactDistance = 6f;

    //things we find once at the start
    private CharacterController body;
    private Transform cam;
    private Transform leftHand;
    private Transform rightHand;
    private XRRayInteractor ray;

    //current state
    private float pitch = 0f;
    private float verticalSpeed = 0f;

    private void OnEnable()
    {
        //the XR Origin already has a CharacterController, it stops the player at walls
        body = GetComponent<CharacterController>();
        body.height = 1.8f;
        body.center = new Vector3(0f, 0.9f, 0f);

        //the camera of the XR Origin (the template's avatar follows it)
        cam = GetComponent<XROrigin>().Camera.transform;

        //the controllers are off, but their positions still move the avatar's hands
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "Left Controller")
            {
                leftHand = child;
            }
            if (child.name == "Right Controller")
            {
                rightHand = child;
            }
        }

        //a VR ray in the middle of the screen, so we can grab objects and click menus
        if (ray == null)
        {
            GameObject rayObject = new GameObject("Desktop Ray Interactor");
            rayObject.transform.SetParent(cam, false);
            ray = rayObject.AddComponent<XRRayInteractor>();
            ray.maxRaycastDistance = interactDistance;
            ray.enableUIInteraction = true;
            ray.useForceGrab = false;

            //the ray's buttons are pressed by this script, not by a VR controller
            ray.selectInput.inputSourceMode = XRInputButtonReader.InputSourceMode.ManualValue;
            ray.activateInput.inputSourceMode = XRInputButtonReader.InputSourceMode.ManualValue;
            ray.uiPressInput.inputSourceMode = XRInputButtonReader.InputSourceMode.ManualValue;
        }

        LockCursor(true);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (keyboard == null)
        {
            return;
        }
        if (mouse == null)
        {
            return;
        }

        //Escape frees the cursor, a click captures it again
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            LockCursor(false);
        }
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (mouse.leftButton.wasPressedThisFrame)
            {
                LockCursor(true);
            }
            PressRay(false, false);
            PlaceCameraAndHands();
            return;
        }

        //look: left/right turns the whole body, up/down only tilts the camera
        Vector2 look = mouse.delta.ReadValue() * mouseSensitivity;
        transform.Rotate(0f, look.x, 0f);
        pitch = Mathf.Clamp(pitch - look.y, -80f, 80f);

        //move: direction from the keys, relative to where the player looks
        Vector3 direction = Vector3.zero;
        if (keyboard.wKey.isPressed)
        {
            direction += transform.forward;
        }
        if (keyboard.sKey.isPressed)
        {
            direction -= transform.forward;
        }
        if (keyboard.dKey.isPressed)
        {
            direction += transform.right;
        }
        if (keyboard.aKey.isPressed)
        {
            direction -= transform.right;
        }
        direction = direction.normalized;

        //Shift to run
        float speed = walkSpeed;
        if (keyboard.leftShiftKey.isPressed)
        {
            speed = runSpeed;
        }

        //gravity, so the player stays on the floor
        if (body.isGrounded)
        {
            verticalSpeed = -1f;
        }
        else
        {
            verticalSpeed += Physics.gravity.y * Time.deltaTime;
        }

        Vector3 move = direction * speed;
        move.y = verticalSpeed;
        body.Move(move * Time.deltaTime);

        //left click grabs and presses, right click uses the held object
        PressRay(mouse.leftButton.isPressed, mouse.rightButton.isPressed);

        //done in Update so the avatar sends the new head position this frame
        PlaceCameraAndHands();
    }

    //tells the ray which buttons are pressed
    private void PressRay(bool select, bool activate)
    {
        float selectValue = 0f;
        if (select)
        {
            selectValue = 1f;
        }
        float activateValue = 0f;
        if (activate)
        {
            activateValue = 1f;
        }
        ray.selectInput.QueueManualState(select, selectValue);
        ray.uiPressInput.QueueManualState(select, selectValue);
        ray.activateInput.QueueManualState(activate, activateValue);
    }

    //puts the camera at eye height and the hands by the player's sides
    private void PlaceCameraAndHands()
    {
        cam.position = transform.position + Vector3.up * eyeHeight;
        cam.rotation = Quaternion.Euler(pitch, transform.eulerAngles.y, 0f);

        Quaternion bodyRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        if (leftHand != null)
        {
            leftHand.position = cam.position + bodyRotation * new Vector3(-0.25f, -0.6f, 0.15f);
            leftHand.rotation = bodyRotation;
        }
        if (rightHand != null)
        {
            rightHand.position = cam.position + bodyRotation * new Vector3(0.25f, -0.6f, 0.15f);
            rightHand.rotation = bodyRotation;
        }
    }

    //small square in the middle of the screen to aim
    private void OnGUI()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            GUI.Box(new Rect(Screen.width / 2f - 3f, Screen.height / 2f - 3f, 6f, 6f), GUIContent.none);
        }
    }

    //locked = cursor hidden in the middle of the screen
    private void LockCursor(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}