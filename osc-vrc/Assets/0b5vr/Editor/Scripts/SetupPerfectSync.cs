using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;

public class SetupPerfectSyncDialog : EditorWindow
{
    GameObject m_modelRoot;
    AnimatorController m_animatorControllerFX;
    SkinnedMeshRenderer m_mesh;
    Dictionary<string, PerfectSyncType> m_perfectSyncTypes = new Dictionary<string, PerfectSyncType>(defaultPerfectSyncTypes);
    bool m_foldoutTypes = false;
    Vector2 m_scrollTypes = new Vector2();
    string m_outputJSONId = "<REPLACE ME>";
    string m_outputJSONName = "<REPLACE ME>";
    string m_outputJSONString;
    Vector2 m_scrollJSON = new Vector2();

    private static List<string> perfectSyncNameList = new List<string>()
    {
        "EyeBlinkLeft",
        "EyeLookDownLeft",
        "EyeLookInLeft",
        "EyeLookOutLeft",
        "EyeLookUpLeft",
        "EyeSquintLeft",
        "EyeWideLeft",
        "EyeBlinkRight",
        "EyeLookDownRight",
        "EyeLookInRight",
        "EyeLookOutRight",
        "EyeLookUpRight",
        "EyeSquintRight",
        "EyeWideRight",
        "JawForward",
        "JawLeft",
        "JawRight",
        "JawOpen",
        "MouthClose",
        "MouthFunnel",
        "MouthPucker",
        "MouthLeft",
        "MouthRight",
        "MouthSmileLeft",
        "MouthSmileRight",
        "MouthFrownLeft",
        "MouthFrownRight",
        "MouthDimpleLeft",
        "MouthDimpleRight",
        "MouthStretchLeft",
        "MouthStretchRight",
        "MouthRollLower",
        "MouthRollUpper",
        "MouthShrugLower",
        "MouthShrugUpper",
        "MouthPressLeft",
        "MouthPressRight",
        "MouthLowerDownLeft",
        "MouthLowerDownRight",
        "MouthUpperUpLeft",
        "MouthUpperUpRight",
        "BrowDownLeft",
        "BrowDownRight",
        "BrowInnerUp",
        "BrowOuterUpLeft",
        "BrowOuterUpRight",
        "CheekPuff",
        "CheekSquintLeft",
        "CheekSquintRight",
        "NoseSneerLeft",
        "NoseSneerRight",
        "TongueOut",
    };

    private enum PerfectSyncType
    {
        None,
        Bool1,
        Bool2,
        Float8,
    }

    private static Dictionary<PerfectSyncType, int> perfectSyncTypeToBitsMap = new Dictionary<PerfectSyncType, int>()
    {
        { PerfectSyncType.None, 0 },
        { PerfectSyncType.Bool1, 1 },
        { PerfectSyncType.Bool2, 2 },
        { PerfectSyncType.Float8, 8 },
    };

    private static Dictionary<PerfectSyncType, int> perfectSyncTypeThresholdMap = new Dictionary<PerfectSyncType, int>()
    {
        { PerfectSyncType.Bool1, 80 },
        { PerfectSyncType.Bool2, 30 },
    };

