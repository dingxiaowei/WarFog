using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASL.FogOfWar
{
    /// <summary>
    /// 战争迷雾屏幕特效渲染器
    /// </summary>
    public class FOWRenderer
    {
        private Material m_FOWEffectMaterial;

        private Material m_BlurMaterial;

        private Material m_MiniMapMaterial;

        /// <summary>
        /// 世界空间到迷雾投影空间矩阵
        /// </summary>
        public Matrix4x4 m_WorldToProjector;

        private bool m_GenerateMimiMapMask;

        private RenderTexture m_MiniMapMask;


        public FOWRenderer(Material fowEffectMaterial, Shader miniMapRenderShader, Vector3 position, float xSize,
            float zSize)
        {
            m_FOWEffectMaterial = fowEffectMaterial;
            //这里是以战争迷雾中心点作为这个矩阵变换的坐标原点，即使用这个矩阵做映射会得到相对于迷雾中心的坐标值
            m_WorldToProjector = default(Matrix4x4);
            m_WorldToProjector.m00 = 1.0f/xSize;
            m_WorldToProjector.m03 = -1.0f/xSize*position.x + 0.5f;
            m_WorldToProjector.m12 = 1.0f/zSize;
            m_WorldToProjector.m13 = -1.0f/zSize*position.z + 0.5f;
            m_WorldToProjector.m33 = 1.0f;

            // if (miniMapRenderShader)
            // {
            //     m_MiniMapMaterial = new Material(miniMapRenderShader);
            //     m_MiniMapMaterial.SetColor("_FogColor", fogColor);
            // }
            // m_BlurInteration = blurInteration;
        }

        /// <summary>
        /// 渲染战争迷雾
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public void RenderFogOfWar(Camera camera, Texture2D fogTexture, RenderTexture src, RenderTexture dst)
        {
            //  if (m_BlurMaterial && fogTexture)
            //  {
            //      RenderTexture rt = RenderTexture.GetTemporary(fogTexture.width, fogTexture.height, 0);
            //      Graphics.Blit(fogTexture, rt, m_BlurMaterial);
            //      for (int i = 0; i <= m_BlurInteration; i++)
            //      {
            //          RenderTexture rt2 = RenderTexture.GetTemporary(fogTexture.width / 2, fogTexture.height / 2, 0);
            //          Graphics.Blit(rt, rt2, m_BlurMaterial);
            //          RenderTexture.ReleaseTemporary(rt);
            //          rt = rt2;
            //      }
            //      if (m_MiniMapMaterial)
            //          RenderToMiniMapMask(rt);
            //      m_EffectMaterial.SetTexture("_FogTex", rt);
            //      CustomGraphicsBlit(src, dst, m_EffectMaterial);
            //      RenderTexture.ReleaseTemporary(rt);
            //  }
            //  else
            //  {
            //      if (m_MiniMapMaterial)
            //          RenderToMiniMapMask(fogTexture);
            //      m_EffectMaterial.SetTexture("_FogTex", fogTexture);
            //      CustomGraphicsBlit(src, dst, m_EffectMaterial);
            //  }
            // // if (m_GenerateMimiMapMask)
            //  //    RenderToMiniMapMask(dst);
        }

        public RenderTexture GetMimiMapMask()
        {
            return m_MiniMapMask;
        }

        /// <summary>
        /// 设置当前迷雾和上一次更新的迷雾的插值
        /// </summary>
        /// <param name="fade"></param>
        public void SetFogFade(float fade)
        {
            m_FOWEffectMaterial.SetFloat("_MixValue", fade);
            if (m_MiniMapMaterial)
                m_MiniMapMaterial.SetFloat("_MixValue", fade);
        }

        public void Release()
        {
        }

        /// <summary>
        /// TODO 小地图迷雾可以直接在URP Pass里加一个Blit
        /// </summary>
        /// <param name="from"></param>
        private void RenderToMiniMapMask(Texture from)
        {
            if (m_MiniMapMask == null)
            {
                m_MiniMapMask = new RenderTexture(512, 512, 0);
            }

            Graphics.Blit(from, m_MiniMapMask, m_MiniMapMaterial);
        }
    }
}