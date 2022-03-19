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

        public MelonPreferences_Entry<Hands> selectedHand;

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
            if (lastInteraction + .5f > Time.time)
                return;

            if (Networking.LocalPlayer == null)
                return;

            if (!IsVisible())
                return;

            //Each corner provides its world space value. The returned array of 4 vertices is clockwise.
            //It starts bottom left and rotates to top left, then top right, and finally bottom right.
            //Note that bottom left, for example, is an (x, y, z) vector with x being left and y being bottom.
            rectTransform.GetWorldCorners(worldPosition);

            Plane plane = new Plane(worldPosition[0], worldPosition[1], worldPosition[2]);


            bool isTouching = false;


            Vector3 fingerPosRight = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.RightIndexDistal);
            Vector3 fingerPosLeft = Networking.LocalPlayer.GetBonePosition(HumanBodyBones.LeftIndexDistal);

            if (selectedHand.Value == Hands.LeftHand || selectedHand.Value == Hands.BothHands)
                isTouching |= CheckIfTouching(plane, fingerPosLeft);

            if (selectedHand.Value == Hands.RightHand || selectedHand.Value == Hands.BothHands)
                isTouching |= CheckIfTouching(plane, fingerPosRight);




            if (isTouching)
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
            else if (lastPressedLocal || !(lastTouchButton?.isActiveAndEnabled ?? true))
            {
                lastPressedLocal = false;
                lastTouchButton = null;
                lastPressed = false;
                lastInteraction = Time.time;
            }
        }

        private bool CheckIfTouching(Plane plane, Vector3 fingerPos)
        {
            Vector3 closestPoint = plane.ClosestPointOnPlane(fingerPos);

            float distance = Vector3.Distance(fingerPos, closestPoint);

            if (distance > 0.05f)
                return false;

            float d1 = Vector3.Distance(closestPoint, worldPosition[0]);
            float d2 = Vector3.Distance(closestPoint, worldPosition[1]);
            float d3 = Vector3.Distance(closestPoint, worldPosition[2]);
            float d4 = Vector3.Distance(closestPoint, worldPosition[3]);

            float d = Vector3.Distance(worldPosition[0], worldPosition[1]);

            return d1 < d && d2 < d && d3 < d && d4 < d;



        }
    }
}
