using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using Valve.VR;
using Valve.VR.Extras;

namespace SceneAware
{
    public enum ManipulateStatus { UNSELECTED = 0, SELECTED };
    public class SpaceManipulateController : MonoBehaviour
    {

        // public GameObject mainCameraObject; // VR相机对象,需要拖入
        private LineRenderer line;

        // 手柄输入相关代码
        public GameObject rightHand; // 需要拖入

        // 单手操作空间相关信息
        public float userArmLength; // 用户手臂长度，需要输入
        public float userBoomLength; // 用户大臂长度，用于计算手肘位置

        // 视口相关信息
        private Camera mainCamera;
        private float halfFov; // 垂直FOV一半，弧度
        private float aspectFov; // 视锥体的宽高比
        public float farPlaneValue;

        // 父物体为Head的手柄对象复制
        public GameObject localHandle;

        /// <summary>
        /// 操作流程相关变量
        /// </summary>
        // 1.手柄按键获取肩膀位置
        public GameObject userShoulder; // 用户肩膀对象，需要拖入
        public SteamVR_Action_Boolean selectUserShoulder; // 选择用户肩膀位置
        private bool isUserShoulderSampled;
        // 2.手柄按键更新头部朝向（肩膀为其子物体）
        public GameObject userHead;
        public SteamVR_Action_Boolean updateUserHead; // 更新单手操作空间朝向
        // 3.手柄按键更新视口朝向
        private List<Vector3> cameraInfo; // 坐标、前方、上方、右方
        public SteamVR_Action_Boolean updateUserFov; // 更新主相机位置和朝向
        // 4.手柄按键选取目标物体
        public GameObject targetObject;
        private ManipulateStatus manipulateStatus;
        public SteamVR_Action_Boolean selectManipulateObject; // 选择需要操作的物体


        // 凝视相关脚本
        // 眼部跟踪信息
        public GameObject GazeRaySample;
        private SRanipal_GazeRaySample_v2 sRanipalGazeSample;

        // 采样相关变量
        public GameObject standardObject; // 用于计算误差的目标物体
        public bool isSampleData = false;
        private SMSampler smSamplerScript; // 采样脚本


        void Start()
        {
            mainCamera = Camera.main;
            line = GetComponent<LineRenderer>();
            halfFov = (mainCamera.fieldOfView * 0.5f) * Mathf.Deg2Rad;
            aspectFov = mainCamera.aspect;

            manipulateStatus = ManipulateStatus.UNSELECTED;
            isUserShoulderSampled = false;

            cameraInfo = new List<Vector3>(new Vector3[4]);
            //Debug.Log("camerainfo:" + cameraInfo.Count);

            // 初始化凝视脚本
            sRanipalGazeSample = GazeRaySample.GetComponent<SRanipal_GazeRaySample_v2>();
            // 初始化采样脚本
            if (isSampleData)
            {
                smSamplerScript = GetComponent<SMSampler>();
            }

           

        }

        // Update is called once per frame
        void Update()
        {
            UpdateLocalHandle();//localhandle与右手柄位置同步

            GetShoulderPosition();//左手x键

            UpdateSHSForwardByPress();//右手b

            UpdateUserFovByPress();//右手A

            // 正式启动
            GetManipulateObject();//点击右手上面的扳机键,选取射中的目标物体
           if(targetObject)
            {
                Debug.Log(targetObject.transform.position);
            }
            TranslateObjectBySyncMapping();

            // 采样程序

            SampleManipulateData();
        }

        /// <summary>
        /// 更新手柄复制对象的世界坐标，与手柄同步
        /// </summary>
        void UpdateLocalHandle()
        {
            localHandle.transform.position = rightHand.transform.position;
        }

        /// <summary>
        /// 按键获取肩膀世界坐标
        /// </summary>
        void GetShoulderPosition()
        {
            if (selectUserShoulder.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
               // if (!isUserShoulderSampled)
                {
                    // 获取此时的右手世界坐标并赋值给肩膀
                    userShoulder.transform.position = rightHand.transform.position;
                    isUserShoulderSampled = true;
                    Debug.Log("成功获取肩膀坐标");
                }
            }
        }

        /// <summary>
        /// 通过按键将单手操作空间的朝向与VR相机朝向同步
        /// </summary>
        void UpdateSHSForwardByPress()
        {
            if (updateUserHead.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                // 只同步y轴和x轴的偏转，z轴不考虑
                Vector3 eyeEulerAngle = new Vector3(0, mainCamera.transform.eulerAngles.y, mainCamera.transform.eulerAngles.z);
                userHead.transform.eulerAngles = eyeEulerAngle;
                // 同步空间位置
                userHead.transform.position = mainCamera.transform.position;
                Debug.Log("更新了单手操作空间的朝向");
            }

        }

