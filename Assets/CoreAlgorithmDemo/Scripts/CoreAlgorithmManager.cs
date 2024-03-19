using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;
using ViveSR.anipal.Eye;
namespace CoreAlgorithm
{
    public enum ManipulateStatus { UNSELECTED = 0, SELECTED };
    public class CoreAlgorithmManager : MonoBehaviour
    {
        // Start is called before the first frame update
        // �������鷽������
        public GameObject SingleHandSpace; // ���ֲ����ռ�
        private Bounds SHSBounds;


        public GameObject SceneManipulateSpace; // ���������ռ�
        private Bounds SMSBounds;


        // 
        public GameObject VRCamera; // ������ʵ�����
        public GameObject Eye; // �۾�λ�ã��������λ�ã��������ͬ��
        public GameObject Shoulder; // ���λ�ã��ֱ�����ȷ�ϣ����Headλ�ò���


        // �ֱ�������ش���
        public GameObject RightHand;
        public GameObject LeftHand;
        // ��ʼ���׶Σ���Ҫ�ɼ��û�������Ϣ��Ŀǰֻ��Ҫ���λ��
        public SteamVR_Action_Boolean selectUserShoulder; // ѡ���û����λ��
        private bool isUserShoulderSampled = false;
        // �����׶Σ��û���ȡ���岢�ƶ��ֱ����в���

        // �������
        // ��ǰѡ�е�����
        public GameObject targetObject;
        private ManipulateStatus manipulateStatus;
        public SteamVR_Action_Boolean selectManipulateObject; // ѡ����Ҫ����������
        public SteamVR_Action_Boolean translateManipulateObject; // ƽ����Ҫ����������
        // ����ͬ�����ֲ����ռ��VR���
        public SteamVR_Action_Boolean updateSHSForward; // ���µ��ֲ����ռ䳯�򣬺�VR���ͬ��

        // λ�Ʋ���
        private Vector3 handlePositionPre; // ��һ֡�ֱ�λ��
        private Vector3 translateGain; // ƽ��ʱ����ά���ϵ�gain

        // zoom-in/out ����
        public GameObject GazeRaySampleV2;
        private SRanipal_GazeRaySample_v2 gazeRayScript;
        public SteamVR_Action_Boolean zoomInOutCamera;
        public GameObject VRCameraRig;
        private bool isCameraRigZoomIn;
        private Vector3 CameraRigPreviousPosition;

        void Start()
        {
            manipulateStatus = ManipulateStatus.UNSELECTED;

            // �ű���ʼ��
            gazeRayScript = GazeRaySampleV2.GetComponent<SRanipal_GazeRaySample_v2>();
            isCameraRigZoomIn = false;

            CalculateTranslateGain();

        }

        // Update is called once per frame
        void Update()
        {
            // ��Ҫ�������¼�
            GetShoulderPosition();

            // ��ȡ��Ҫ����������
            GetManipulateObject();

            //TranslateManipulateObject();
            TranslateObjectBySyncMapping();

            ZoomCamera();

            // ÿִ֡�е��¼�
            UpdateEyePosition();

            
        }

        /// <summary>
        /// ͬ���۾���VR�����������
        /// </summary>
        void UpdateEyePosition()
        {
            Eye.transform.position = VRCamera.transform.position; 
        }

        /// <summary>
        /// ͨ�����������ֲ����ռ�ĳ�����VR�������ͬ��
        /// </summary>
        void UpdateSHSForwardByPress()
        {
            if (updateSHSForward.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                // ֻͬ��y���ƫת���������᲻����
                Vector3 eyeEulerAngle = new Vector3(0, VRCamera.transform.eulerAngles.y, 0);
                Eye.transform.eulerAngles = eyeEulerAngle;
                Debug.Log("ͬ���˵��ֲ����ռ�ĳ���");
            }

        }

        /// <summary>
        /// ��ȡ�����������
        /// </summary>
        void GetShoulderPosition()
        {
            if (selectUserShoulder.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                if (!isUserShoulderSampled)
                {
                    // ��ȡ��ʱ�������������겢��ֵ�����
                    Shoulder.transform.position = RightHand.transform.position;
                    isUserShoulderSampled = true;
                    Debug.Log("�ɹ���ȡ�������...");
                }
            }
        }

        /// <summary>
        /// ͨ���ֱ�����ѡȡĿ������
        /// Ŀǰʹ�����߻�ȡ:GetObjectByRaycast()
        /// </summary>
        void GetManipulateObject()
        {
            if (selectManipulateObject.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                Debug.Log("�����˻�ȡ����İ���...");
                GetObjectByRaycast(RightHand.transform.position, RightHand.transform.forward);
            }
        }

        /// <summary>
        /// ��Ҫ��GetManipulateObject����
        /// �㷨1��ͨ�����߻�ȡ��Ҫ����������
        /// </summary>
        /// <param name="handPosition"></param>
        /// <param name="handForward"></param>
        void GetObjectByRaycast(Vector3 handPosition, Vector3 handForward)
        {
            Ray handRay = new Ray(handPosition, handForward);
            RaycastHit hitInfo;
            if (Physics.Raycast(handRay, out hitInfo, Mathf.Infinity, (1 << 10)))
            {
                targetObject = hitInfo.collider.gameObject;
                while (targetObject.transform.parent != null)
                {
                    targetObject = targetObject.transform.parent.gameObject;
                }
                manipulateStatus = ManipulateStatus.SELECTED;

                Debug.Log("��ȡĿ������:" + targetObject.name);
            }
            else
            {
                // ��ѡ����Ϊ������ǰ����
                Debug.Log("��Ŀ���������ų��Һ���û�꣡");
                manipulateStatus = ManipulateStatus.UNSELECTED;
            }
        }

