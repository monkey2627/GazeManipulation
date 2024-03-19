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
        public SteamVR_Action_Boolean manipulateCatch; // ��ȡ����������
        public SteamVR_Action_Boolean manipulateTranslate; // ƽ�Ʋ���
        
        // �ֱ�����
        public GameObject rightHandle; // ��inspector������
        public GameObject leftHandel; // ��inspector������


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
                if (isGazeVoxelization) { Debug.Log("�������ӵ㲶��ģʽ..."); }
                else { Debug.Log("���ӵ㲶��ģʽ�ر�..."); }
            }
        }
    }
}

