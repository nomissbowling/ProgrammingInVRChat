#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;

namespace Iwsd
{

    public class InUnityDebug
    {

        static string playerPrefabPath = "iwsd_vrc/Tools/OnEditorEmu/Prefabs/Emu_Player";
        
        // This is entry point of this emulator.
        [PostProcessScene]
        static void OnPostProcessScene()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode || !Application.isEditor) {
                // CHECK Unity spec. Is this condition suitable?
                return;
            }
            
            // NOTE https://anchan828.github.io/editor-manual/web/callbacks.html
            // EditorSceneManager.GetSceneManagerSetup()
        
            Setup_SceneDescriptor();
            Setup_TriggersComponents();

            SpawnPlayerObject();
        }

        static private void Setup_SceneDescriptor()
        {
            var comps = Object.FindObjectsOfType(typeof(VRCSDK2.VRC_SceneDescriptor));

            VRCSDK2.VRC_SceneDescriptor descriptor;
            switch (comps.Length)
            {
                case 0:
                    Iwlog.Warn("VRC_SceneDescriptor not found. Create temporary");
                    var go = new GameObject("VRC_SceneDescriptor holder");
                    descriptor = go.AddComponent<VRCSDK2.VRC_SceneDescriptor>();
                    var scene = EditorSceneManager.GetActiveScene();
                    EditorSceneManager.MoveGameObjectToScene(go, scene);
                    break;
                case 1:
                    descriptor = (VRCSDK2.VRC_SceneDescriptor)comps[0];
                    break;
                default:
                    Iwlog.Warn("Too many VRC_SceneDescriptor found.");
                    descriptor = (VRCSDK2.VRC_SceneDescriptor)comps[0];
                    break;
            }

            LocalPlayerContext.SceneDescriptor = descriptor;
        }


        // (see also. Execute_SpawnObject)
        static private void Setup_TriggersComponents()
        {
            foreach (VRCSDK2.VRC_Trigger triggerComp in UnityEngine.Resources.FindObjectsOfTypeAll(typeof(VRCSDK2.VRC_Trigger)))
            {
                // https://answers.unity.com/questions/218429/how-to-know-if-a-gameobject-is-a-prefab.html
                // (In latest Unity GetPrefabParent is obsolete.)
                bool isPrefabOriginal = (PrefabUtility.GetPrefabParent(triggerComp.gameObject) == null)
                    && (PrefabUtility.GetPrefabObject(triggerComp.gameObject.transform) != null);
 
                if (isPrefabOriginal) {
                    continue;
                }
                
                var emu_trigger = triggerComp.gameObject.AddComponent<Emu_Trigger>();
                emu_trigger.SetupFrom(triggerComp);
 
                emu_trigger.debugString = triggerComp.gameObject.name;
            }
        }

        static private void SpawnPlayerObject()
        {
            // Put player prefab
            var playerPrefab = Resources.Load<GameObject>(playerPrefabPath);
            if (playerPrefab == null) {
                Iwlog.Error("PlayerPrefab not found. path='" + playerPrefabPath + "'");
            } else {
                var playerInstance = Object.Instantiate(playerPrefab);
                var scene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MoveGameObjectToScene(playerInstance, scene);
                // TODO move to spawn point

                LocalPlayerContext.PlayerGameObject = playerInstance;
            }
        }

    }
}

#endif // if UNITY_EDITOR