    private static Dictionary<string, PerfectSyncType> defaultPerfectSyncTypes = new Dictionary<string, PerfectSyncType>()
    {
        { "EyeBlinkLeft", PerfectSyncType.Float8 },
        { "EyeLookDownLeft", PerfectSyncType.None },
        { "EyeLookInLeft", PerfectSyncType.None },
        { "EyeLookOutLeft", PerfectSyncType.None },
        { "EyeLookUpLeft", PerfectSyncType.None },
        { "EyeSquintLeft", PerfectSyncType.Bool2 },
        { "EyeWideLeft", PerfectSyncType.Bool2 },
        { "EyeBlinkRight", PerfectSyncType.Float8 },
        { "EyeLookDownRight", PerfectSyncType.None },
        { "EyeLookInRight", PerfectSyncType.None },
        { "EyeLookOutRight", PerfectSyncType.None },
        { "EyeLookUpRight", PerfectSyncType.None },
        { "EyeSquintRight", PerfectSyncType.Bool2 },
        { "EyeWideRight", PerfectSyncType.Bool2 },
        { "JawForward", PerfectSyncType.None },
        { "JawLeft", PerfectSyncType.None },
        { "JawRight", PerfectSyncType.None },
        { "JawOpen", PerfectSyncType.Float8 },
        { "MouthClose", PerfectSyncType.Bool2 },
        { "MouthFunnel", PerfectSyncType.Bool2 },
        { "MouthPucker", PerfectSyncType.Bool2 },
        { "MouthLeft", PerfectSyncType.None },
        { "MouthRight", PerfectSyncType.None },
        { "MouthSmileLeft", PerfectSyncType.Bool2 },
        { "MouthSmileRight", PerfectSyncType.Bool2 },
        { "MouthFrownLeft", PerfectSyncType.None },
        { "MouthFrownRight", PerfectSyncType.None },
        { "MouthDimpleLeft", PerfectSyncType.None },
        { "MouthDimpleRight", PerfectSyncType.None },
        { "MouthStretchLeft", PerfectSyncType.Bool2 },
        { "MouthStretchRight", PerfectSyncType.Bool2 },
        { "MouthRollLower", PerfectSyncType.Bool2 },
        { "MouthRollUpper", PerfectSyncType.Bool2 },
        { "MouthShrugLower", PerfectSyncType.Bool2 },
        { "MouthShrugUpper", PerfectSyncType.Bool2 },
        { "MouthPressLeft", PerfectSyncType.Bool2 },
        { "MouthPressRight", PerfectSyncType.Bool2 },
        { "MouthLowerDownLeft", PerfectSyncType.Bool2 },
        { "MouthLowerDownRight", PerfectSyncType.Bool2 },
        { "MouthUpperUpLeft", PerfectSyncType.Bool2 },
        { "MouthUpperUpRight", PerfectSyncType.Bool2 },
        { "BrowDownLeft", PerfectSyncType.Bool2 },
        { "BrowDownRight", PerfectSyncType.Bool2 },
        { "BrowInnerUp", PerfectSyncType.Bool1 },
        { "BrowOuterUpLeft", PerfectSyncType.Bool1 },
        { "BrowOuterUpRight", PerfectSyncType.Bool1 },
        { "CheekPuff", PerfectSyncType.Bool1 },
        { "CheekSquintLeft", PerfectSyncType.Bool2 },
        { "CheekSquintRight", PerfectSyncType.Bool2 },
        { "NoseSneerLeft", PerfectSyncType.None },
        { "NoseSneerRight", PerfectSyncType.None },
        { "TongueOut", PerfectSyncType.Bool1 },
    };

    private static Dictionary<AnimatorControllerParameterType, VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType> parameterTypeVRCTypeMap = new Dictionary<AnimatorControllerParameterType, VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType>()
    {
        { AnimatorControllerParameterType.Bool, VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Bool },
        { AnimatorControllerParameterType.Float, VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Float },
        { AnimatorControllerParameterType.Int, VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Int },
    };

    private static Dictionary<AnimatorControllerParameterType, string> parameterTypeJSONTypeMap = new Dictionary<AnimatorControllerParameterType, string>()
    {
        { AnimatorControllerParameterType.Bool, "Int" }, // THIS IS INT
        { AnimatorControllerParameterType.Float, "Float" },
        { AnimatorControllerParameterType.Int, "Int" },
    };

    [Serializable]
    private class OutputJSONParameterInput
    {
        public string address;
        public string type;
    }

    [Serializable]
    private class OutputJSONParameter
    {
        public string name;
        public OutputJSONParameterInput input;
    }

    [Serializable]
    private class OutputJSON
    {
        public string id;
        public string name;
        public OutputJSONParameter[] parameters;
    }

    [MenuItem("0b5vr/Setup PerfectSync Animator", false, 0)]
    private static void Init()
    {
        var window = (SetupPerfectSyncDialog)GetWindow(typeof(SetupPerfectSyncDialog));
        window.titleContent = new GUIContent("Setup PerfectSync Animator");
        window.Show();
    }

