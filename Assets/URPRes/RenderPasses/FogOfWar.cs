using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FogOfWar : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;

        /// <summary>
        /// 战争迷雾颜色
        /// </summary>
        [Tooltip("战争迷雾颜色")] public Color FogOfWarColor;

        /// <summary>
        /// 战争迷雾材质
        /// </summary>
        [Tooltip("战争迷雾材质")] public Material FogOfWarMaterial = null;

        /// <summary>
        /// 战争迷雾模糊材质
        /// </summary>
        [Tooltip("战争迷雾模糊材质")] public Material BlurMaterial = null;

        /// <summary>
        /// 战争迷雾滤波模式
        /// </summary>
        [Tooltip("战争迷雾滤波模式")] public FilterMode FliterMode = FilterMode.Bilinear;

        /// <summary>
        /// 战争迷雾模糊偏移量
        /// </summary>
        [Tooltip("战争迷雾模糊偏移量")] public float BlurOffset;

        /// <summary>
        /// 战争迷雾模糊次数
        /// </summary>
        [Tooltip("战争迷雾模糊次数")] public int BlurInteration;
    }

    public Settings settings = new Settings();

    FogOfWarPass fogOfWarPass;

    public override void Create()
    {
        this.settings.BlurMaterial.SetFloat("_Offset", this.settings.BlurOffset);
        this.settings.FogOfWarMaterial.SetColor("_FogColor", this.settings.FogOfWarColor);
        fogOfWarPass = new FogOfWarPass(settings.Event, name,
            settings.FogOfWarMaterial, settings.BlurMaterial, settings.FliterMode, settings.BlurInteration);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var src = renderer.cameraColorTarget;
        var dest = renderer.cameraColorTarget;

        fogOfWarPass.Setup(src, dest);
        renderer.EnqueuePass(fogOfWarPass);
    }
}