using ASL.FogOfWar;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Copy the given color buffer to the given destination color buffer.
///
/// You can use this pass to copy a color buffer to the destination,
/// so you can use it later in rendering. For example, you can copy
/// the opaque texture to use it for distortion effects.
/// </summary>
class FogOfWarPass : ScriptableRenderPass
{
    private RenderTargetIdentifier source { get; set; }
    private RenderTargetIdentifier destination { get; set; }

    RenderTargetHandle m_TempColorTextureForBlurFOW;

    RenderTargetHandle m_TempColorTextureForBlitCameraColor;

    RenderTargetHandle m_FowTexture;

    int Internal_WorldToProjector = Shader.PropertyToID("internal_WorldToProjector");

    string m_ProfilerTag;

    /// <summary>
    /// 战争迷雾材质
    /// </summary>
    private Material m_FogOfWarMaterial = null;

    /// <summary>
    /// 战争迷雾模糊材质
    /// </summary>
    private Material m_BlurMaterial = null;

    /// <summary>
    /// 战争迷雾滤波模式
    /// </summary>
    private FilterMode m_FilterMode;

    /// <summary>
    /// 战争迷雾模糊次数
    /// </summary>
    private int m_BlurInteration;

    /// <summary>
    /// Create the CopyColorPass
    /// </summary>
    public FogOfWarPass(RenderPassEvent renderPassEvent, string tag,
        Material fowEffect,
        Material fowBlur, FilterMode filterMode, int fowBlurCount)
    {
        this.renderPassEvent = renderPassEvent;
        m_ProfilerTag = tag;

        m_TempColorTextureForBlurFOW.Init("_TempColorTextureForBlurFOW");
        m_TempColorTextureForBlitCameraColor.Init("_TempColorTextureForBlitCameraColor");
        m_FowTexture.Init("_FogTex");

        this.m_FogOfWarMaterial = fowEffect;
        this.m_BlurMaterial = fowBlur;
        this.m_FilterMode = filterMode;
        this.m_BlurInteration = fowBlurCount;
    }

    /// <summary>
    /// Configure the pass with the source and destination to execute on.
    /// </summary>
    /// <param name="source">Source Render Target</param>
    /// <param name="destination">Destination Render Target</param>
    public void Setup(RenderTargetIdentifier source, RenderTargetIdentifier destination)
    {
        this.source = source;
        this.destination = destination;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (FogOfWarEffect.Instance == null || FogOfWarEffect.Instance.m_Map == null) return;
        //先获取FOW的纹理
        Texture2D fowTexture2D = FogOfWarEffect.Instance.m_Map.GetFOWTexture();
        if (fowTexture2D == null) return;


        CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

        CaculateWorldPos(renderingData.cameraData.camera, cmd);

        cmd.SetGlobalMatrix(Internal_WorldToProjector,
            FogOfWarEffect.Instance.m_Renderer.m_WorldToProjector);

        cmd.GetTemporaryRT(m_FowTexture.id, fowTexture2D.width, fowTexture2D.height, 0, m_FilterMode);

        //将FOW纹理Blit到RenderTexture
        cmd.Blit(fowTexture2D, m_FowTexture.id, m_BlurMaterial);
        //申请模糊纹理用的RT，注意使用双线性滤波或三线性滤波，否则模糊效果将极不明显
        cmd.GetTemporaryRT(m_TempColorTextureForBlurFOW.id, fowTexture2D.width / 2, fowTexture2D.height / 2, 0,
            m_FilterMode);

        for (int i = 0; i < m_BlurInteration; i++)
        {
            //注意写回纹素
            Blit(cmd, m_FowTexture.id, m_TempColorTextureForBlurFOW.id, m_BlurMaterial);
            Blit(cmd, m_TempColorTextureForBlurFOW.id, m_FowTexture.id);
        }

        cmd.SetGlobalTexture(m_FowTexture.id, m_FowTexture.id);

        //申请Blit cameraColorTexture用的RT
        cmd.GetTemporaryRT(m_TempColorTextureForBlitCameraColor.id, renderingData.cameraData.camera.scaledPixelWidth,
            renderingData.cameraData.camera.scaledPixelHeight);

        Blit(cmd, this.source, this.m_TempColorTextureForBlitCameraColor.id, m_FogOfWarMaterial);
        Blit(cmd, this.m_TempColorTextureForBlitCameraColor.id, this.source);

        cmd.ReleaseTemporaryRT(m_TempColorTextureForBlitCameraColor.id);
        cmd.ReleaseTemporaryRT(m_TempColorTextureForBlurFOW.id);
        cmd.ReleaseTemporaryRT(m_FowTexture.id);

        //执行命令缓冲区命令
        context.ExecuteCommandBuffer(cmd);

        CommandBufferPool.Release(cmd);
    }

    private void CaculateWorldPos(Camera camera, CommandBuffer commandBuffer)
    {
        Matrix4x4 frustumCorners = Matrix4x4.identity;
        Transform cameraTransform = camera.transform;
        
        float fov = camera.fieldOfView;
        float near = camera.nearClipPlane;
        float aspect = camera.aspect;

        float halfHeight = near * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        Vector3 toRight = cameraTransform.right * halfHeight * aspect;
        Vector3 toTop = cameraTransform.up * halfHeight;

        Vector3 topLeft = cameraTransform.forward * near + toTop - toRight;
        float scale = topLeft.magnitude / near;

        topLeft.Normalize();
        topLeft *= scale;

        Vector3 topRight = cameraTransform.forward * near + toRight + toTop;
        topRight.Normalize();
        topRight *= scale;

        Vector3 bottomLeft = cameraTransform.forward * near - toTop - toRight;
        bottomLeft.Normalize();
        bottomLeft *= scale;

        Vector3 bottomRight = cameraTransform.forward * near + toRight - toTop;
        bottomRight.Normalize();
        bottomRight *= scale;

        //计算近裁剪平面四个角对应向量，并存储在一个矩阵类型的变量中
        frustumCorners.SetRow(0, bottomLeft);
        frustumCorners.SetRow(1, bottomRight);
        frustumCorners.SetRow(2, topRight);
        frustumCorners.SetRow(3, topLeft);
        

        commandBuffer.SetGlobalMatrix("_FrustumCornersRay", frustumCorners);
    }
}