using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneAware
{
    public struct SHSVoxel
    {
        public Vector3 center; // 体素坐标
        public bool isGazeVisited;
        public int gazeTimes; // 凝视次数
        public SHSVoxel(Vector3 voxelCenter, bool isVoxcelVisited, int voxelGazeTimes)
        {
            center = voxelCenter;
            isGazeVisited = isVoxcelVisited;
            gazeTimes = voxelGazeTimes;
        }

    }
    public class SMSampler : MonoBehaviour
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

        // 手柄控制信息
        private bool isManipulateStart = false;

        // 采样信息
        List<Vector3Int> voxelIndexHeatList; // 用于记录访问过的体素的下标，按时间顺序，不会重复放入同一体素
        List<Vector3> handlePositionList; // 用于记录手柄轨迹，每个体素只会被记录一次
        List<Vector3> objectPositionList; // 用于记录物体轨迹，每个体素只会被记录一次

        [HideInInspector] public float manipulateStartTime; // Time.time获取游戏的运行时间
        [HideInInspector] public float manipulateStopTime; // 操作结束时间
        [HideInInspector] public float manipulateError; // 操作误差=目标位置-对象最终位置
        [HideInInspector] public float manipulateDistance; // 物体直线距离 = 物体起点 - 目标位置

        // 文件读取相关信息
        FileIO fileIO;
        public string fileName; // 文件名不带后缀
        public string fileType; // csv\txt等

        // Start is called before the first frame update
        void Start()
        {
            // 初始化实验采样空间
            InitialExperimentBox();
            CreateSampleVoxelArray();
            // 初始化其他变量
            voxelIndexHeatList = new List<Vector3Int>();
            handlePositionList = new List<Vector3>();
            objectPositionList = new List<Vector3>();

            fileIO = GetComponent<FileIO>();
        }

        // Update is called once per frame
        void Update()
        {

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

                        SHSVoxel newVoxel = new SHSVoxel(newVoxelCenter, false,0);

                        tempYList.Add(newVoxel);
                    }
                    tempXList.Add(tempYList);
                }
                sampleVoxelArray.Add(tempXList);
            }
        }

        /// <summary>
        /// 进行体素热度更新
        /// </summary>
        /// <param name="gazePoint">凝视点坐标</param>
        public void SampleEyeGazePoint(Vector3 gazePoint)
        {
            // 计算gazepoint对应的体素
            int xVoxelIndex = Mathf.FloorToInt((gazePoint.x - boxOrigin.x) / voxelSideLength - 0.5f) + 1;
            int yVoxelIndex = Mathf.FloorToInt((gazePoint.y - boxOrigin.y) / voxelSideLength - 0.5f) + 1;
            int zVoxelIndex = Mathf.FloorToInt((gazePoint.z - boxOrigin.z) / voxelSideLength - 0.5f) + 1;

            // 判断凝视点越界
            if (xVoxelIndex >= sampleNumberX || xVoxelIndex < 0 || yVoxelIndex >= sampleNumberY || yVoxelIndex < 0 || zVoxelIndex >= sampleNumberZ || zVoxelIndex < 0)
            {
                Debug.LogError("[WARNING]凝视点越界");
                return;
            }

            // 该体素热度+1
            SHSVoxel curVoxel = sampleVoxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex];
            curVoxel.gazeTimes += 1;
            // 判断该体素是否访问过，访问过则跳过，未访问过则加入列表

            if (!curVoxel.isGazeVisited)
            {
                voxelIndexHeatList.Add(new Vector3Int(xVoxelIndex, yVoxelIndex, zVoxelIndex));
                curVoxel.isGazeVisited = true;
            }

            sampleVoxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex] = curVoxel;

        }

        /// <summary>
        /// 记录手柄移动轨迹
        /// </summary>
        /// <param name="handlePoint">手柄坐标</param>
        public void SampleHandlePoint(Vector3 handlePoint)
        {
            // 计算handlePoint对应的体素
            int xVoxelIndex = Mathf.FloorToInt((handlePoint.x - boxOrigin.x) / voxelSideLength - 0.5f) + 1;
            int yVoxelIndex = Mathf.FloorToInt((handlePoint.y - boxOrigin.y) / voxelSideLength - 0.5f) + 1;
            int zVoxelIndex = Mathf.FloorToInt((handlePoint.z - boxOrigin.z) / voxelSideLength - 0.5f) + 1;

            // 判断手柄越界
            if (xVoxelIndex >= sampleNumberX || xVoxelIndex < 0 || yVoxelIndex >= sampleNumberY || yVoxelIndex < 0 || zVoxelIndex >= sampleNumberZ || zVoxelIndex < 0)
            {
                Debug.LogError("[WARNING]手柄越界");
                return;
            }

            SHSVoxel curVoxel = sampleVoxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex];

            handlePositionList.Add(curVoxel.center);
        }

        /// <summary>
        /// 记录物体移动轨迹
        /// </summary>
        /// <param name="objectPoint">物体坐标</param>
        public void SampleObjectPoint(Vector3 objectPoint)
        {
            // 计算objectPoint对应的体素
            int xVoxelIndex = Mathf.FloorToInt((objectPoint.x - boxOrigin.x) / voxelSideLength - 0.5f) + 1;
            int yVoxelIndex = Mathf.FloorToInt((objectPoint.y - boxOrigin.y) / voxelSideLength - 0.5f) + 1;
            int zVoxelIndex = Mathf.FloorToInt((objectPoint.z - boxOrigin.z) / voxelSideLength - 0.5f) + 1;

            // 判断物体越界
            if (xVoxelIndex >= sampleNumberX || xVoxelIndex < 0 || yVoxelIndex >= sampleNumberY || yVoxelIndex < 0 || zVoxelIndex >= sampleNumberZ || zVoxelIndex < 0)
            {
                Debug.LogError("[WARNING]物体越界");
                return;
            }

            SHSVoxel curVoxel = sampleVoxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex];

            objectPositionList.Add(curVoxel.center);
        }

        public void CalculateManipulateError(Vector3 objectPosition,Vector3 targetPosition)
        {
            manipulateError = Vector3.Distance(objectPosition, targetPosition);
        }

        public void WriteExperimentData2File()
        {
            WriteGaze2File();
            WriteHandlePosition2File();
            WriteObjectPosition2File();
            WriteManipulateResult2File();

            Debug.Log("所有实验数据写入完毕");
        }

        public void WriteGaze2File()
        {
            List<string> dataList = new List<string>();
            string filePath = "./Assets/GazeDemo/Output/GazeHeat/";
            string fullFilePath = filePath + fileName + "_gazeHeat" + "." + fileType;

            dataList.Add("x,y,z,gazetimes");

            foreach(Vector3Int voxelIndex in voxelIndexHeatList)
            {
                SHSVoxel curVoxel = sampleVoxelArray[voxelIndex.x][voxelIndex.y][voxelIndex.z];
                dataList.Add(string.Format("{0:F3},{1:F3},{2:F3},{3:D1}", curVoxel.center.x, curVoxel.center.y, curVoxel.center.z, curVoxel.gazeTimes));
            }

            fileIO.Writef(dataList, true, fullFilePath);

            Debug.Log("凝视数据 写入完毕");
        }

        public void WriteHandlePosition2File()
        {
            List<string> dataList = new List<string>();
            string filePath = "./Assets/GazeDemo/Output/Handle/";
            string fullFilePath = filePath + fileName + "_handle" + "." + fileType;

            dataList.Add("x,y,z");

            foreach (Vector3 pos in handlePositionList)
            {
                dataList.Add(string.Format("{0:F3},{1:F3},{2:F3}", pos.x, pos.y,pos.z));
            }

            fileIO.Writef(dataList, true, fullFilePath);
            Debug.Log("手柄数据 写入完毕");
        }

        public void WriteObjectPosition2File()
        {
            List<string> dataList = new List<string>();
            string filePath = "./Assets/GazeDemo/Output/MObject/";
            string fullFilePath = filePath + fileName + "_object" + "." + fileType;

            dataList.Add("x,y,z");

            foreach (Vector3 pos in objectPositionList)
            {
                dataList.Add(string.Format("{0:F3},{1:F3},{2:F3}", pos.x, pos.y, pos.z));
            }

            fileIO.Writef(dataList, true, fullFilePath);
            Debug.Log("操作对象数据 写入完毕");
        }

        /// <summary>
        /// 记录操作结果信息，目前包括：
        /// 1.标准操作距离
        /// 2.操作时间
        /// 3.操作误差
        /// </summary>
        public void WriteManipulateResult2File()
        {
            List<string> dataList = new List<string>();
            string filePath = "./Assets/GazeDemo/Output/MResult/";
            string fullFilePath = filePath + fileName + "_result" + "." + fileType;

            dataList.Add(string.Format("{0:F3},{1:F3},{2:F3}",manipulateDistance ,manipulateStopTime - manipulateStartTime, manipulateError));

            fileIO.Writef(dataList, true, fullFilePath);

            Debug.Log("操作结果数据 写入完毕");
        }
    }
}


