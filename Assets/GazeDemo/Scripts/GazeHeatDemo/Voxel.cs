using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GazeHeat
{
    public enum VoxelStatus {DEAD=0,ALIVE_STABEL,ALIVE_LOSSBLOOD}

    public class Voxel : MonoBehaviour
    {
        [HideInInspector] public GameObject voxelObject;
        [HideInInspector] public int gazeTimes;
        [HideInInspector] public VoxelStatus status;
        public int serialNumberX;
        public int serialNumberY;
        public int serialNumberZ;
        public float stableLifeTime; // ��������,��RoomManagerment�г�ʼ��
        public float currenLifeTime; // ��ǰʣ������
        public float magnification; // ���ӵ��ȶ������ٶȣ���ֵԽ������Խ��

        // ���ز���
        private Material voxelMaterial;
        

        // Start is called before the first frame update
        public void Start()
        {
            magnification = 10f;
            voxelObject = this.gameObject;
            gazeTimes = 0;
            status = VoxelStatus.DEAD;
            CreateVoxelMaterial();
            
            //Debug.Log("���س�ʼ�����");
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void FixedUpdate()
        {
            DecreaseStableLifeTime();
            DecreaseGazeTimesLife();
        }

        /// <summary>
        /// �����ӵ���ʱ�����
        /// </summary>
        public void BeGazedAt()
        {
            //Debug.Log("���ر�����");
            if (status.Equals(VoxelStatus.DEAD)||status.Equals(VoxelStatus.ALIVE_LOSSBLOOD))
            {
                status = VoxelStatus.ALIVE_STABEL;
                currenLifeTime = stableLifeTime;
            }
            gazeTimes++;
            UpdateVoxelMaterial();
        }

        public void DecreaseStableLifeTime()
        {
            if (status.Equals(VoxelStatus.ALIVE_STABEL))
            {
                // �ȶ����ʱ����
                currenLifeTime -= Time.fixedDeltaTime;
                if (currenLifeTime<0)
                {
                    //Debug.Log("�����ȶ�̬����");
                    status = VoxelStatus.ALIVE_LOSSBLOOD;
                    // ��ԭʼ�ķ�����ֱ�������ص������ȶȴ�������ֵ
                    currenLifeTime = gazeTimes;
                    // һ�ָ�����ת��Ϊ�ȶ�ֵ[0,255]
                }
            }
        }

        public void DecreaseGazeTimesLife()
        {
            if (status.Equals(VoxelStatus.ALIVE_LOSSBLOOD))
            {
                currenLifeTime -= 10*Time.fixedDeltaTime;
                gazeTimes = Mathf.Max(Mathf.FloorToInt(currenLifeTime),0);
                UpdateVoxelMaterial();
                if (currenLifeTime < 0)
                {
                    //Debug.Log("��������");
                    status = VoxelStatus.DEAD;
                    voxelObject.SetActive(false);
                }
            }
        }

        public int GazeTimes2Alpha(int gazeTimes, float magnification)
        {
            float angles = Mathf.Atan(gazeTimes / magnification);
            int alpha = Mathf.FloorToInt(angles * 512 / Mathf.PI);

            return alpha >= 255 ? 255 : alpha;
        }

        public void CreateVoxelMaterial()
        {
            GetComponent<MeshRenderer>().material = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"))
            {
                color = new Color(168f / 255f, 15f / 255f, 15f / 255f, 0f)
            };
            voxelMaterial = GetComponent<MeshRenderer>().material;
        }

        public void UpdateVoxelMaterial()
        {
            int alpha = GazeTimes2Alpha(this.gazeTimes, magnification);
            voxelMaterial.SetColor("_Color", new Color(168f / 255f, 15f / 255f, 15f / 255f, alpha / 255f));
        }
    }
}

