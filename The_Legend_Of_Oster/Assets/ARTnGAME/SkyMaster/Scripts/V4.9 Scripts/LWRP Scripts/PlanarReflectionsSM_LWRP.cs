using UnityEngine;
//using Unity.Mathematics;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

//using UnityEngine.Experimental.Rendering.LWRP;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]//[ImageEffectAllowedInSceneView] //[ExecuteAlways]
public class PlanarReflectionsSM_LWRP : MonoBehaviour//, IBeforeCameraRender
    {

    public bool invertCulling = true; //v0.1 

        [System.Serializable]
        public enum ResolutionMulltiplier
        {
            Full,
            Half,
            Third,
            Quarter
        }
        
        [System.Serializable]
        public class PlanarReflectionSettings
        {
            public ResolutionMulltiplier m_ResolutionMultiplier = ResolutionMulltiplier.Third;
            public float m_ClipPlaneOffset = 0.07f;
            public LayerMask m_ReflectLayers = -1;
        }

        
        [SerializeField]
        public PlanarReflectionSettings m_settings = new PlanarReflectionSettings();

        public GameObject target;
        [FormerlySerializedAs("camOffset")] public float m_planeOffset;

        private static Camera m_ReflectionCamera;
        private Vector2 m_TextureSize = new Vector2(256, 128);
        private RenderTexture m_ReflectionTexture = null;
        //private int planarReflectionTextureID = Shader.PropertyToID("_PlanarReflectionTexture");
        private int planarReflectionTextureID;
        public string refractionTextureName = "_PlanarReflectionTexture";

        public Camera outReflectionCamera; //v1.6

        private Vector2 m_OldReflectionTextureSize;

        // Cleanup all the objects we possibly have created
        private void OnDisable()
        {
            Cleanup();
        }

        private void OnDestroy()
        {

            prevRenderer = -1;

            Cleanup();
        }
    public int rendererID = 1;
    int prevRenderer = -1;
    //NEW
    //sample code 
    //https://github.com/0lento/OcclusionProbes/blob/package/OcclusionProbes/OcclusionProbes.cs#L41
    private void OnEnable()
    {
        if (m_ReflectionCamera && prevRenderer != rendererID)
        {        
            UniversalAdditionalCameraData thisCameraData = m_ReflectionCamera.gameObject.GetComponent<UniversalAdditionalCameraData>();
            thisCameraData.SetRenderer(rendererID);
            prevRenderer = rendererID;
        }

        RenderPipelineManager.beginCameraRendering += DoReflections;
        //RenderPipelineManager.endCameraRendering += DoReflections;
        planarReflectionTextureID = Shader.PropertyToID(refractionTextureName);
    }
    //END NEW

    void Cleanup()
    {
        if (m_ReflectionCamera)
        {
            m_ReflectionCamera.targetTexture = null;
            SafeDestroy(m_ReflectionCamera.gameObject);
        }
        if (m_ReflectionTexture)
        {
            RenderTexture.ReleaseTemporary(m_ReflectionTexture);
        }

        //new
        RenderPipelineManager.beginCameraRendering -= DoReflections;
        //RenderPipelineManager.endCameraRendering += DoReflections;

        prevRenderer = -1;
    }

        void SafeDestroy(Object obj)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(obj);
            }
            else
            {
                Destroy(obj);
            }
        }
        
        private void UpdateCamera(Camera src, Camera dest)
        {
            if (dest == null)
                return;
            dest.CopyFrom(src);
        }
        
        private void UpdateReflectionCamera(Camera realCamera)
        {

            

            planarReflectionTextureID = Shader.PropertyToID(refractionTextureName); //////////// reset name if changed

            if (m_ReflectionCamera == null)
            {
                m_ReflectionCamera = CreateMirrorObjects(realCamera);
            }


        if (m_ReflectionCamera && prevRenderer != rendererID)
        {
            UniversalAdditionalCameraData thisCameraData = m_ReflectionCamera.gameObject.GetComponent<UniversalAdditionalCameraData>();
            thisCameraData.SetRenderer(rendererID);
            prevRenderer = rendererID;
        }

        

        // find out the reflection plane: position and normal in world space
        Vector3 pos = Vector3.zero;
            Vector3 normal = Vector3.up;
            if (target != null)
            {
                pos = target.transform.position + Vector3.up * m_planeOffset;
                normal = target.transform.up;
            }

            UpdateCamera(realCamera, m_ReflectionCamera);
            
            // Render reflection
            // Reflect camera around reflection plane
            float d = -Vector3.Dot(normal, pos) - m_settings.m_ClipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            Matrix4x4 reflection = Matrix4x4.identity;
            reflection *= Matrix4x4.Scale(new Vector3(1, -1, 1));

            CalculateReflectionMatrix(ref reflection, reflectionPlane);
            Vector3 oldpos = realCamera.transform.position - new Vector3(0, pos.y * 2, 0);
            Vector3 newpos = ReflectPosition(oldpos);
            m_ReflectionCamera.transform.forward = Vector3.Scale(realCamera.transform.forward, new Vector3(1, -1, 1));
            m_ReflectionCamera.worldToCameraMatrix = realCamera.worldToCameraMatrix * reflection;

            // Setup oblique projection matrix so that near plane is our reflection
            // plane. This way we clip everything below/above it for free.
            Vector4 clipPlane = CameraSpacePlane(m_ReflectionCamera, pos - Vector3.up * 0.1f, normal, 1.0f);
            Matrix4x4 projection = realCamera.CalculateObliqueMatrix(clipPlane);
            m_ReflectionCamera.projectionMatrix = projection;
            m_ReflectionCamera.cullingMask = m_settings.m_ReflectLayers; // never render water layer
            m_ReflectionCamera.transform.position = newpos;
        }
        
        // Calculates reflection matrix around the given plane
        private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }

        private static Vector3 ReflectPosition(Vector3 pos)
        {
            Vector3 newPos = new Vector3(pos.x, -pos.y, pos.z);
            return newPos;
        }

        private float GetScaleValue()
        {
            switch(m_settings.m_ResolutionMultiplier)
            {
                case ResolutionMulltiplier.Full:
                    return 1f;
                case ResolutionMulltiplier.Half:
                    return 0.5f;
                case ResolutionMulltiplier.Third:
                    return 0.33f;
                case ResolutionMulltiplier.Quarter:
                    return 0.25f;
            }
            return 0.5f; // default to half res
        }
        
        // Compare two int2
        //private static bool Int2Compare(int2 a, int2 b)
        //{
        //    if(a.x == b.x && a.y == b.y)
        //        return true;
        //    else
        //        return false;
        //}

        // Given position/normal of the plane, calculates plane in camera space.
        private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * m_settings.m_ClipPlaneOffset;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        private Camera CreateMirrorObjects(Camera currentCamera)
        {
            GameObject go =
                new GameObject("Planar Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(),
                    typeof(Camera));
            UnityEngine.Rendering.Universal.UniversalAdditionalCameraData lwrpCamData =
                go.AddComponent(typeof(UnityEngine.Rendering.Universal.UniversalAdditionalCameraData)) as UnityEngine.Rendering.Universal.UniversalAdditionalCameraData;
            lwrpCamData.renderShadows = false; // turn off shadows for the reflection camera
            lwrpCamData.requiresColorOption = UnityEngine.Rendering.Universal.CameraOverrideOption.Off;
        //lwrpCamData.requiresDepthOption = UnityEngine.Rendering.Universal.CameraOverrideOption.Off;

        lwrpCamData.requiresDepthOption = UnityEngine.Rendering.Universal.CameraOverrideOption.On;/////////////////// NEW

        var reflectionCamera = go.GetComponent<Camera>();
            reflectionCamera.transform.SetPositionAndRotation(transform.position, transform.rotation);
            //reflectionCamera.targetTexture = m_ReflectionTexture;
            reflectionCamera.allowMSAA = currentCamera.allowMSAA;
            reflectionCamera.depth = -10;
            reflectionCamera.enabled = false;
            reflectionCamera.allowHDR = false;// currentCamera.allowHDR;
            go.hideFlags = HideFlags.DontSave;

        reflectionCamera.depthTextureMode = DepthTextureMode.Depth;/////////////////// NEW

            return reflectionCamera;
        }

        private Vector2 ReflectionResolution(Camera cam, float scale)
        {
            var x = (int)(cam.pixelWidth * scale * GetScaleValue());
            var y = (int)(cam.pixelHeight * scale * GetScaleValue());
            return new Vector2(x, y);
        }

    // public void ExecuteBeforeCameraRender(
    //     LightweightRenderPipeline pipelineInstance,
    //     ScriptableRenderContext context,
    //     Camera camera)

        //NEW
    public void DoReflections( ScriptableRenderContext context, Camera camera)
    {
            if (!enabled)
                return;

        //v0.1
        if (m_ReflectionCamera && prevRenderer != rendererID)
        {
            UniversalAdditionalCameraData thisCameraData = m_ReflectionCamera.gameObject.GetComponent<UniversalAdditionalCameraData>();
            thisCameraData.SetRenderer(rendererID);
            prevRenderer = rendererID;
        }

        if (m_ReflectionCamera != null && outReflectionCamera == null)
        {
            outReflectionCamera = m_ReflectionCamera;//v1.6
        }

        GL.invertCulling = invertCulling;// true; v0.1
            //RenderSettings.fog = false;
            var max = QualitySettings.maximumLODLevel;
            var bias = QualitySettings.lodBias;
            QualitySettings.maximumLODLevel = 1;
            QualitySettings.lodBias = bias * 0.5f;
            
            UpdateReflectionCamera(camera);

            var res = ReflectionResolution(camera, 1);
            if (m_ReflectionTexture == null)
            {
            m_ReflectionTexture = RenderTexture.GetTemporary((int)res.x, (int)res.y, 16, RenderTextureFormat.DefaultHDR);// RenderTextureFormat.DefaultHDR);///////
            }
                        
            m_ReflectionCamera.targetTexture = m_ReflectionTexture;

        //new
        //LightweightRenderPipeline.RenderSingleCamera(pipelineInstance, context, m_ReflectionCamera);
        UnityEngine.Rendering.Universal.UniversalRenderPipeline.RenderSingleCamera( context, m_ReflectionCamera);

        GL.invertCulling = false;
            //RenderSettings.fog = true;
            QualitySettings.maximumLODLevel = max;
            QualitySettings.lodBias = bias;
            Shader.SetGlobalTexture(planarReflectionTextureID, m_ReflectionTexture);
        }
    }