    void OnEnable()
    {
        Debug.Log(Selection.activeTransform);
        if (Selection.activeTransform != null) {
            m_modelRoot = Selection.activeTransform.gameObject;

            if ( m_modelRoot != null )
            {
                var avatarDescriptor = m_modelRoot.GetComponent<VRCAvatarDescriptor>();
                if ( avatarDescriptor != null )
                {
                    var fxLayer = avatarDescriptor.baseAnimationLayers[ 4 ];
                    m_animatorControllerFX = (AnimatorController)fxLayer.animatorController;
                }

                m_mesh = m_modelRoot.GetComponentInChildren<SkinnedMeshRenderer>();

                var pipelineManager = m_modelRoot.GetComponent<VRC.Core.PipelineManager>();
                if ( pipelineManager != null )
                {
                    m_outputJSONName = pipelineManager.name;
                    m_outputJSONId = pipelineManager.blueprintId;
                }
            }
        }
    }

    void OnGUI()
    {
        // set property
        m_modelRoot = (GameObject)EditorGUILayout.ObjectField( "Model Root", m_modelRoot, typeof( GameObject ), true );

        if ( m_modelRoot != null )
        {
            if ( m_modelRoot.GetComponent<Animator>() == null )
            {
                EditorGUILayout.HelpBox( "No Animator Found!", MessageType.Error );
            }
        }

        m_animatorControllerFX = (AnimatorController)EditorGUILayout.ObjectField( "Animator Controller FX", m_animatorControllerFX, typeof( AnimatorController ), true );

        m_mesh = (SkinnedMeshRenderer)EditorGUILayout.ObjectField( "Mesh Renderer", m_mesh, typeof( SkinnedMeshRenderer ), true );

        // setup types
        if ( m_foldoutTypes = EditorGUILayout.Foldout( m_foldoutTypes, "BlendShape Types" ) ) {
            m_scrollTypes = GUILayout.BeginScrollView( m_scrollTypes, GUILayout.Height( 320 ) );

            EditorGUI.indentLevel ++;

            foreach ( var name in perfectSyncNameList )
            {
                m_perfectSyncTypes[ name ] = (PerfectSyncType)EditorGUILayout.EnumPopup( name, m_perfectSyncTypes[ name ] );
            }

            EditorGUI.indentLevel --;

            GUILayout.EndScrollView();
        }

        m_outputJSONId = EditorGUILayout.TextField( "JSON id", m_outputJSONId );
        m_outputJSONName = EditorGUILayout.TextField( "JSON name", m_outputJSONName );

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox( "Beware! It will overwrite the VRCExpressionParameters that already exists.", MessageType.None );
        EditorGUILayout.HelpBox( "It will disable VRChat viseme and VRChat eye transforms.", MessageType.None );

        // a big button
        if ( GUILayout.Button( "LFG" ) )
        {
            Setup();
        }

        EditorGUILayout.Space();

        EditorGUILayout.SelectableLabel(m_outputJSONString, EditorStyles.textArea, GUILayout.Height( 100 ) );

        if ( GUILayout.Button( "Open OSC Config Location" ) )
        {
            Regex regex = new Regex( "(.*/LocalLow)" );
            var match = regex.Match( Application.persistentDataPath );
            var path = $"{ match.Value }/VRChat/VRChat/OSC";
            EditorUtility.RevealInFinder( path );
        }
    }

