using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using Unity.XR.CoreUtils;

public class PlatformBootstrap : MonoBehaviour
{
    //used to check what platform we play with (headset = true, desktop = false)
    private bool isVR = false;

    //names of the children of the XR Origin that only work with a headset
    private string[] vrOnlyObjects = { "Left Controller", "Right Controller", "Left Hand", "Right Hand", "Locomotion" };

    //names of the components on the XR Origin and its camera that only work with a headset
    private string[] vrOnlyComponents = { "TrackedPoseDriver", "XRInputModalityManager", "CharacterControllerDriver" };

    //runs once, before the first scene is loaded, so the XR components find the headset when they start
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void StartXRBeforeScene()
    {
        XRManagerSettings manager = XRGeneralSettings.Instance.Manager;

        //if vr is not started yet (PC), we try to start it now
        if (manager.activeLoader == null)
        {
            manager.InitializeLoaderSync();
        }

        //if a loader started, we start the headset systems
        if (manager.activeLoader != null)
        {
            manager.StartSubsystems();
        }
    }

    private void Start()
    {
        //get the XR manager of unity
        XRManagerSettings manager = XRGeneralSettings.Instance.Manager;

        //if a loader is running, we check that the headset really displays
        if (manager.activeLoader != null)
        {
            XRDisplaySubsystem display = manager.activeLoader.GetLoadedSubsystem<XRDisplaySubsystem>();
            if (display != null)
            {
                isVR = display.running;
            }
        }

        //paste the result in console
        if (isVR)
        {
            Debug.Log("[PlatformBootstrap] Mode VR");
        }
        else
        {
            Debug.Log("[PlatformBootstrap] Mode bureau");
            SetupDesktop();
        }
    }

    //turns off everything that needs a headset, then turns on the keyboard and mouse controls
    private void SetupDesktop()
    {
        //1. turn off the VR-only children (controllers, hands, VR movement)
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            foreach (string objectName in vrOnlyObjects)
            {
                if (child.name == objectName)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        //2. turn off the VR-only components on the XR Origin and on the camera
        Camera cam = GetComponent<XROrigin>().Camera;
        DisableComponents(gameObject);
        DisableComponents(cam.gameObject);

        //3. turn on the desktop controls (added if they are not already on the object)
        DesktopPlayerController desktop = GetComponent<DesktopPlayerController>();
        if (desktop == null)
        {
            desktop = gameObject.AddComponent<DesktopPlayerController>();
        }
        desktop.enabled = true;
    }

    //disables every component of this object whose type name is in vrOnlyComponents
    private void DisableComponents(GameObject target)
    {
        foreach (Behaviour component in target.GetComponents<Behaviour>())
        {
            foreach (string typeName in vrOnlyComponents)
            {
                if (component.GetType().Name == typeName)
                {
                    component.enabled = false;
                }
            }
        }
    }
}