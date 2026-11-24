using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

public class VRMultiplayerSceneTests
{
    // Path to the template's main scene
    const string scenePath = "Assets/Scenes/SampleScene.unity";

    [SetUp]
    public void OpenScene()
    {
        // Runs before every test: open the sample scene
        EditorSceneManager.OpenScene(scenePath);
    }

    // Looks through every script in the scene for one with this class name
    bool SceneHasComponent(string className)
    {
        MonoBehaviour[] all = Object.FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (MonoBehaviour script in all)
        {
            if (script != null && script.GetType().Name == className)
                return true;
        }
        return false;
    }

    [Test]
    public void SanityCheck_TestFrameworkWorks()
    {
        Assert.AreEqual(4, 2 + 2);
    }

    [Test]
    public void Scene_HasNetworkManager()
    {
        Assert.IsTrue(SceneHasComponent("NetworkManagerVRMultiplayer"),
    "No NetworkManager found - multiplayer won't start.");
    }

    [Test]
    public void Scene_HasXRINetworkGameManager()
    {
        Assert.IsTrue(SceneHasComponent("XRINetworkGameManager"),
            "No XRINetworkGameManager found in the scene.");
    }

    [Test]
    public void Scene_HasXROrigin()
    {
        Assert.IsTrue(SceneHasComponent("XROrigin"),
            "No XR Origin (VR player rig) found in the scene.");
    }
}