using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;

namespace UserStudy
{
    public class SHSHandleInput : MonoBehaviour
    {
        public SteamVR_Action_Boolean startHandleSample;

        [HideInInspector] public bool isHandleSampleActive;

        // Start is called before the first frame update
        void Start()
        {
            isHandleSampleActive = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (startHandleSample.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                isHandleSampleActive = !isHandleSampleActive;
                if (isHandleSampleActive) { Debug.Log("进入手柄采样模式..."); }
                else { Debug.Log("手柄采样模式关闭..."); }
            }
        }
    }
}

