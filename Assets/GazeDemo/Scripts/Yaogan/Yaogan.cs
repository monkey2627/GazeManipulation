using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using Valve.VR;
using Valve.VR.Extras;

    public class Yaogan : MonoBehaviour
    {

        // public GameObject mainCameraObject; // VR相机对象,需要拖入
        private LineRenderer line;
        public GameObject rightHand;

  
        public GameObject userHead;
        public SteamVR_Action_Vector2 rightRocker; 
        public SteamVR_Action_Vector2 leftRocker; 
        public SteamVR_Action_Boolean xbRotate;
        public SteamVR_Action_Boolean yaRotate;

        // 4.射线选取目标物体
        public GameObject targetObject;
        public SteamVR_Action_Boolean selectManipulateObject; // 选择需要操作的物体

        public GameObject[] targetposition;//目标位置
        void Start()
        {
            targetObject = null;
            line = GetComponent<LineRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            //射线选中一个物体
            if(targetObject == null)
                GetManipulateObject();
        if (targetObject != null) { 
            TranslateObjectByRocker();

            CheckTarget();}
            // 采样程序

           // SampleManipulateData();
        }
        public void CheckTarget()
    {
        foreach (var item in targetposition)
        {
            if((item.transform.position - targetObject.transform.position).magnitude <= 0.02)
            {
                targetObject.transform.position = item.transform.position;
                targetObject.transform.rotation = item.transform.rotation;
                targetObject = null;
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
               //模式变成 选择物体模式
                // 时间采样
              //  if (isSampleData)
               // {
                //    smSamplerScript.manipulateStartTime = Time.time; // 获取游戏当前时间
                //    smSamplerScript.manipulateDistance = Vector3.Distance(targetObject.transform.position,standardObject.transform.position); // 计算起点和终点的距离
              //  }

                Debug.Log("获取目标物体:" + targetObject.name);
            }
            else
            {
                // 空选，认为放弃当前物体
                Debug.Log("我目标物体呢团长我和你没完！");
             
                
            }
        }

   
        /// <summary>
        /// 物体和手柄进行位置映射，在两个空间中进行同步
        /// </summary>
        void TranslateObjectByRocker()
        {
            //旋转
            if (xbRotate.GetState(SteamVR_Input_Sources.RightHand) && !yaRotate.GetState(SteamVR_Input_Sources.RightHand))
            {
            Debug.Log("2");
                //绕x轴旋转一定角度
                //如果是围绕自身的x轴进行旋转，就是transform.Rotate(new Vector3(1, 0, 0));
                //如果是围绕自身的y轴进行旋转，就是transform.Rotate(new Vector3(0, 1, 0));
                //如果是围绕自身的z轴进行旋转，就是transform.Rotate(new Vector3(0, 0, 1));
                targetObject.transform.Rotate(new Vector3(1, 0, 0));
            }

           else if (!xbRotate.GetState(SteamVR_Input_Sources.RightHand) && yaRotate.GetState(SteamVR_Input_Sources.RightHand))
            {
            Debug.Log("1");
            //绕x轴旋转一定角度
            //如果是围绕自身的x轴进行旋转，就是transform.Rotate(new Vector3(1, 0, 0));
            //如果是围绕自身的y轴进行旋转，就是transform.Rotate(new Vector3(0, 1, 0));
            //如果是围绕自身的z轴进行旋转，就是transform.Rotate(new Vector3(0, 0, 1));
            targetObject.transform.Rotate(new Vector3(0, 1, 0));
            }

           else if (xbRotate.GetState(SteamVR_Input_Sources.RightHand) && yaRotate.GetState(SteamVR_Input_Sources.RightHand))
            {
            //绕x轴旋转一定角度
            //如果是围绕自身的x轴进行旋转，就是transform.Rotate(new Vector3(1, 0, 0));
            //如果是围绕自身的y轴进行旋转，就是transform.Rotate(new Vector3(0, 1, 0));
            //如果是围绕自身的z轴进行旋转，就是transform.Rotate();
            targetObject.transform.Rotate(new Vector3(0, 0, 1));
            }

            var temp = 0.01f * rightRocker.GetAxis(SteamVR_Input_Sources.RightHand);
            //移动
            if (temp.magnitude > 0)
            {
                targetObject.transform.position += new Vector3(temp.x,temp.y,0);
            }
            temp = 0.01f * leftRocker.GetAxis(SteamVR_Input_Sources.LeftHand);
            //移动
            if (temp.magnitude > 0)
            {
                targetObject.transform.position += new Vector3(0, 0, temp.y);
            }
        }
      /*  void SampleManipulateData()
        {
           // if (!isSampleData) { return; }
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
           // smSamplerScript.SampleHandlePoint(rightHand.transform.position);
            // 采样物体移动
            smSamplerScript.SampleObjectPoint(targetObject.transform.position);
        }*/
    }


