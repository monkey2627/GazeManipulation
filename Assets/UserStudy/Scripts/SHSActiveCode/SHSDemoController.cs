using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;


namespace UserStudy
{
    public struct SHSVoxel
    {
        public Vector3 center;
        public bool isVisited;

        public SHSVoxel(Vector3 voxelCenter,bool isVoxcelVisited)
        {
            center = voxelCenter;
            isVisited = isVoxcelVisited;
        }

    }
    public class SHSDemoController : MonoBehaviour
    {
        // 活动空间盒基本信息
        public GameObject ExperimentBox; // 实验活动空间盒子
        [HideInInspector] public Bounds boxBounds;
        [HideInInspector] public float boxLengthX;
        [HideInInspector] public float boxLengthY;
        [HideInInspector] public float boxLengthZ;
        [HideInInspector] public Vector3 boxOrigin;

        // 空间盒子体素信息
        public float voxelSideLength;
        // 空间盒三轴体素数量，由boxLength决定
        [HideInInspector] public int sampleNumberX;
        [HideInInspector] public int sampleNumberY;
        [HideInInspector] public int sampleNumberZ;
        [HideInInspector] public List<List<List<SHSVoxel>>> sampleVoxelArray; // 用于存放体素结构体


        // 手柄操作信息
        public GameObject rightHand; // 右手手柄
        public SteamVR_Action_Boolean selectUserShoulder; // 选择用户肩膀位置
        private bool isUserShoulderSelected = false; // 用户肩膀采集状况

        public SteamVR_Action_Boolean startSampleHandle; // 开始采集手柄数据
        private bool isHandleSampleStart = false;

        // 文件读写信息
        List<Vector3> positionList2File;
        FileIO fileIO;
        public string fileName;

        // 朝向

        

        // Start is called before the first frame update
        void Start()
        {
            InitialExperimentBox();
            CreateSampleVoxelArray();

            // 变量初始化
            positionList2File = new List<Vector3>();
            fileIO = GetComponent<FileIO>();
        }

        // Update is called once per frame
        void Update()
        {
            SampleShoulderPosition();
            SampleHandleMovement();
        }

        private void InitialExperimentBox()
        {
            // 实验盒需要带碰撞盒
            boxBounds = ExperimentBox.GetComponent<BoxCollider>().bounds;
            boxLengthX = boxBounds.size.x;
            boxLengthY = boxBounds.size.y;
            boxLengthZ = boxBounds.size.z;
            boxOrigin = boxBounds.min;
        }

        /// <summary>
        /// 初始化体素结构体阵列，计算出中心点
        /// </summary>
        private void CreateSampleVoxelArray()
        {
            // 计算三个轴上采样点数量，向上取整
            sampleNumberX = Mathf.CeilToInt(boxLengthX / voxelSideLength);
            sampleNumberY = Mathf.CeilToInt(boxLengthY / voxelSideLength);
            sampleNumberZ = Mathf.CeilToInt(boxLengthZ / voxelSideLength);

            sampleVoxelArray = new List<List<List<SHSVoxel>>>();

            for (int ix = 0; ix < sampleNumberX; ix++)
            {
                List<List<SHSVoxel>> tempXList = new List<List<SHSVoxel>>();
                for (int iy = 0; iy < sampleNumberY; iy++)
                {
                    List<SHSVoxel> tempYList = new List<SHSVoxel>();
                    for (int iz = 0; iz < sampleNumberZ; iz++)
                    {
                        
                        Vector3 newVoxelCenter = boxOrigin +
                            ix * Vector3.right * (voxelSideLength) +
                            iy * Vector3.up * (voxelSideLength) +
                            iz * Vector3.forward * (voxelSideLength);
                        
                        SHSVoxel newVoxel = new SHSVoxel(newVoxelCenter,false);

                        tempYList.Add(newVoxel);
                    }
                    tempXList.Add(tempYList);
                }
                sampleVoxelArray.Add(tempXList);
            }
        }

        /// <summary>
        /// 获取肩膀世界坐标
        /// </summary>
        void SampleShoulderPosition()
        {
            if (selectUserShoulder.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                if (!isUserShoulderSelected)
                {
                    // 获取此时的右手世界坐标为肩膀位置，写入存储列表
                    positionList2File.Add(rightHand.transform.position);

                    isUserShoulderSelected = true;
                    Debug.Log("成功获取肩膀坐标");
                }
                else
                {
                    Debug.Log("[重复操作]肩膀坐标已获取");
                }
            }
        }

        /// <summary>
        /// 计算手柄坐标对应的体素序号
        /// </summary>
        private void HandlePosition2Voxel(Vector3 handlePosition)
        {
            int xVoxelIndex = Mathf.FloorToInt((handlePosition.x - boxOrigin.x) / voxelSideLength - 0.5f) + 1;
            int yVoxelIndex = Mathf.FloorToInt((handlePosition.y - boxOrigin.y) / voxelSideLength - 0.5f) + 1;
            int zVoxelIndex = Mathf.FloorToInt((handlePosition.z - boxOrigin.z) / voxelSideLength - 0.5f) + 1;

            // 判断手柄越界
            if (xVoxelIndex >= sampleNumberX || xVoxelIndex < 0 || yVoxelIndex >= sampleNumberY || yVoxelIndex < 0 || zVoxelIndex >= sampleNumberZ || zVoxelIndex < 0)
            {
                Debug.LogError("[ERROR]手柄越界");
                return;
            }

            SHSVoxel curVoxel = sampleVoxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex];

            if (!curVoxel.isVisited)
            {
                //将中心坐标输出到文件中
                positionList2File.Add(curVoxel.center);

                // 该体素标记为访问过
                curVoxel.isVisited = true;

                // 更新体素列表
                sampleVoxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex] = curVoxel;
            }
        }

        private void SampleHandleMovement()
        {
            if (startSampleHandle.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                isHandleSampleStart = !isHandleSampleStart;
                if (isHandleSampleStart)
                {
                    Debug.Log("开始采样手柄数据...");
                }
                else
                {
                    Debug.Log("结束采样手柄数据,将数据写入文件...");
                    WritePosition2File();


                }
            }

            if (isHandleSampleStart)
            {
                HandlePosition2Voxel(rightHand.transform.position);

            }
        }

        private void WritePosition2File()
        {
            List<string> dataList = new List<string>();
            string filePath = "./Assets/UserStudy/Output/";
            

            // 将Vector列表转为a,b,c的string
            foreach(Vector3 pos in positionList2File)
            {
                dataList.Add(string.Format("{0:F3},{1:F3},{2:F3}", pos.x, pos.y, pos.z));
            }

            
            fileIO.Writef(dataList, true, filePath + fileName);

        }
        

    }
}
