using System;
using MochiBTS.Core;
using MochiBTS.Core.Primitives;
using MochiBTS.Core.Primitives.DataContainers;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
namespace MochiBTS.Editor
{
    public class BehaviorTreeEditor : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset visualTree;
        private Agent agent;
        private InspectorView blackboardView;
        //private SerializedProperty blackboardProperty;

        //private IMGUIContainer blackboardView;
        private InspectorView inspectorView;
        private TreeRunner runner;
        private BehaviorTreeSettings settings;

        //private SerializedObject treeObject;
        private BehaviorTreeView treeView;
        private VariableBoard variableBoard;

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }


        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        public void CreateGUI()
        {
            settings = BehaviorTreeSettings.GetOrCreateSettings();
            if (settings.behaviorTreeXml is null || settings.behaviorTreeStyle is null) return;
            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;

            // Instantiate UXML
            visualTree = settings.behaviorTreeXml;
            visualTree.CloneTree(root);

            var styleSheet = settings.behaviorTreeStyle;
            root.styleSheets.Add(styleSheet);

            treeView = root.Q<BehaviorTreeView>();
            treeView.AddSearchWindow(this);
            inspectorView = root.Q<InspectorView>();
            blackboardView = root.Q<InspectorView>("Blackboard");
            // blackboardView = root.Q<IMGUIContainer>();
            // blackboardView.onGUIHandler = () => {
            //     if (treeObject?.targetObject is null) return;
            //     treeObject?.Update();
            //     EditorGUILayout.PropertyField(blackboardProperty,includeChildren:true);
            //     treeObject?.ApplyModifiedProperties();
            // };
            treeView.onNodeSelected = OnNodeSelectionChanged;
            root.Q<Button>("blackboardButton").clicked += () => blackboardView?.UpdateBlackBoard(treeView.tree);
            root.Q<Button>("variableButton").clicked +=
                () => blackboardView?.UpdateVariableBoard(variableBoard);
            root.Q<Button>("agentButton").clicked +=
                () => blackboardView?.UpdateAgent(agent);
            root.Q<Button>("runnerButton").clicked +=
                () => blackboardView?.UpdateRunner(runner);
            OnSelectionChange();
        }

        private void OnInspectorUpdate()
        {
            treeView?.UpdateNodeStates();
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject is null) return;
            var tree = Selection.activeObject as BehaviorTree;
            if (!tree)
                if (Selection.activeObject is GameObject gameObject) {
                    runner = gameObject.GetComponent<TreeRunner>();
                    if (runner)
                        tree = runner.tree;
                    agent = gameObject.GetComponent<Agent>();
                    if (agent)
                        variableBoard = agent.variableBoard;
                }
            if (Application.isPlaying) {
                if (tree)
                    treeView?.PopulateView(tree);
            } else {
                if (tree && AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID()))
                    treeView?.PopulateView(tree);
            }

            if (tree != null) blackboardView?.UpdateBlackBoard(tree);
            //     treeObject = new SerializedObject(tree);
            //     blackboardProperty = treeObject.FindProperty("blackboard");



        }

        [MenuItem("MochiBTS/Editor")]
        public static void OpenWindow()
        {
            var wnd = GetWindow<BehaviorTreeEditor>();
            wnd.titleContent = new GUIContent("MochiBTSEditor");
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj) {

                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(obj), obj, null);
            }
        }

        private void OnNodeSelectionChanged(NodeView node)
        {
            inspectorView.UpdateSelection(node);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is not BehaviorTree tree) return false;
            OpenWindow();
            return true;
        }
    }
}