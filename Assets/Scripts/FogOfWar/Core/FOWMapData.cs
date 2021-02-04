using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASL.FogOfWar
{
    /// <summary>
    /// FOW地图数据接口
    /// </summary>
    public interface IFOWMapData
    {
        /// <summary>
        /// 地图数据是否为预生成
        /// </summary>
        bool isPregeneration { get; }

        /// <summary>
        /// 生成地图数据
        /// </summary>
        /// <param name="beginx"></param>
        /// <param name="beginy"></param>
        /// <param name="deltax"></param>
        /// <param name="deltay"></param>
        /// <param name="heightRange"></param>
        void GenerateMapData(float beginx, float beginy, float deltax, float deltay, float heightRange);

        /// <summary>
        /// 获取地图目标格数据，结果为false说明为障碍，结果为true说明不为障碍
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        bool this[int i, int j] { get; }
    }
    
    public class FOWMapData : IFOWMapData
    {
        public bool isPregeneration
        {
            get { return false; }
        }

        public int width
        {
            get { return m_Width; }
        }

        public int height
        {
            get { return m_Height; }
        }

        public bool this[int i, int j]
        {
            get
            {
                return m_MapData[i, j]; 
                
            }
        }

        private bool[,] m_MapData;

        private int m_Width;
        private int m_Height;

        public FOWMapData(int width, int height)
        {
            m_MapData = new bool[width, height];
            m_Width = width;
            m_Height = height;
        }

        public void GenerateMapData(float beginx, float beginy, float deltax, float deltay, float heightRange)
        {
            for (int i = 0; i < m_Width; i++)
            {
                for (int j = 0; j < m_Height; j++)
                {
                    m_MapData[i, j] = FOWUtils.IsObstacle(beginx, beginy, deltax, deltay, heightRange, i, j);
                }
            }
        }
    }
}