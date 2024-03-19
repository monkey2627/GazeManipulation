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
        [HideInInspector] public bool isGazeVoxelization;
        public SteamVR_Action_Boolean manipulateCatch; // 获取被操纵物体
        public SteamVR_Action_Boolean manipulateTranslate; // 平移操作
        
        // 手柄对象
        public GameObject rightHandle; // 在inspector中拖入
        public GameObject leftHandel; // 在inspector中拖入


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

