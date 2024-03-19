using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XCharts.Runtime;

namespace GazeHeat{
    public struct GainData
    {
        public float gainValue;
        public float gainIndex;
    }

    public class ChartManagement : MonoBehaviour
    {
        public GameObject TranslateChartObject;
        [HideInInspector] public LineChart translateChart;
        [HideInInspector] public Line translateLine;
        private Queue<GainData> translateDataQueue;
        public int MaxTranslateDataNumber;

        // 测试用参数
        private float currentTime;
        private int indexCount;


        // Start is called before the first frame update
        void Start()
        {
            InitialTranslateChart();

            translateDataQueue = new Queue<GainData>();

            // 测试用参数
            currentTime = 0f;
            indexCount = 0;
        }

        // Update is called once per frame
        void Update()
        {

        }

        void FixedUpdate()
        {
            currentTime += Time.fixedDeltaTime;
            if (currentTime > 0.5f)
            {
                currentTime = 0f;
                indexCount++;
                UpdateTranslateChart(indexCount * 0.5f, Random.Range(0.1f, 1f));
                    
            }
            
            
        }

        public void InitialTranslateChart()
        {
            translateChart = TranslateChartObject.GetComponent<LineChart>();
            // 设置图标标题
            var title = translateChart.GetOrAddChartComponent<Title>();
            title.text = "Translate Gain";

            // 设置坐标轴
            var xAxis = translateChart.GetOrAddChartComponent<XAxis>();
            xAxis.type = Axis.AxisType.Value;
            xAxis.axisName.name = "time";
            xAxis.axisName.show = true;
            //xAxis.interval = 1d;


            var yAxis = translateChart.GetOrAddChartComponent<YAxis>();
            yAxis.type = Axis.AxisType.Value;
            yAxis.axisName.name = "gain";
            yAxis.axisName.labelStyle.offset = new Vector3(0, 20, 0);
            yAxis.axisName.show = true;

            //清空数据
            translateChart.RemoveData();
            translateLine = translateChart.AddSerie<Line>("translate gain");
            translateLine.showDataDimension = 2;
        }

        public void UpdateTranslateChart(float xIndex,float yGain)
        {
            translateLine.ClearData();

            GainData newGainData = new GainData() { gainValue = yGain,gainIndex = xIndex};

            if (translateDataQueue.Count< MaxTranslateDataNumber)
            {
                translateDataQueue.Enqueue(newGainData);
            }
            else
            {
                translateDataQueue.Dequeue();
                translateDataQueue.Enqueue(newGainData);
            }

            
            foreach(var item in translateDataQueue)
            {
                SerieData serieData = new SerieData();
                serieData.data.Add(item.gainIndex);
                serieData.data.Add(item.gainValue);
                translateLine.AddSerieData(serieData);
            }


        }
    }
}

