﻿using TouchCamera;
using MelonLoader;
using UnityEngine;
using VRC.UserCamera;
using System.Collections;
using UnityEngine.UI;
using UnhollowerBaseLib;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using CameraUiAnimator = MonoBehaviourPublicReObSiObInSiBuReVeUnique;

[assembly: MelonInfo(typeof(TouchCameraMod), "TouchCamera", "1.0.9", "Eric van Fandenfart")]
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

            uishader = bundle.LoadAsset<Shader>("Assets/UIReplacement.shader");
            uishaderTMPRO = bundle.LoadAsset<Shader>("Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile-Replacment.shader");

            LoggerInstance.Msg("Loading shaders");
            LoggerInstance.Msg("Applying shaders");



            var rightArrow = cameraobj.Find("ViewFinder/PhotoControls/Primary /ControlGroup_Main/RightArrow");
            var leftArrow = cameraobj.Find("ViewFinder/PhotoControls/Primary /ControlGroup_Main/LeftArrow");
            //rightArrow.GetComponent<CameraUiAnimator>().
            rightArrow.GetComponent<CameraUiAnimator>().enabled = false;
            leftArrow.GetComponent<CameraUiAnimator>().enabled = false;

            rightArrow.localScale = new Vector3(0.5f, 1, 1);
            leftArrow.localScale = new Vector3(0.5f, 1, 1);

            MelonCoroutines.Start(ApplyArrowTransform(leftArrow, rightArrow));



            foreach (var item in cameraobj.Find("ViewFinder/PhotoControls").GetComponentsInChildren<CanvasRenderer>(true))
            {
                ReplaceShader(item);
                item.gameObject.AddComponent<EnableDisableListener>().OnEnableEvent += obj => MelonCoroutines.Start(UpdateShader(obj));
            }

            while (cameraobj.Find("ViewFinder/PhotoControls/Primary /ControlGroup_Main/SelectedGroupHighlightArrow")?.GetComponent<CanvasRenderer>()?.GetMaterial()?.shader == null)
                yield return null;

            //do it a second time to make sure all sub components also got it
            foreach (var item in cameraobj.Find("ViewFinder/PhotoControls").GetComponentsInChildren<CanvasRenderer>(true).Where(x => x.GetComponent<EnableDisableListener>() == null))
            {
                ReplaceShader(item);
                item.gameObject.AddComponent<EnableDisableListener>().OnEnableEvent += obj => MelonCoroutines.Start(UpdateShader(obj));
            }



            LoggerInstance.Msg("Disabled Overrender");
        }

        private IEnumerator ApplyArrowTransform(Transform leftArrow, Transform rightArrow)
        {
            yield return new WaitForSeconds(1.5f);//make sure animation is done playing

            var posRight = rightArrow.localPosition;
            posRight.x = 530;
            rightArrow.transform.localPosition = posRight;


            var posLeft = leftArrow.localPosition;
            posLeft.x = -530;
            leftArrow.localPosition = posLeft;

        }

        private IEnumerator UpdateShader(GameObject obj)
        {
            yield return new WaitForSeconds(0.1f);
            ReplaceShader(obj.GetComponent<CanvasRenderer>());
        }

        private Dictionary<string, Material> replacmentMaterials = new Dictionary<string, Material>();
        private Shader uishader;
        private Shader uishaderTMPRO;

        private void ReplaceShader(CanvasRenderer renderer)
        {
            if (renderer.GetMaterial() == null || replacmentMaterials.ContainsValue(renderer.GetMaterial()) || (!renderer.GetMaterial().name.Contains("NotoSans-Regular") && !renderer.GetMaterial().name.Contains("VRChat/UI/Default")))
                return;

            var name = renderer.GetMaterial().name.Replace("(Clone)", "");
            if (!replacmentMaterials.ContainsKey(name))
            {
                LoggerInstance.Msg($"Creating a new material for {name}");
                replacmentMaterials[name] = Object.Instantiate(renderer.GetMaterial());
                replacmentMaterials[name].shader = renderer.GetMaterial().name.Contains("NotoSans-Regular") ? uishaderTMPRO : uishader;
                replacmentMaterials[name].hideFlags = HideFlags.HideAndDontSave;
            }

            renderer.SetMaterial(replacmentMaterials[name], 0);
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                SetLayerRecursively(obj.transform.GetChild(i).gameObject, newLayer);
            }
        }

    }
}
