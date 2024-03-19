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
        // 建立两块方形区域
        public GameObject SingleHandSpace; // 单手操作空间
        private Bounds SHSBounds;


        public GameObject SceneManipulateSpace; // 场景操作空间
        private Bounds SMSBounds;


        // 
        public GameObject VRCamera; // 虚拟现实主相机
        public GameObject Eye; // 眼睛位置，即主相机位置，与主相机同步
        public GameObject Shoulder; // 肩膀位置，手柄按键确认，相对Head位置不变


        // 手柄输入相关代码
        public GameObject RightHand;
        public GameObject LeftHand;
        // 初始化阶段，需要采集用户基本信息，目前只需要肩膀位置
        public SteamVR_Action_Boolean selectUserShoulder; // 选择用户肩膀位置
        private bool isUserShoulderSampled = false;
        // 操作阶段，用户获取物体并移动手柄进行操作

        // 操作相关
        // 当前选中的物体
        public GameObject targetObject;
        private ManipulateStatus manipulateStatus;
        public SteamVR_Action_Boolean selectManipulateObject; // 选择需要操作的物体
        public SteamVR_Action_Boolean translateManipulateObject; // 平移需要操作的物体
        // 按键同步单手操作空间和VR相机
        public SteamVR_Action_Boolean updateSHSForward; // 更新单手操作空间朝向，和VR相机同步

        // 位移参数
        private Vector3 handlePositionPre; // 上一帧手柄位置
        private Vector3 translateGain; // 平移时三个维度上的gain

        // zoom-in/out 代码
        public GameObject GazeRaySampleV2;
        private SRanipal_GazeRaySample_v2 gazeRayScript;
        public SteamVR_Action_Boolean zoomInOutCamera;
        public GameObject VRCameraRig;
        private bool isCameraRigZoomIn;
        private Vector3 CameraRigPreviousPosition;

        void Start()
        {
            manipulateStatus = ManipulateStatus.UNSELECTED;

            // 脚本初始化
            gazeRayScript = GazeRaySampleV2.GetComponent<SRanipal_GazeRaySample_v2>();
            isCameraRigZoomIn = false;

            CalculateTranslateGain();

        }

        // Update is called once per frame
        void Update()
        {
            // 需要触发的事件
            GetShoulderPosition();

            // 获取想要操作的物体
            GetManipulateObject();

            //TranslateManipulateObject();
            TranslateObjectBySyncMapping();

            ZoomCamera();

            // 每帧执行的事件
            UpdateEyePosition();

            
        }

        /// <summary>
        /// 同步眼睛与VR相机世界坐标
        /// </summary>
        void UpdateEyePosition()
        {
            Eye.transform.position = VRCamera.transform.position; 
        }

        /// <summary>
        /// 通过按键将单手操作空间的朝向与VR相机朝向同步
        /// </summary>
        void UpdateSHSForwardByPress()
        {
            if (updateSHSForward.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                // 只同步y轴的偏转，另外两轴不考虑
                Vector3 eyeEulerAngle = new Vector3(0, VRCamera.transform.eulerAngles.y, 0);
                Eye.transform.eulerAngles = eyeEulerAngle;
                Debug.Log("同步了单手操作空间的朝向");
            }

        }

        /// <summary>
        /// 获取肩膀世界坐标
        /// </summary>
        void GetShoulderPosition()
        {
            if (selectUserShoulder.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                if (!isUserShoulderSampled)
                {
                    // 获取此时的右手世界坐标并赋值给肩膀
                    Shoulder.transform.position = RightHand.transform.position;
                    isUserShoulderSampled = true;
                    Debug.Log("成功获取肩膀坐标...");
                }
            }
        }

        /// <summary>
        /// 通过手柄按键选取目标物体
        /// 目前使用射线获取:GetObjectByRaycast()
        /// </summary>
        void GetManipulateObject()
        {
            if (selectManipulateObject.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                Debug.Log("按下了获取物体的按键...");
                GetObjectByRaycast(RightHand.transform.position, RightHand.transform.forward);
            }
        }

        /// <summary>
        /// 需要被GetManipulateObject调用
        /// 算法1：通过射线获取需要操作的物体
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

                Debug.Log("获取目标物体:" + targetObject.name);
            }
            else
            {
                // 空选，认为放弃当前物体
                Debug.Log("我目标物体呢团长我和你没完！");
                manipulateStatus = ManipulateStatus.UNSELECTED;
            }
        }

        /// <summary>
        /// 通过按住手柄按键平移目标物体
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

            // 更新手柄位置
            handlePositionPre = RightHand.transform.position;
        }

        /// <summary>
        /// 静态平移gain计算函数，暂用
        /// 和TranslateManipulateObject函数配套使用
        /// </summary>
        void CalculateTranslateGain()
        {
            //SHSBounds = SingleHandSpace.GetComponent<BoxCollider>().bounds;
            //SMSBounds = SceneManipulateSpace.GetComponent<BoxCollider>().bounds;
            Vector3 SHSscale = SingleHandSpace.transform.localScale;
            Vector3 SMSscale = SceneManipulateSpace.transform.localScale;
            //Debug.Log("单手操作空间SHS包围盒size:" + SHSBounds.size.x + "," + SHSBounds.size.y + "," + SHSBounds.size.z);
            float gain_x = SMSscale.x / SHSscale.x;
            float gain_y = SMSscale.y / SHSscale.y;
            float gain_z = SMSscale.z / SHSscale.z;
            translateGain = new Vector3(gain_x, gain_y, gain_z);
            Debug.Log("平移gain:" + translateGain.ToString());
        }

        /// <summary>
        /// 物体和手柄进行位置映射，在两个空间中进行同步
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

        // 手柄按键，获取当前视线起点和方向，设定移动距离，按照该方向移动相机
        void ZoomCamera()
        {
            if (zoomInOutCamera.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                if (!isCameraRigZoomIn)
                {
                    CameraRigPreviousPosition = VRCameraRig.transform.position;
                    // 直接计算相机终点,获取视线碰撞点
                    Vector3 gazeDirection = gazeRayScript.GazeDirectionCombined;
                    Vector3 eyePosition = VRCamera.transform.position;
                    Vector3 gazeHitPoint;
                    Vector3 cameraTargetPoint;

                    Ray gazeRay = new Ray(eyePosition, gazeDirection);
                    RaycastHit hitInfo;
                    if (Physics.Raycast(gazeRay, out hitInfo, Mathf.Infinity))
                    {
                        Debug.Log("开始Zoom in");
                        gazeHitPoint = hitInfo.point;
                        cameraTargetPoint = gazeHitPoint - gazeDirection.normalized;
                        VRCameraRig.transform.position = cameraTargetPoint;
                        isCameraRigZoomIn = true;
                    }
                    else
                    {
                        Debug.Log("咋没有碰撞点啊这Zoom不了");
                    }
                }
                else
                {
                    VRCameraRig.transform.position = CameraRigPreviousPosition;
                    isCameraRigZoomIn = false;
                    Debug.Log("位置已复原");
                }

            }
        }


    }
}


