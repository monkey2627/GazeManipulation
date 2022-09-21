using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;
using Valve.VR;
using Valve.VR.Extras;

namespace GazeHeat
{
    /// <summary>
    /// 房间信息结构体
    /// </summary>
    public struct RoomBox
    {
        public Bounds RoomBounds;
        public float XLength;
        public float YLength;
        public float ZLength;
        public Vector3 BoxOriginPoint; // 坐标轴原点
    }

    public struct Voxel
    {
        public GameObject voxelObject; // 体素物体对象
        public int gazeTimes; // 被凝视的次数
    }

    public class RoomVoxelization : MonoBehaviour
    {
        public GameObject roomObject;
        public GameObject voxelPrefab;

        private RoomBox roomBox; // 场景基本信息结构体

        // public Material VoxelBeGazedMaterial;

        // 体素阵列信息
        public float voxcelSideLength; // 体素边长(默认正方体)
        private GameObject voxcelParent; // 体素阵列空父物体
        [HideInInspector] public List<List<List<Voxel>>> voxelArray;
        [HideInInspector] public int xVoxellNumber; // x轴上体素数量
        [HideInInspector] public int yVoxellNumber; // y轴上体素数量
        [HideInInspector] public int zVoxellNumber; // z轴上体素数量

        // 凝视点信息
        public GameObject GazeRaySample;
        private SRanipal_GazeRaySample_v2 sRanipalGazeSample;

        // VR手柄操作变量
        public SteamVR_Action_Boolean switchGazeMode;
        private bool isGazeVoxelization =false;

        // 凝视点物体
        public GameObject gazeCursorPrefab;
        private GameObject gazeCursorObject;

        // Start is called before the first frame update
        void Start()
        {
            // 获取凝视点脚本
            sRanipalGazeSample = GazeRaySample.GetComponent<SRanipal_GazeRaySample_v2>();

            // 获取包围盒
            Bounds roomBounds = roomObject.GetComponent<BoxCollider>().bounds;
            roomBox = new RoomBox() { 
                RoomBounds = roomBounds, 
                XLength = roomBounds.size.x, 
                YLength = roomBounds.size.y, 
                ZLength = roomBounds.size.z, 
                BoxOriginPoint = roomBounds.min,

            };

            CreateVoxelArray();

            gazeCursorObject = Instantiate(gazeCursorPrefab);
            gazeCursorObject.SetActive(false);
        }

