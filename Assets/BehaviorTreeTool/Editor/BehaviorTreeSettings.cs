using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviorTreeTool.Editor
{
    // Create a new type of Settings Asset.
    [CreateAssetMenu]
    internal class BehaviorTreeSettings : ScriptableObject
    {
        public VisualTreeAsset BehaviorTreeXml => behaviorTreeXml;
        public StyleSheet BehaviorTreeStyle => behaviorTreeStyle;
        public VisualTreeAsset NodeXml => nodeXml;
        public TextAsset ScriptTemplateActionNode => scriptTemplateActionNode;
        public TextAsset ScriptTemplateCompositeNode => scriptTemplateCompositeNode;
        public TextAsset ScriptTemplateConditionNode => scriptTemplateConditionNode;
        public TextAsset ScriptTemplateDecoratorNode => scriptTemplateDecoratorNode;
        public string NewNodeBasePath => newNodeBasePath;

        [SerializeField] private VisualTreeAsset behaviorTreeXml;
        [SerializeField] private StyleSheet behaviorTreeStyle;
        [SerializeField] private VisualTreeAsset nodeXml;
        [SerializeField] private TextAsset scriptTemplateActionNode;
        [SerializeField] private TextAsset scriptTemplateCompositeNode;
        [SerializeField] private TextAsset scriptTemplateConditionNode;
        [SerializeField] private TextAsset scriptTemplateDecoratorNode;
        [SerializeField] private string newNodeBasePath = "Assets/";

        private static BehaviorTreeSettings FindSettings()
        {
            var guids = AssetDatabase.FindAssets("t:BehaviorTreeSettings");
            if (guids.Length > 1)
            {
                Debug.LogWarning($"Found multiple settings files, using the first.");
            }

            switch (guids.Length)
            {
                case 0:
                    return null;
                default:
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    return AssetDatabase.LoadAssetAtPath<BehaviorTreeSettings>(path);
            }
        }

        internal static BehaviorTreeSettings GetOrCreateSettings()
        {
            var settings = FindSettings();
            if (settings == null)
            {
                settings = CreateInstance<BehaviorTreeSettings>();
                AssetDatabase.CreateAsset(settings, "Assets");
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

// Register a SettingsProvider using UIElements for the drawing framework:
    internal static class MyCustomSettingsUIElementsRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
            var provider = new SettingsProvider("Project/MyCustomUIElementsSettings", SettingsScope.Project)
            {
                label = "BehaviorTree",
                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = BehaviorTreeSettings.GetSerializedSettings();

                    // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                    // isn't called because the SettingsProvider uses the UIElements drawing framework.
                    var title = new Label()
                    {
                        text = "Behaviour Tree Settings"
                    };
                    title.AddToClassList("title");
                    rootElement.Add(title);

                    var properties = new VisualElement()
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Column
                        }
                    };
                    properties.AddToClassList("property-list");
                    rootElement.Add(properties);

                    properties.Add(new InspectorElement(settings));

                    rootElement.Bind(settings);
                },
            };

            return provider;
        }
    }
}