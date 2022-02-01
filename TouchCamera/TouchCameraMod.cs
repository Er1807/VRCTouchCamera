using TouchCamera;
using MelonLoader;
using UnityEngine;
using VRC.UserCamera;
using System.Collections;
using UnityEngine.UI;
using UnhollowerBaseLib;
using System.IO;

[assembly: MelonInfo(typeof(TouchCameraMod), "TouchCamera", "1.0.1", "Eric van Fandenfart")]
[assembly: MelonGame]

namespace TouchCamera
{

    public class TouchCameraMod : MelonMod
    {
        

        public override void OnApplicationStart()
        {
            MelonCoroutines.Start(WaitForCamera());
        }

        private IEnumerator WaitForCamera()
        {
            while (UserCameraController.field_Internal_Static_UserCameraController_0 == null)
                yield return null;
            var cameraobj = UserCameraController.field_Internal_Static_UserCameraController_0.transform;

            while (cameraobj.Find("ViewFinder/PhotoControls/Primary /ControlGroup_Main/ControlGroup_Space/Scroll View/Viewport/Content/Attached/Icon")?.GetComponent<CanvasRenderer>()?.GetMaterial()?.shader == null)
                yield return null;

            var buttonParent = cameraobj.Find("ViewFinder/PhotoControls/Primary /ControlGroup_Main").gameObject;
            LoggerInstance.Msg("Registering TouchButton");
            foreach (var item in buttonParent.GetComponentsInChildren<Button>(true))
            {
                item.gameObject.AddComponent<TouchButton>();
            }
            foreach (var item in buttonParent.GetComponentsInChildren<Toggle>(true))
            {
                item.gameObject.AddComponent<TouchButton>();
            }
            LoggerInstance.Msg("Registered TouchButton");

            LoggerInstance.Msg("Disabling Overrender");

            SetLayerRecursively(cameraobj.Find("ViewFinder/PhotoControls").gameObject, 3);
            SetLayerRecursively(cameraobj.Find("ViewFinder/UserCamera_New").gameObject, 3);
            AssetBundle bundle;

            LoggerInstance.Msg("Loading replacment shaders from assetbundle");
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("TouchCamera.shaderreplacment"))
            using (var tempStream = new MemoryStream((int)stream.Length))
            {
                stream.CopyTo(tempStream);
                bundle = AssetBundle.LoadFromMemory(tempStream.ToArray(), 0);
            }

            var uishader = bundle.LoadAsset<Shader>("Assets/UIReplacement.shader");
            var uishaderTMPRO = bundle.LoadAsset<Shader>("Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile-Replacment.shader");

            LoggerInstance.Msg("Loading shaders");
            LoggerInstance.Msg("Applying shaders");

            ReplaceShader(cameraobj, "ViewFinder/PhotoControls/Primary /ControlGroup_Main/Scroll View/Viewport/Content/Space/Icon", uishader);
            ReplaceShader(cameraobj, "ViewFinder/PhotoControls/Primary /ControlGroup_Main/ControlGroup_Space/Scroll View/Viewport/Content/Attached/Icon", uishader);
            ReplaceShader(cameraobj, "ViewFinder/PhotoControls/Controls _ OnScreen/FrontCenter/Zoom _ Slider/Slider/Background/Fill Area", uishader);
            ReplaceShader(cameraobj, "ViewFinder/PhotoControls/Controls _ OnScreen/FrontCenter/Zoom _ Slider/Slider/Background/Fill Area/Fill", uishader);
            ReplaceShader(cameraobj, "ViewFinder/PhotoControls/Controls _ OnScreen/FrontCenter/Zoom _ Slider/Slider/Background/Fill Area", uishader);
            ReplaceShader(cameraobj, "ViewFinder/PhotoControls/Primary /ControlGroup_Main/Scroll View/Scrollbar Horizontal/Sliding Area/Handle", uishader);
            ReplaceShader(cameraobj, "ViewFinder/PhotoControls/Primary /ControlGroup_Main/RightArrow/Image", uishader);

            ReplaceShader(cameraobj, "ViewFinder/PhotoControls/Primary /ControlGroup_Main/Scroll View/Viewport/Content/CameraAimMode/Text (TMP)", uishaderTMPRO);



            while (cameraobj.Find("ViewFinder/PhotoControls/Primary /ControlGroup_Main/SelectedGroupHighlightArrow")?.GetComponent<CanvasRenderer>()?.GetMaterial()?.shader == null)
                yield return null;

            ReplaceShader(cameraobj, "ViewFinder/PhotoControls/Primary /ControlGroup_Main/SelectedGroupHighlightArrow", uishader);

            LoggerInstance.Msg("Disabled Overrender");
        }

        private void ReplaceShader(Transform transform,string path, Shader shader)
        {
            transform.Find(path).GetComponent<CanvasRenderer>().GetMaterial().shader = shader;
        }

        private void SetLayerRecursively(GameObject obj,int newLayer)
        {
            obj.layer = newLayer;
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                SetLayerRecursively(obj.transform.GetChild(i).gameObject, newLayer);
            }
        }

    }
}