    private void Setup()
    {
        var animator = m_modelRoot.GetComponent<Animator>();

        var expressionParameterList = new List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter>();

        var savePathFX = Path.GetDirectoryName( AssetDatabase.GetAssetPath( m_animatorControllerFX ) );

        var jsonParamList = new List<OutputJSONParameter>();

        string GetRelativePath( Transform transform )
        {
            var rootPath = m_modelRoot.transform.GetHierarchyPath();
            var transformPath = transform.GetHierarchyPath();
            return transformPath.Substring( rootPath.Length + 1 );
        }

        var meshPath = GetRelativePath( m_mesh.transform );

        void CreateParameter(
            AnimatorController controller,
            string parameterName,
            string address,
            AnimatorControllerParameterType animType,
            bool createVRCParam,
            bool createJSONParam
        )
        {
            // controller
            var index = Array.FindIndex( controller.parameters, ( parameter ) => parameter.name == parameterName );

            if ( index == -1 )
            {
                controller.AddParameter( parameterName, animType );
            }
            else
            {
                var parameter = controller.parameters[ index ];
                if ( parameter.type != animType )
                {
                    throw new System.Exception( $"The parameter { parameterName } already exists in the animator controller and I want to make it { animType }" );
                }
            }

            // vrc expression parameters
            if ( createVRCParam )
            {
                if ( expressionParameterList.FindIndex( ( param ) => param.name == parameterName ) == -1 )
                {
                    var vrcParam = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter
                    {
                        name = parameterName,
                        saved = false,
                        defaultValue = 0.0f,
                        valueType = parameterTypeVRCTypeMap[ animType ],
                    };
                    expressionParameterList.Add( vrcParam );
                }
            }

            // osc json
            if ( createJSONParam )
            {
                if ( address != null )
                {
                    if ( jsonParamList.FindIndex( ( param ) => param.name == parameterName ) == -1 )
                    {
                        var jsonParam = new OutputJSONParameter()
                        {
                            name = parameterName,
                            input = new OutputJSONParameterInput()
                            {
                                address = address,
                                type = parameterTypeJSONTypeMap[ animType ],
                            }
                        };
                        jsonParamList.Add( jsonParam );
                    }
                }
            }
        }

        AnimatorControllerLayer CreateOrOverwriteLayer( AnimatorController controller, string name )
        {
            var index = Array.FindIndex( controller.layers, ( layer ) => layer.name == name );

            if ( index != -1 )
            {
                controller.RemoveLayer( index );
            }

            var newLayer = new AnimatorControllerLayer
            {
                name = name,
                defaultWeight = 1.0f,
                stateMachine = new AnimatorStateMachine(),
            };

            controller.AddLayer( newLayer );

            return newLayer;
        }

        // humanoid
        {
            var head = animator.GetBoneTransform( HumanBodyBones.Head );
            var leftEye = animator.GetBoneTransform( HumanBodyBones.LeftEye );
            var rightEye = animator.GetBoneTransform( HumanBodyBones.RightEye );

            Transform CreateConstraintAndSource( string name, Transform parent, List<Transform> destinations )
            {
                // source - we are going to rotate head using constraint (what)
                var source = parent.transform.Find( name );
                if ( source == null )
                {
                    source = new GameObject( name ).transform;
                    source.parent = parent;
                }

                foreach ( var dst in destinations )
                {
                    // remove rotation constraint if it already exists
                    var rotConstraintAlreadyExists = dst.GetComponent<RotationConstraint>();
                    if ( rotConstraintAlreadyExists )
                    {
                        DestroyImmediate( rotConstraintAlreadyExists );
                    }

                    // and this is the constraint
                    var rotConstraint = dst.gameObject.AddComponent<RotationConstraint>();
                    var rotConstraintSource = new ConstraintSource()
                    {
                        sourceTransform = source,
                        weight = 1.0f,
                    };
                    rotConstraint.AddSource( rotConstraintSource );
                    rotConstraint.rotationOffset = Vector3.zero;
                    rotConstraint.constraintActive = true;
                }

                return source;
            }

            var headRef = CreateConstraintAndSource( "HeadRef", head.parent, new List<Transform>{ head } );
            var eyesRef = CreateConstraintAndSource( "EyesRef", head, new List<Transform>{ leftEye, rightEye } );

            // param - we need this in order to make blend tree work
            CreateParameter(
                m_animatorControllerFX,
                "One",
                null,
                AnimatorControllerParameterType.Float,
                false,
                false
            );

            for ( var i = 0; i < m_animatorControllerFX.parameters.Length; i ++ )
            {
                var parameter = m_animatorControllerFX.parameters[ i ];
                if ( parameter.name == "One" )
                {
                    var parameters = m_animatorControllerFX.parameters;
                    parameters[ i ].defaultFloat = 1.0f;
                    m_animatorControllerFX.parameters = parameters;
                }
            }

            // layer
            var layer = CreateOrOverwriteLayer( m_animatorControllerFX, "Humanoid" );

            // state
            var stateBlendTree = new AnimatorState();
            stateBlendTree.name = "Humanoid";
            layer.stateMachine.AddState( stateBlendTree, new Vector3( 300, 0, 0 ) );

            var rootBlendTree = new BlendTree();
            rootBlendTree.blendType = BlendTreeType.Direct;
            stateBlendTree.motion = rootBlendTree;

            void CreateRotationLayer(
                Transform transform,
                string axis,
                string name,
                string parameterName,
                string address,
                float range
            )
            {
                var blendTree = new BlendTree();
                blendTree.blendType = BlendTreeType.FreeformCartesian2D;
                blendTree.blendParameter = parameterName;
                blendTree.blendParameterY = parameterName;

                // param
                CreateParameter(
                    m_animatorControllerFX,
                    parameterName,
                    address,
                    AnimatorControllerParameterType.Float,
                    true,
                    true
                );

                // clips
                var clipN = new AnimationClip();
                var clip0 = new AnimationClip();
                var clipP = new AnimationClip();

                var relativePath = GetRelativePath( transform );
                clipN.SetCurve( relativePath, typeof( Transform ), $"localEulerAngles.{ axis }", AnimationCurve.Constant( 0, 1, -range ) );
                clip0.SetCurve( relativePath, typeof( Transform ), $"localEulerAngles.{ axis }", AnimationCurve.Constant( 0, 1, 0 ) );
                clipP.SetCurve( relativePath, typeof( Transform ), $"localEulerAngles.{ axis }", AnimationCurve.Constant( 0, 1, range ) );

                AssetDatabase.CreateAsset( clipN, $"{ savePathFX }/{ name }_N.anim" );
                AssetDatabase.CreateAsset( clip0, $"{ savePathFX }/{ name }_0.anim" );
                AssetDatabase.CreateAsset( clipP, $"{ savePathFX }/{ name }_P.anim" );

                blendTree.AddChild( clipN );
                blendTree.AddChild( clip0 );
                blendTree.AddChild( clipP );

                var children = blendTree.children;
                children[ 0 ].position = new Vector2( -1.0f, 0.0f );
                children[ 1 ].position = new Vector2( 0.0f, 0.0f );
                children[ 2 ].position = new Vector2( 1.0f, 0.0f );
                blendTree.children = children;

                // add to root blend tree
                rootBlendTree.AddChild( blendTree );

                var rootChildren = rootBlendTree.children;
                rootChildren[ rootChildren.Length - 1 ].directBlendParameter = "One";
                rootBlendTree.children = rootChildren;
            }

            // head
            CreateRotationLayer( headRef, "x", "Head_X", "Head_X", "/iFacialMocap/head/x", 120.0f );
            CreateRotationLayer( headRef, "y", "Head_Y", "Head_Y", "/iFacialMocap/head/y", 120.0f );
            CreateRotationLayer( headRef, "z", "Head_Z", "Head_Z", "/iFacialMocap/head/z", 120.0f );

            // eyes
            CreateRotationLayer( eyesRef, "x", "Eyes_X", "Eyes_X", "/iFacialMocap/eyes/x", 11.0f );
            CreateRotationLayer( eyesRef, "y", "Eyes_Y", "Eyes_Y", "/iFacialMocap/eyes/y", 15.0f );
        }

        // blendshapes
        foreach ( var name in perfectSyncNameList )
        {
            var bsName = ToCamelCase( name );

            var psType = m_perfectSyncTypes[ name ];
            if ( psType == PerfectSyncType.None ) { continue; }

            var bits = perfectSyncTypeToBitsMap[ psType ];

            // layer
            var layer = CreateOrOverwriteLayer( m_animatorControllerFX, $"{ name }" );

            if (
                psType == PerfectSyncType.Bool1 ||
                psType == PerfectSyncType.Bool2
            )
            {
                var maxInt = (int)Math.Pow( 2, bits );

                // params
                for ( var i = 0; i < bits; i ++ )
                {
                    CreateParameter(
                        m_animatorControllerFX,
                        $"{ name }_{ psType }_{ i }",
                        $"/iFacialMocap/blendshapes/{ bsName }/{ i }",
                        AnimatorControllerParameterType.Bool,
                        true,
                        true
                    );
                }

                // states
                var states = new List<AnimatorState>();

                for ( var iValue = 0; iValue < maxInt; iValue ++ )
                {
                    var state = new AnimatorState();
                    state.name = $"{ name }_{ psType }_{ iValue }";
                    layer.stateMachine.AddState( state, new Vector3( 300, 50 * iValue, 0 ) );

                    var value = (float)(iValue * perfectSyncTypeThresholdMap[ psType ]);

                    var clip = new AnimationClip();
                    clip.SetCurve( meshPath, typeof( SkinnedMeshRenderer ), $"blendShape.{ bsName }", AnimationCurve.Constant( 0, 1, value ) );
                    AssetDatabase.CreateAsset( clip, $"{ savePathFX }/{ name }_{ psType }_{ iValue }.anim" );
                    state.motion = clip;

                    var trans = layer.stateMachine.AddAnyStateTransition( state );
                    trans.duration = 0.05f;

                    for ( var iBit = 0; iBit < bits; iBit ++ )
                    {
                        var condition = ( ( 1 << iBit ) & iValue ) != 0
                            ? AnimatorConditionMode.If
                            : AnimatorConditionMode.IfNot;
                        trans.AddCondition( condition, 0, $"{ name }_{ psType }_{ iBit }" );
                    }

                    states.Add( state );
                }
            }
            else if ( psType == PerfectSyncType.Float8 )
            {
                var parameterName = $"{ name }_{ psType }";
                CreateParameter(
                    m_animatorControllerFX,
                    parameterName,
                    $"/iFacialMocap/blendshapes/{ bsName }",
                    AnimatorControllerParameterType.Float,
                    true,
                    true
                );

                // state
                var state = new AnimatorState();
                state.name = $"{ name }_{ psType }";
                layer.stateMachine.AddState( state, new Vector3( 300, 0, 0 ) );

                var blendTree = new BlendTree();
                blendTree.blendType = BlendTreeType.Direct;
                state.motion = blendTree;

                var clip = new AnimationClip();
                clip.SetCurve( meshPath, typeof( SkinnedMeshRenderer ), $"blendShape.{ bsName }", AnimationCurve.Constant( 0, 1, 100.0f ) );
                AssetDatabase.CreateAsset( clip, $"{ savePathFX }/{ name }_{ psType }.anim" );

                blendTree.AddChild( clip );
                var children = blendTree.children;
                var childMotionIndex = children.Length - 1;
                children[ childMotionIndex ].directBlendParameter = parameterName;
                blendTree.children = children;
            }
        }

        // heyo avatar descriptor
        var avatarDescriptor = m_modelRoot.GetComponent<VRCAvatarDescriptor>();
        avatarDescriptor.expressionParameters.parameters = expressionParameterList.ToArray();
        avatarDescriptor.lipSync = VRC.SDKBase.VRC_AvatarDescriptor.LipSyncStyle.VisemeParameterOnly;
        avatarDescriptor.enableEyeLook = false;

        // https://www.youtube.com/watch?v=taWzoLpdxaI
        var outputJSON = new OutputJSON()
        {
            id = m_outputJSONId,
            name = m_outputJSONName,
            parameters = jsonParamList.ToArray(),
        };
        m_outputJSONString = JsonUtility.ToJson( outputJSON, true );
    }

    private static string ToCamelCase( string str )
    {
        return Char.ToLowerInvariant( str[ 0 ] ) + str.Substring( 1 );
    }
}
