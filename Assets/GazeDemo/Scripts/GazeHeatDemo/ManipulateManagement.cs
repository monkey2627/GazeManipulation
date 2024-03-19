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
        // �ֱ�����ű�
        private HandleInput handleInputScript;

        // private GameObject leftHandle;
        private GameObject leftHandle;
        

        // λ�Ʋ���
        private Vector3 rightHandPositionPre; // ��һ֡�ֱ�λ��
        private Vector3 offsetPosition;

        // ����gains����
        public float gainTranslation;

        // ��ǰѡ�е�����
        public GameObject targetObject;
        private ManipulateStatus manipulateStatus;
        

        // Start is called before the first frame update
        void Start()
        {
            // ��ȡ�ֱ�����ű�
            handleInputScript = GetComponent<HandleInput>(); // ������ͬһ��������
            //leftHandle = handleInputScript.leftHandle;
            leftHandle = handleInputScript.leftHandel;

            // ����״̬
            manipulateStatus = ManipulateStatus.UNSELECTED;

        }

        // Update is called once per frame
        void Update()
        {
            
            // ѡȡ����������
            if (handleInputScript.manipulateCatch.GetStateDown(SteamVR_Input_Sources.LeftHand))
            {
                CatchManipulatedObject(leftHandle.transform.position, leftHandle.transform.forward);
            }

            // ƽ�Ʋ���
            if (manipulateStatus.Equals(ManipulateStatus.SELECTED))
            {
                if (handleInputScript.manipulateTranslate.GetState(SteamVR_Input_Sources.LeftHand))
                {
                    //Debug.Log("����ƽ��...");
                    // ƫ���� = ��ǰ�ֱ�λ�� - ��һ֡�ֱ�λ��
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

                Debug.Log("��ȡĿ������:" + targetObject.name);
            }
            else
            {
                // ��ѡ����Ϊ������ǰ����
                manipulateStatus = ManipulateStatus.UNSELECTED;
            }
        }


    }
}

