using MelonLoader;
using System;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace TouchCamera
{

    [RegisterTypeInIl2Cpp]
    public class TouchButton : MonoBehaviour
    {

        //[method: HideFromIl2Cpp]
        //public event Action? OnEnabled;

        //[method: HideFromIl2Cpp]
        //public event Action? OnDisabled;
        private RectTransform rectTransform;
        private Button button;
        private Toggle toggle;
        private ScrollRect scrollrect;
        private Il2CppStructArray<Vector3> worldPosition;
        static float lastInteraction = 0;

        public int totalCountInParent;
        public int buttonPositionInParent;


        static bool lastPressed = false;
        bool lastPressedLocal = false;
        static TouchButton lastTouchButton = null;

        public TouchButton(IntPtr obj0) : base(obj0)
        {
            rectTransform = gameObject.GetComponent<RectTransform>();
            button = gameObject.GetComponent<Button>();
            toggle = gameObject.GetComponent<Toggle>();
            worldPosition = new Il2CppStructArray<Vector3>(4);
            if(rectTransform.parent.name == "Content")
            {
                scrollrect = rectTransform.parent.parent.parent.GetComponent<ScrollRect>();
                totalCountInParent = rectTransform.parent.childCount;
                for (int i = 0; i < totalCountInParent; i++)
                {
                    if (rectTransform.parent.GetChild(i) == rectTransform)
                    {
                        buttonPositionInParent = i;
                        break;
                    }
                }
            }

        }

        public bool IsVisible()
        {
            if (scrollrect == null)
                return true;

            int startpoint = (int) Math.Round((totalCountInParent - 6) * scrollrect.horizontalNormalizedPosition);
            int endpoint = startpoint + 5;

            return buttonPositionInParent >= startpoint && buttonPositionInParent <= endpoint;
        }

        void Update()
        {
            if (lastInteraction + 1.5f > Time.time)
                return;
            Vector3? fingerPos = Networking.LocalPlayer?.GetBonePosition(HumanBodyBones.RightIndexDistal);
            if (!fingerPos.HasValue)
                return;

            //Each corner provides its world space value. The returned array of 4 vertices is clockwise.
            //It starts bottom left and rotates to top left, then top right, and finally bottom right.
            //Note that bottom left, for example, is an (x, y, z) vector with x being left and y being bottom.
            rectTransform.GetWorldCorners(worldPosition);

            Vector3 pos1 = worldPosition[0] + (Vector3.Scale(rectTransform.forward, new Vector3(0.05f, 0.05f, 0.05f)));
            Vector3 pos2 = worldPosition[2] - (Vector3.Scale(rectTransform.forward, new Vector3(0.05f, 0.05f, 0.05f)));

            Vector3 min = Vector3.Min(pos1, pos2);
            Vector3 max = Vector3.Max(pos1, pos2);

            Vector3 fingerPosValue = fingerPos.Value;

            if (!IsVisible())
                return;

            if (fingerPosValue.x < max.x &&
                fingerPosValue.y < max.y &&
                fingerPosValue.z < max.z &&
                fingerPosValue.x > min.x &&
                fingerPosValue.y > min.y &&
                fingerPosValue.z > min.z)
            {
                if (!lastPressed)
                {
                    lastPressed = true; 
                    lastPressedLocal = true;
                    lastTouchButton = this;
                    button?.Press();
                    toggle?.InternalToggle();
                }
            }
            else if(lastPressedLocal || !lastTouchButton.isActiveAndEnabled)
            {
                lastPressedLocal = false;
                lastTouchButton = null;
                lastPressed = false;
                lastInteraction = Time.time;
            }
        }
    }
}
