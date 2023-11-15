using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DepthTextureGenerator : MonoBehaviour
{
    public Shader depthTextureShader; //用来生成mipmap的shader
    public RenderTexture temp;

    RenderTexture m_depthTexture; //带mipmap的深度图
    public RenderTexture depthRexture => m_depthTexture;

    Material m_depthTextureMaterial;

    //深度取值范围0-1，单通道即可
    const RenderTextureFormat m_depthTextureFormat = RenderTextureFormat.RHalf;

    int m_depthTextureShaderID;
    int m_uvSizePerPixelShaderID;

    // Start is called before the first frame update
    void Start()
    {
        m_depthTextureMaterial = new Material(depthTextureShader);
        Camera.main.depthTextureMode |= DepthTextureMode.Depth;

        m_depthTextureShaderID = Shader.PropertyToID("_DepthTexture");
        m_uvSizePerPixelShaderID = Shader.PropertyToID("_UVSizePerPixel");

        InitDepthTexture();
    }

    void InitDepthTexture() {
        if (m_depthTexture != null) {
            return;
        }
        m_depthTexture = new RenderTexture(1024, 1024, 0, m_depthTextureFormat);
        m_depthTexture.autoGenerateMips = false;
        m_depthTexture.useMipMap = true;
        m_depthTexture.filterMode = FilterMode.Point;
        m_depthTexture.Create();
    }

    // Update is called once per frame
    void Update()
    {
        int w = m_depthTexture.width;
        int h = m_depthTexture.height;
        int mipmapLevel = 0;

        RenderTexture currentRenderTexture = null;
        RenderTexture preRenderTexture = null;

        //如果当前mipmap宽高>8,则计算下一层的mipmap
        while (h > 8) {
            m_depthTextureMaterial.SetVector(m_uvSizePerPixelShaderID, new Vector4(1.0f / w, 1.0f / h, 0, 0));

            currentRenderTexture = RenderTexture.GetTemporary(w, h, 0, m_depthTextureFormat);

            currentRenderTexture.filterMode = FilterMode.Point;

            if (preRenderTexture == null)
            {
                Graphics.Blit(Shader.GetGlobalTexture("_CameraDepthTexture"), currentRenderTexture);
            }
            else {
                m_depthTextureMaterial.SetTexture(m_depthTextureShaderID, preRenderTexture);
                Graphics.Blit(null, currentRenderTexture, m_depthTextureMaterial);
                RenderTexture.ReleaseTemporary(preRenderTexture);
            }
            Graphics.CopyTexture(currentRenderTexture, 0, 0, m_depthTexture, 0, mipmapLevel);
            preRenderTexture = currentRenderTexture;

            w /= 2;
            h /= 2;
            mipmapLevel++;
        }
        RenderTexture.ReleaseTemporary(preRenderTexture);
        GameObject.Find("RawImage").GetComponent<UnityEngine.UI.RawImage>().texture = Shader.GetGlobalTexture("_CameraDepthTexture");
    }

    void OnDestroy()
    {
        m_depthTexture.Release();
        Destroy(m_depthTexture);
    }
}
