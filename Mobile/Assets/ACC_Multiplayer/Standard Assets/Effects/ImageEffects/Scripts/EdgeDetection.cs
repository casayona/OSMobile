using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent (typeof (Camera))]
    [AddComponentMenu ("Image Effects/Edge Detection/Edge Detection")]
    public class EdgeDetection : PostEffectsBase
    {
        public enum EdgeDetectMode
        {
            TriangleDepthNormals = 0,
            RobertsCrossDepthNormals = 1,
            SobelDepth = 2,
            SobelDepthThin = 3,
            TriangleLuminance = 4,
        }


        public EdgeDetectMode mode = EdgeDetectMode.SobelDepthThin;
        public float sensitivityDepth = 1.0f;
        public float sensitivityNormals = 1.0f;
        public float lumThreshold = 0.2f;
        public float edgeExp = 1.0f;
        public float sampleDist = 1.0f;
        public float edgesOnly = 0.0f;
        public Color edgesOnlyBgColor = Color.white;

        public Shader edgeDetectShader;
        private Material edgeDetectMaterial = null;
        private EdgeDetectMode oldMode = EdgeDetectMode.SobelDepthThin;


        public override bool CheckResources ()
		{
            CheckSupport (true);

            edgeDetectMaterial = CheckShaderAndCreateMaterial (edgeDetectShader,edgeDetectMaterial);
            if (mode != oldMode)
                SetCameraFlag ();

            oldMode = mode;

            if (!isSupported)
                ReportAutoDisable ();
            return isSupported;
        }


        new void Start ()
		{
            oldMode	= mode;
        }

        void SetCameraFlag ()
		{
            if (mode == EdgeDetectMode.SobelDepth || mode == EdgeDetectMode.SobelDepthThin)
                GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
            else if (mode == EdgeDetectMode.TriangleDepthNormals || mode == EdgeDetectMode.RobertsCrossDepthNormals)
                GetComponent<Camera>().depthTextureMode |= DepthTextureMode.DepthNormals;
        }

        void OnEnable ()
		{
            SetCameraFlag();
        }

        [ImageEffectOpaque]
        void OnRenderImage (RenderTexture source, RenderTexture destination)
		{
            if (CheckResources () == false)
			{
                Graphics.Blit (source, destination);
                return;
            }

            Vector2 sensitivity = new Vector2 (sensitivityDepth, sensitivityNormals);
            edgeDetectMaterial.SetVector ("_Sensitivity", new Vector4 (sensitivity.x, sensitivity.y, 1.0f, sensitivity.y));
            edgeDetectMaterial.SetFloat ("_BgFade", edgesOnly);
            edgeDetectMaterial.SetFloat ("_SampleDistance", sampleDist);
            edgeDetectMaterial.SetVector ("_BgColor", edgesOnlyBgColor);
            edgeDetectMaterial.SetFloat ("_Exponent", edgeExp);
            edgeDetectMaterial.SetFloat ("_Threshold", lumThreshold);

            Graphics.Blit (source, destination, edgeDetectMaterial, (int) mode);
        }
    }

#if UNITY_EDITOR
    [CustomEditor (typeof (EdgeDetection))]
    class EdgeDetectionEditor :Editor
    {
        SerializedObject serObj;

        SerializedProperty mode;
        SerializedProperty sensitivityDepth;
        SerializedProperty sensitivityNormals;

        SerializedProperty lumThreshold;

        SerializedProperty edgesOnly;
        SerializedProperty edgesOnlyBgColor;

        SerializedProperty edgeExp;
        SerializedProperty sampleDist;


        void OnEnable ()
        {
            serObj = new SerializedObject (target);

            mode = serObj.FindProperty ("mode");

            sensitivityDepth = serObj.FindProperty ("sensitivityDepth");
            sensitivityNormals = serObj.FindProperty ("sensitivityNormals");

            lumThreshold = serObj.FindProperty ("lumThreshold");

            edgesOnly = serObj.FindProperty ("edgesOnly");
            edgesOnlyBgColor = serObj.FindProperty ("edgesOnlyBgColor");

            edgeExp = serObj.FindProperty ("edgeExp");
            sampleDist = serObj.FindProperty ("sampleDist");
        }


        public override void OnInspectorGUI ()
        {
            serObj.Update ();

            GUILayout.Label ("Detects spatial differences and converts into black outlines", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField (mode, new GUIContent ("Mode"));

            if (mode.intValue < 2)
            {
                EditorGUILayout.PropertyField (sensitivityDepth, new GUIContent (" Depth Sensitivity"));
                EditorGUILayout.PropertyField (sensitivityNormals, new GUIContent (" Normals Sensitivity"));
            }
            else if (mode.intValue < 4)
            {
                EditorGUILayout.PropertyField (edgeExp, new GUIContent (" Edge Exponent"));
            }
            else
            {
                // lum based mode
                EditorGUILayout.PropertyField (lumThreshold, new GUIContent (" Luminance Threshold"));
            }

            EditorGUILayout.PropertyField (sampleDist, new GUIContent (" Sample Distance"));

            EditorGUILayout.Separator ();

            GUILayout.Label ("Background Options");
            edgesOnly.floatValue = EditorGUILayout.Slider (" Edges only", edgesOnly.floatValue, 0.0f, 1.0f);
            EditorGUILayout.PropertyField (edgesOnlyBgColor, new GUIContent (" Color"));

            serObj.ApplyModifiedProperties ();
        }
    }
#endif
}