        private void Update()
        {
            if (switchGazeMode.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                isGazeVoxelization = !isGazeVoxelization;
                if (isGazeVoxelization) { Debug.Log("进入凝视点捕获模式..."); }
                else { Debug.Log("凝视点捕获模式关闭..."); }
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            // Debug.Log("凝视点向量:"+sRanipalGazeSample.GazeDirectionCombined.ToString());
            
            if (isGazeVoxelization)
            {
                if (gazeCursorObject.activeSelf == false) { gazeCursorObject.SetActive(true); }
                UpdateVoxelByGazePoint(sRanipalGazeSample.GazeDirectionCombined);
            }
            if (!isGazeVoxelization)
            {
                if (gazeCursorObject.activeSelf == true) { gazeCursorObject.SetActive(false); }
            }
            
        }

        /// <summary>
        /// 创建体素阵列
        /// </summary>
        private void CreateVoxelArray()
        {
            // 计算三个轴上体素数量，向上取整
            xVoxellNumber = Mathf.CeilToInt(roomBox.XLength / voxcelSideLength); 
            yVoxellNumber = Mathf.CeilToInt(roomBox.YLength / voxcelSideLength);
            zVoxellNumber = Mathf.CeilToInt(roomBox.ZLength / voxcelSideLength);

            // 创建体素阵列父物体
            voxcelParent = new GameObject("体素阵列父物体");

            // 创建体素阵列
            voxelArray = new List<List<List<Voxel>>>();
            for (int ix = 0; ix < xVoxellNumber; ix++)
            {
                List<List<Voxel>> tempXList = new List<List<Voxel>>();
                for (int iy = 0; iy < yVoxellNumber; iy++)
                {
                    List<Voxel> tempYList = new List<Voxel>();
                    for (int iz = 0; iz < zVoxellNumber; iz++)
                    {
                        GameObject newVoxelObject = Instantiate(voxelPrefab, voxcelParent.transform);
                        newVoxelObject.name = string.Format("voxcel_{0}{1}{2}", ix, iy, iz);
                        newVoxelObject.isStatic = true;
                        // 设置体素位置
                        newVoxelObject.transform.position = roomBox.BoxOriginPoint + ix * Vector3.right * voxcelSideLength + iy * Vector3.up * voxcelSideLength + iz * Vector3.forward * voxcelSideLength;

                        // 暂时不需要材质设置
                        Voxel newVoxcelStruct = new Voxel() { voxelObject = newVoxelObject, gazeTimes = 0 };
                        tempYList.Add(newVoxcelStruct);
                    }
                    tempXList.Add(tempYList);
                }
                voxelArray.Add(tempXList);
            }
        }


        private void UpdateVoxelByGazePoint(Vector3 gazeDirect)
        {
            // 获取起始点
            Vector3 eyePosition = Camera.main.transform.position - Camera.main.transform.up * 0.05f;
            // 获取凝视点与场景的碰撞点
            Ray gazeRay = new Ray(eyePosition, gazeDirect);
            RaycastHit hitInfo;
            if(Physics.Raycast(gazeRay, out hitInfo))
            {
                Debug.DrawRay(eyePosition, gazeDirect, Color.green);
                Vector3 gazePoint = hitInfo.point;
                int xVoxelIndex = Mathf.FloorToInt((gazePoint.x - roomBox.BoxOriginPoint.x) / voxcelSideLength - 0.5f) + 1;
                int yVoxelIndex = Mathf.FloorToInt((gazePoint.y - roomBox.BoxOriginPoint.y) / voxcelSideLength - 0.5f) + 1;
                int zVoxelIndex = Mathf.FloorToInt((gazePoint.z - roomBox.BoxOriginPoint.z) / voxcelSideLength - 0.5f) + 1;
                
                Voxel tempVoxel = voxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex];

                if (tempVoxel.gazeTimes == 0)
                {
                    // 该像素块第一次被选中，为其定制脚本
                    Material newMaterial = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"))
                    {
                        color = new Color(168f/255f, 15f/255f, 15f/255f, 0f)
                    };
                    tempVoxel.voxelObject.GetComponent<MeshRenderer>().material = newMaterial;

                }
                tempVoxel.gazeTimes += 1;
                //Material voxelMaterial = tempVoxel.voxelObject.GetComponent<MeshRenderer>().material;
                //voxelMaterial.color = new Color(168, 15, 15, GazeTimes2Alpha(tempVoxel.gazeTimes));
                int alpha = GazeTimes2Alpha(tempVoxel.gazeTimes,50f);
                // print("alpha:" + alpha);
                tempVoxel.voxelObject.GetComponent<MeshRenderer>().material.SetColor("_Color",new Color(168f / 255f, 15f / 255f, 15f / 255f, alpha/255f));
                // print("material:" + tempVoxel.voxelObject.GetComponent<MeshRenderer>().material.color.ToString());

                voxelArray[xVoxelIndex][yVoxelIndex][zVoxelIndex] = tempVoxel;

                // 绘制
                gazeCursorObject.transform.position = hitInfo.point;
                // Debug.Log("碰撞点法向量:" + hitInfo.normal.ToString());
                // gazeCursorObject.transform.eulerAngles = hitInfo.normal;
            }

        }

        private int GazeTimes2Alpha(int gazeTimes,float magnification)
        {
            float angles = Mathf.Atan(gazeTimes/magnification);
            int alpha = Mathf.FloorToInt(angles * 512 / Mathf.PI);

            return  alpha>=255?255:alpha;
        }
    }
}


