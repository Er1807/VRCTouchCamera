using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

namespace TouchCamera
{
    [RegisterTypeInIl2Cpp]
    class EnableDisableListener : MonoBehaviour
    {

        [method: HideFromIl2Cpp]
        public event Action<GameObject> OnEnableEvent;
        [method: HideFromIl2Cpp]
        public event Action<GameObject> OnDisableEvent;


        public EnableDisableListener(IntPtr obj0) : base(obj0)
        {
        }
        internal void OnEnable() => OnEnableEvent?.Invoke(gameObject);
        internal void OnDisable() => OnDisableEvent?.Invoke(gameObject);
    }
}