        /// <summary>
        /// ͨ����ס�ֱ�����ƽ��Ŀ������
        /// </summary>
        void TranslateManipulateObject()
        {
            if (manipulateStatus.Equals(ManipulateStatus.SELECTED))
            {
                if (translateManipulateObject.GetState(SteamVR_Input_Sources.RightHand))
                {
                    Vector3 offsetHandlePosition = RightHand.transform.position - handlePositionPre;
                    
                    
                    Vector3 offsetObjectPosition = new Vector3(
                        offsetHandlePosition.x * translateGain.x,
                        offsetHandlePosition.y * translateGain.y,
                        offsetHandlePosition.z * translateGain.z
                        );

                    targetObject.transform.position += offsetObjectPosition;
                }
            }

            // �����ֱ�λ��
            handlePositionPre = RightHand.transform.position;
        }

        /// <summary>
        /// ��̬ƽ��gain���㺯��������
        /// ��TranslateManipulateObject��������ʹ��
        /// </summary>
        void CalculateTranslateGain()
        {
            //SHSBounds = SingleHandSpace.GetComponent<BoxCollider>().bounds;
            //SMSBounds = SceneManipulateSpace.GetComponent<BoxCollider>().bounds;
            Vector3 SHSscale = SingleHandSpace.transform.localScale;
            Vector3 SMSscale = SceneManipulateSpace.transform.localScale;
            //Debug.Log("���ֲ����ռ�SHS��Χ��size:" + SHSBounds.size.x + "," + SHSBounds.size.y + "," + SHSBounds.size.z);
            float gain_x = SMSscale.x / SHSscale.x;
            float gain_y = SMSscale.y / SHSscale.y;
            float gain_z = SMSscale.z / SHSscale.z;
            translateGain = new Vector3(gain_x, gain_y, gain_z);
            Debug.Log("ƽ��gain:" + translateGain.ToString());
        }

        /// <summary>
        /// ������ֱ�����λ��ӳ�䣬�������ռ��н���ͬ��
        /// </summary>
        void TranslateObjectBySyncMapping()
        {
            if (manipulateStatus.Equals(ManipulateStatus.SELECTED))
            {
                SHSBounds = SingleHandSpace.GetComponent<BoxCollider>().bounds;
                SMSBounds = SceneManipulateSpace.GetComponent<BoxCollider>().bounds;

                Vector3 SHSOrigin = SHSBounds.min;
                Vector3 SMSOrigin = SMSBounds.min;

                Vector3 handlePosition = RightHand.transform.position;

                Vector3 handleSpaceRate = new Vector3(
                    System.Math.Abs(handlePosition.x - SHSOrigin.x) / SHSBounds.size.x,
                    System.Math.Abs(handlePosition.y - SHSOrigin.y) / SHSBounds.size.y,
                    System.Math.Abs(handlePosition.z - SHSOrigin.z) / SHSBounds.size.z
                    );
                
                Vector3 sceneSpacePosition = new Vector3(
                    SMSOrigin.x + handleSpaceRate.x * SMSBounds.size.x,
                    SMSOrigin.y + handleSpaceRate.y * SMSBounds.size.y,
                    SMSOrigin.z + handleSpaceRate.z * SMSBounds.size.z
                    );

                targetObject.transform.position = sceneSpacePosition;
            }
        }

        // �ֱ���������ȡ��ǰ�������ͷ����趨�ƶ����룬���ո÷����ƶ����
        void ZoomCamera()
        {
            if (zoomInOutCamera.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                if (!isCameraRigZoomIn)
                {
                    CameraRigPreviousPosition = VRCameraRig.transform.position;
                    // ֱ�Ӽ�������յ�,��ȡ������ײ��
                    Vector3 gazeDirection = gazeRayScript.GazeDirectionCombined;
                    Vector3 eyePosition = VRCamera.transform.position;
                    Vector3 gazeHitPoint;
                    Vector3 cameraTargetPoint;

                    Ray gazeRay = new Ray(eyePosition, gazeDirection);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(gazeRay, out hitInfo, Mathf.Infinity))
                    {
                        Debug.Log("��ʼZoom in");
                        gazeHitPoint = hitInfo.point;
                        cameraTargetPoint = gazeHitPoint - gazeDirection.normalized;
                        VRCameraRig.transform.position = cameraTargetPoint;
                        isCameraRigZoomIn = true;
                    }
                    else
                    {
                        Debug.Log("զû����ײ�㰡��Zoom����");
                    }
                }
                else
                {
                    VRCameraRig.transform.position = CameraRigPreviousPosition;
                    isCameraRigZoomIn = false;
                    Debug.Log("λ���Ѹ�ԭ");
                }

            }
        }


    }
}