        /// <summary>
        /// 通过按键将用户视野朝向与VR相机朝向同步
        /// </summary>
        void UpdateUserFovByPress()
        {
            if (updateUserFov.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                cameraInfo[0] = mainCamera.transform.position;
                cameraInfo[1] = mainCamera.transform.forward;
                cameraInfo[2] = mainCamera.transform.right;
                cameraInfo[3] = mainCamera.transform.up;
                Debug.Log("更新了用户FOV的Transform:");
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
                GetObjectByRaycast(rightHand.transform.position, rightHand.transform.forward);
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
            line.SetPosition(0, handPosition);
            line.SetPosition(1, handPosition + 100f * handForward);
            Ray handRay = new Ray(handPosition, handForward);
            RaycastHit hitInfo;
            if (Physics.Raycast(handRay, out hitInfo, Mathf.Infinity, (1 << 10)))
            {
                targetObject = hitInfo.collider.gameObject;
                while (targetObject.transform.parent != null)
                {
                    targetObject = targetObject.transform.parent.gameObject;
                }
                manipulateStatus = ManipulateStatus.SELECTED;//模式变成 选择物体模式
                // 时间采样
                if (isSampleData)
                {
                    smSamplerScript.manipulateStartTime = Time.time; // 获取游戏当前时间
                    smSamplerScript.manipulateDistance = Vector3.Distance(targetObject.transform.position,standardObject.transform.position); // 计算起点和终点的距离
                }

                Debug.Log("获取目标物体:" + targetObject.name);
            }
            else
            {
                // 空选，认为放弃当前物体
                Debug.Log("我目标物体呢团长我和你没完！");
                if (manipulateStatus.Equals(ManipulateStatus.SELECTED))
                {
                    Debug.Log("释放物体，操作结束");
                    manipulateStatus = ManipulateStatus.UNSELECTED;
                    // 时间采样
                    if (isSampleData)
                    {
                        Debug.Log("开始写入本次操作数据");
                        smSamplerScript.manipulateStopTime = Time.time; // 获取游戏当前时间
                        smSamplerScript.CalculateManipulateError(targetObject.transform.position, standardObject.transform.position);
                        smSamplerScript.WriteExperimentData2File();
                    }
                }
                
            }
        }

        /// <summary>
        /// 根据手柄局部坐标计算虚拟对象在场景操作空间中的位置
        /// 单手操作空间为球体
        /// </summary>
        /// <param name="p_h">手柄坐标</param>
        /// <returns>操作对象的坐标</returns>
        Vector3 GetSMSObjectPosition(Vector3 p_h)
        {
            float d_np = mainCamera.nearClipPlane; // distance_near_plane
            float d_fp = mainCamera.farClipPlane; // distance_far_plane
            d_fp = 7f; // 测试时规定
            float d_fn = d_fp - d_np; // 平截头体的高
            float l_arm = userArmLength;
            Vector3 p_s = userShoulder.transform.localPosition;

            Vector3 p_c = cameraInfo[0]; // 相机位置

            //Debug.Log("p_s:" + p_s.ToString());
            //Debug.Log("p_h:" + p_h.ToString());

            // 计算zo
            float z_rate = Mathf.Abs(p_h.z - p_s.z) / l_arm;
            float z_o; // 标量，之后需要乘以方向向量
            if (p_h.z > p_s.z)
            {
                z_o = d_np + d_fn * z_rate;
            }
            else
            {
                z_o = d_np - d_fn * z_rate;
            }

            // 计算z_o处的屏幕长宽
            float height_o = z_o * Mathf.Tan(halfFov);
            float width_o = height_o * aspectFov;


            // 计算xo
            float l_yx = Mathf.Sqrt(Mathf.Pow(l_arm, 2) - Mathf.Pow(p_h.y - p_s.y, 2) - Mathf.Pow(p_h.z - p_s.z, 2));
            //Debug.Log("l_yx:" + l_yx.ToString());
            float rate_h_x = Mathf.Abs(p_h.x - p_s.x) / l_yx;
            //Debug.Log("rate_h_x:" + rate_h_x.ToString());
            float x_o;
            if (p_h.x > p_s.x)
            {
                x_o = (width_o / 2f) * rate_h_x;
            }
            else
            {
                x_o = -(width_o / 2f) * rate_h_x;
            }

            // 计算yo
            float l_xz = Mathf.Sqrt(Mathf.Pow(l_arm, 2) - Mathf.Pow(p_h.y - p_s.y, 2) - Mathf.Pow(p_h.x - p_s.x, 2));
            float rate_h_y = Mathf.Abs(p_h.y - p_s.y) / l_xz;
            float y_o;

            if (p_h.y > p_s.y)
            {
                y_o = (height_o / 2f) * rate_h_y;
            }
            else
            {
                y_o = -(height_o / 2f) * rate_h_y;
            }

            // 计算p_o
            Vector3 p_o = p_c + cameraInfo[1] * z_o + cameraInfo[2] * x_o + cameraInfo[3] * y_o;
            return p_o;

        }

        /// <summary>
        /// 根据手柄局部坐标计算虚拟对象位置，单手操作空间为倒立半球
        /// </summary>
        /// <param name="p_h"></param>
        /// <returns></returns>
        Vector3 GetSMSObjectPositionHemi(Vector3 p_h)
        {
            float d_np = mainCamera.nearClipPlane; // distance_near_plane
            float d_fp = mainCamera.farClipPlane; // distance_far_plane
            d_fp = farPlaneValue; // 测试时规定
            float d_fn = d_fp - d_np; // 平截头体的高
            float r_arm = 1.1f*(userArmLength-userBoomLength); // 适当放大userArmLength

            Vector3 p_shoulder = userShoulder.transform.localPosition; // 肩膀局部坐标
            Vector3 p_elbow = p_shoulder - userBoomLength * Vector3.down; // 手肘坐标=肩膀坐标-大臂长度*(0,-1,0)
            Vector3 p_camera = cameraInfo[0]; // 相机位置

            // 计算zo

            float z_distance = p_h.z - p_elbow.z;
            float z_abs_dis;
            if (z_distance > r_arm)
            {
                z_abs_dis = r_arm;
            }
            else if (z_distance > 0)
            {
                z_abs_dis = z_distance;
            }
            else
            {
                z_abs_dis = 0;
            }

            float z_rate = z_distance / r_arm;

            float z_object = d_np + d_fn * z_rate;

            // 计算z_object处的屏幕长宽
            float height_object = z_object * Mathf.Tan(halfFov);
            float width_object = height_object * aspectFov;

            
            
            float r_xy_2 = Mathf.Pow(r_arm, 2) - Mathf.Pow(r_arm - z_abs_dis, 2);
            if (r_xy_2 < 0) { Debug.LogError("映射圆面半径计算小于0,请检查坐标初始化.."); }
            float r_xy = Mathf.Sqrt(r_xy_2); // 映射圆面半径

            // 计算x_object
            float x_abs_dis = Mathf.Abs(p_h.x - p_elbow.x);
            float x_rate;
            if (x_abs_dis > r_xy)
            {
                x_rate = 1;
            }
            else
            {
                x_rate = x_abs_dis / r_xy;
            }      

            float x_object;
            if (p_h.x > p_elbow.x)
            {
                x_object = (width_object / 2f) * x_rate;
            }
            else
            {
                x_object = -(width_object / 2f) * x_rate;
            }

            // 计算y_object

            float y_abs_dis = Mathf.Abs(p_h.y - p_elbow.y);
            float y_rate;

            if (y_abs_dis > r_xy)
            {
                y_rate = 1;
            }
            else
            {
                y_rate = y_abs_dis / r_xy;
            }

            float y_object;
            if (p_h.y > p_elbow.y)
            {
                y_object = (height_object / 2f) * y_rate;
            }
            else
            {
                y_object = -(height_object / 2f) * y_rate;
            }

            // 计算p_o
            Vector3 p_object = p_camera + cameraInfo[1] * z_object + cameraInfo[2] * x_object + cameraInfo[3] * y_object;
            return p_object;
        }


        /// <summary>
        /// 物体和手柄进行位置映射，在两个空间中进行同步
        /// </summary>
        void TranslateObjectBySyncMapping()
        {
            if (manipulateStatus.Equals(ManipulateStatus.SELECTED))
            {
                targetObject.transform.position = GetSMSObjectPositionHemi(localHandle.transform.localPosition);//根据手柄的位置进行移动
            }
        }


        void SampleManipulateData()
        {
            if (!isSampleData) { return; }
            if (manipulateStatus.Equals(ManipulateStatus.UNSELECTED)) { return; }
            // 采样凝视点-改为用头部射线代替
            //Vector3 gazeDirect = sRanipalGazeSample.GazeDirectionCombined;
            Vector3 gazeDirect = Camera.main.transform.forward;
            //Vector3 eyePosition = Camera.main.transform.position;
            Vector3 eyePosition = Camera.main.transform.position;
            Ray gazeRay = new Ray(eyePosition, gazeDirect);
            RaycastHit hitInfo;
            if (Physics.Raycast(gazeRay, out hitInfo))
              {
                 smSamplerScript.SampleEyeGazePoint(hitInfo.point);
              }

            // 采样手柄
            smSamplerScript.SampleHandlePoint(rightHand.transform.position);
            // 采样物体移动
            smSamplerScript.SampleObjectPoint(targetObject.transform.position);
        }
    }

}
