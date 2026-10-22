using System;
using MedicalDeck.Data;
using MedicalDeck.Gameplay;
using MedicalDeck.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MedicalDeck.EditorTools
{
    public static class SceneBuilderHelpers
    {
        public static Font DefaultFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static void CreateCanvasRoot(string canvasName, out RectTransform root)
        {
            if (UnityEngine.Object.FindFirstObjectByType<Camera>() == null)
            {
                var cameraGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                var cam = cameraGo.GetComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.13f, 0.15f, 0.18f);
                cam.orthographic = true;
                cameraGo.tag = "MainCamera";
            }

            var canvasGo = new GameObject(canvasName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }

            root = (RectTransform)canvasGo.transform;
        }

        public static Text CreateText(Transform parent, string name, string content, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            var text = go.AddComponent<Text>();
            text.text = content;
            text.color = Color.black;
            text.font = DefaultFont;
            text.fontSize = fontSize;
            text.alignment = alignment;
            return text;
        }

        public static Text CreateText(Transform parent, string name, string content, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            var text = go.AddComponent<Text>();
            text.text = content;
            text.color = Color.black;
            text.font = DefaultFont;
            text.fontSize = fontSize;
            text.alignment = alignment;
            return text;
        }

        public static Button CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            var image = go.GetComponent<Image>();
            image.color = new Color(0.75f, 0.75f, 0.75f);
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var labelRect = CreateText(go.transform, "Label", label, Vector2.zero, Vector2.zero, 24, TextAnchor.MiddleCenter);
            ((RectTransform)labelRect.transform).anchorMin = Vector2.zero;
            ((RectTransform)labelRect.transform).anchorMax = Vector2.one;
            ((RectTransform)labelRect.transform).offsetMin = Vector2.zero;
            ((RectTransform)labelRect.transform).offsetMax = Vector2.zero;
            return button;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f, 0.5f);
            return rect;
        }

        public static FamilyMemberView CreateFamilyMemberView(Transform parent, string name, int index, int total)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            float width = 1f / total;
            rect.anchorMin = new Vector2(index * width, 0f);
            rect.anchorMax = new Vector2((index + 1) * width, 1f);
            rect.offsetMin = new Vector2(4, 4);
            rect.offsetMax = new Vector2(-4, -4);
            go.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f);

            var view = go.AddComponent<FamilyMemberView>();

            var nameText = CreateText(go.transform, "NameText", "Name", new Vector2(0, 55), new Vector2(160, 30), 22, TextAnchor.MiddleCenter);
            var healthText = CreateText(go.transform, "HealthText", "20/20", new Vector2(0, 20), new Vector2(160, 30), 20, TextAnchor.MiddleCenter);
            var infectionText = CreateText(go.transform, "InfectionText", "", new Vector2(0, -10), new Vector2(160, 30), 20, TextAnchor.MiddleCenter);
            var buffText = CreateText(go.transform, "BuffText", "", new Vector2(0, -40), new Vector2(160, 30), 14, TextAnchor.MiddleCenter);
            var artImage = CreateImage(go.transform, "Art", new Vector2(0, 10), new Vector2(140, 90), new Color(0.7f, 0.7f, 0.7f));
            var infectionOverlay = CreateImage(go.transform, "InfectionOverlay", new Vector2(0, 10), new Vector2(140, 90), new Color(0.2f, 0.8f, 0.2f, 0.35f));
            infectionOverlay.gameObject.SetActive(false);
            var highlight = CreateImage(go.transform, "Highlight", Vector2.zero, Vector2.zero, new Color(1f, 0.9f, 0.2f, 0.35f));
            StretchToParent((RectTransform)highlight.transform, -6);
            highlight.gameObject.SetActive(false);
            var deadOverlay = CreateImage(go.transform, "DeadOverlay", Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.6f));
            StretchToParent((RectTransform)deadOverlay.transform, 0);
            deadOverlay.gameObject.SetActive(false);

            var so = new SerializedObject(view);
            SetProp(so, "nameText", nameText);
            SetProp(so, "healthText", healthText);
            SetProp(so, "infectionText", infectionText);
            SetProp(so, "buffText", buffText);
            SetProp(so, "artImage", artImage);
            SetProp(so, "infectionOverlayImage", infectionOverlay);
            SetProp(so, "highlightObject", highlight.gameObject);
            SetProp(so, "deadOverlayObject", deadOverlay.gameObject);
            SetProp(so, "selectButton", go.GetComponent<Button>());
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        public static Image CreateImage(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        public static void StretchToParent(RectTransform rect, float padding)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }

        public static CardView BuildCardView(GameObject go)
        {
            var view = go.AddComponent<CardView>();
            var nameText = CreateText(go.transform, "NameText", "Card", new Vector2(0, 75), new Vector2(130, 30), 20, TextAnchor.MiddleCenter);
            var descText = CreateText(go.transform, "DescText", "Description", new Vector2(0, -30), new Vector2(130, 60), 14, TextAnchor.UpperCenter);
            var artImage = CreateImage(go.transform, "Art", new Vector2(0, 25), new Vector2(120, 90), new Color(0.65f, 0.65f, 0.65f));
            var highlight = CreateImage(go.transform, "SelectionHighlight", Vector2.zero, Vector2.zero, new Color(1f, 0.9f, 0.2f, 0.4f));
            StretchToParent((RectTransform)highlight.transform, -4);
            highlight.gameObject.SetActive(false);

            var so = new SerializedObject(view);
            SetProp(so, "cardNameText", nameText);
            SetProp(so, "descriptionText", descText);
            SetProp(so, "artImage", artImage);
            SetProp(so, "frameImage", go.GetComponent<Image>());
            SetProp(so, "selectionHighlight", highlight.gameObject);
            SetProp(so, "cardButton", go.GetComponent<Button>());
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        public static GameObject CreateCardPrefabAsset()
        {
            var go = new GameObject("CardViewPrefab", typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(140, 200);
            go.GetComponent<Image>().color = new Color(0.92f, 0.92f, 0.92f);
            BuildCardView(go);

            string folder = "Assets/Prefabs";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "Prefabs");
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/CardViewPrefab.prefab");
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return prefab;
        }

        public static GameObject CreateCardRowPrefabAsset()
        {
            var go = new GameObject("CardRowPrefab", typeof(RectTransform), typeof(Image), typeof(Button));
            var rect = (RectTransform)go.transform;
            rect.sizeDelta = new Vector2(600, 60);
            go.GetComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f, 0.6f);

            var row = go.AddComponent<DeckEditorCardRow>();
            var nameText = CreateText(go.transform, "NameText", "Card", new Vector2(20, 0), new Vector2(180, 40), 20, TextAnchor.MiddleLeft);
            var nameRect = (RectTransform)nameText.transform;
            nameRect.anchorMin = new Vector2(0, 0.5f);
            nameRect.anchorMax = new Vector2(0, 0.5f);
            nameRect.anchoredPosition = new Vector2(15, 0);
            nameRect.sizeDelta = new Vector2(180, 40);

            var descText = CreateText(go.transform, "DescText", "Description", new Vector2(0, 0), new Vector2(280, 40), 14, TextAnchor.MiddleLeft);
            var descRect = (RectTransform)descText.transform;
            descRect.anchorMin = new Vector2(0.35f, 0.5f);
            descRect.anchorMax = new Vector2(0.35f, 0.5f);
            descRect.anchoredPosition = new Vector2(0, 0);
            descRect.sizeDelta = new Vector2(280, 40);

            var so = new SerializedObject(row);
            SetProp(so, "cardNameText", nameText);
            SetProp(so, "descriptionText", descText);
            SetProp(so, "actionButton", go.GetComponent<Button>());
            so.ApplyModifiedPropertiesWithoutUndo();

            string folder = "Assets/Prefabs";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "Prefabs");
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/CardRowPrefab.prefab");
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
            return prefab;
        }

        public static void SetProp(SerializedObject so, string field, UnityEngine.Object value)
        {
            var prop = so.FindProperty(field);
            if (prop != null) prop.objectReferenceValue = value;
            else Debug.LogWarning($"[SceneBuilder] Field '{field}' not found on {so.targetObject.GetType().Name}.");
        }

        public static CardData[] FindAllCards()
        {
            return Array.ConvertAll(
                AssetDatabase.FindAssets("t:CardData"),
                guid => AssetDatabase.LoadAssetAtPath<CardData>(AssetDatabase.GUIDToAssetPath(guid)));
        }

        public static FamilyMemberData[] FindAllFamilyMembers()
        {
            return Array.ConvertAll(
                AssetDatabase.FindAssets("t:FamilyMemberData"),
                guid => AssetDatabase.LoadAssetAtPath<FamilyMemberData>(AssetDatabase.GUIDToAssetPath(guid)));
        }

        public static void AddSceneToBuild(string sceneName)
        {
            string scenePath = $"Assets/Scenes/{sceneName}.unity";
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == scenePath);
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}