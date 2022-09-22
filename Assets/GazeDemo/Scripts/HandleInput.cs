using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;

namespace GazeHeat
{
    public class HandleInput : MonoBehaviour
    {
        public SteamVR_Action_Boolean switchGazeMode;
        public bool isGazeVoxelization;


        // Start is called before the first frame update
        void Start()
        {
            isGazeVoxelization = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (switchGazeMode.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                isGazeVoxelization = !isGazeVoxelization;
                if (isGazeVoxelization) { Debug.Log("进入凝视点捕获模式..."); }
                else { Debug.Log("凝视点捕获模式关闭..."); }
            }
        }
    }
}

