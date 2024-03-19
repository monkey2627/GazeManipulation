using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;
namespace GazeHeat
{
    public enum ManipulateStatus { UNSELECTED=0,SELECTED};
    public class ManipulateManagement : MonoBehaviour
    {
        // 手柄输入脚本
        private HandleInput handleInputScript;

        // private GameObject leftHandle;
        private GameObject leftHandle;
        

        // 位移参数
        private Vector3 rightHandPositionPre; // 上一帧手柄位置
        private Vector3 offsetPosition;

        // 操纵gains参数
        public float gainTranslation;

        // 当前选中的物体
        public GameObject targetObject;
        private ManipulateStatus manipulateStatus;
        

        // Start is called before the first frame update
        void Start()
        {
            // 获取手柄输入脚本
            handleInputScript = GetComponent<HandleInput>(); // 挂载在同一父物体上
            //leftHandle = handleInputScript.leftHandle;
            leftHandle = handleInputScript.leftHandel;

            // 操纵状态
            manipulateStatus = ManipulateStatus.UNSELECTED;

        }

        // Update is called once per frame
        void Update()
        {
            
            // 选取被操纵物体
            if (handleInputScript.manipulateCatch.GetStateDown(SteamVR_Input_Sources.LeftHand))
            {
                CatchManipulatedObject(leftHandle.transform.position, leftHandle.transform.forward);
            }

            // 平移操作
            if (manipulateStatus.Equals(ManipulateStatus.SELECTED))
            {
                if (handleInputScript.manipulateTranslate.GetState(SteamVR_Input_Sources.LeftHand))
                {
                    //Debug.Log("发生平移...");
                    // 偏移量 = 当前手柄位置 - 上一帧手柄位置
                    offsetPosition = leftHandle.transform.position - rightHandPositionPre;
                    offsetPosition = offsetPosition * gainTranslation;
    
                    targetObject.transform.position += offsetPosition;
                    Debug.Log("offsetPosition:" + offsetPosition);
                    Debug.Log(targetObject.name+":" + targetObject.transform.position);
                }
            }

            rightHandPositionPre = leftHandle.transform.position;

        }

        void CatchManipulatedObject(Vector3 handPosition,Vector3 handForward)
        {
            Ray handRay = new Ray(handPosition, handForward);
            RaycastHit hitInfo;
            if(Physics.Raycast(handRay,out hitInfo,Mathf.Infinity,(1<<10)))
            {
                targetObject = hitInfo.collider.gameObject;
                while(targetObject.transform.parent != null)
                {
                    targetObject = targetObject.transform.parent.gameObject;
                }
                manipulateStatus = ManipulateStatus.SELECTED;

                Debug.Log("获取目标物体:" + targetObject.name);
            }
            else
            {
                // 空选，认为放弃当前物体
                manipulateStatus = ManipulateStatus.UNSELECTED;
            }
        }


    }
}

