using MedicalDeck.Gameplay;
using MedicalDeck.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MedicalDeck.EditorTools
{
    public static class SceneBuilder
    {
        private const string MenuSceneName = "MenuScene";
        private const string BattleSceneName = "BattleScene";

        [MenuItem("Medical Deck/Create All Scenes", priority = 0)]
        public static void CreateAllScenes()
        {
            BuildMenuScene();
            BuildBattleScene();
            Debug.Log("[SceneBuilder] Scenes created and registered in Build Settings.");
        }

        [MenuItem("Medical Deck/Create Menu Scene", priority = 1)]
        public static void BuildMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SceneBuilderHelpers.CreateCanvasRoot("MenuCanvas", out var root);

            var editorGo = new GameObject("DeckEditor");
            var editor = editorGo.AddComponent<DeckEditorMenu>();

            var title = SceneBuilderHelpers.CreateText(root, "Title", "DECK EDITOR — build your deck, then start the match",
                new Vector2(0, -45), new Vector2(900, 50), 34, TextAnchor.MiddleCenter);
            ((RectTransform)title.transform).anchorMin = new Vector2(0.5f, 1f);
            ((RectTransform)title.transform).anchorMax = new Vector2(0.5f, 1f);

            var collectionScroll = SceneBuilderHelpers.CreateScrollableGrid(root, "CollectionScroll", new Vector2(0.03f, 0.16f), new Vector2(0.48f, 0.78f), 4, out var collectionContent);
            var deckScroll = SceneBuilderHelpers.CreateScrollableGrid(root, "DeckScroll", new Vector2(0.52f, 0.16f), new Vector2(0.97f, 0.78f), 4, out var deckContent);

            var collHeader = SceneBuilderHelpers.CreateText(collectionScroll, "CollHeader", "ALL CARDS (click to add)", new Vector2(0, -25), new Vector2(400, 30), 22, TextAnchor.MiddleCenter);
            StretchTop((RectTransform)collHeader.transform);
            var deckHeader = SceneBuilderHelpers.CreateText(deckScroll, "DeckHeader", "YOUR DECK (click a card to remove one copy)", new Vector2(0, -25), new Vector2(400, 30), 22, TextAnchor.MiddleCenter);
            StretchTop((RectTransform)deckHeader.transform);
            var tilePrefab = SceneBuilderHelpers.CreateCardTilePrefabAsset();

            var deckCountText = SceneBuilderHelpers.CreateText(root, "DeckCount", "Deck: 0/30", new Vector2(0, 0), new Vector2(300, 40), 24, TextAnchor.MiddleCenter);
            ((RectTransform)deckCountText.transform).anchorMin = new Vector2(0.5f, 0f);
            ((RectTransform)deckCountText.transform).anchorMax = new Vector2(0.5f, 0f);
            ((RectTransform)deckCountText.transform).anchoredPosition = new Vector2(0, 32);

            var startButton = SceneBuilderHelpers.CreateButton(root, "StartButton", "START MATCH", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-100, 80), new Vector2(100, 140));
            UnityEventTools.AddPersistentListener(startButton.onClick, editor.StartMatch);

            var previewPanel = SceneBuilderHelpers.CreateFloatingPanel(root, "PreviewPanel", new Vector2(360, 540), new Color(0.15f, 0.17f, 0.2f, 0.95f));
            var previewImage = previewPanel.GetComponent<Image>();
            previewImage.raycastTarget = false;
            var previewArt = SceneBuilderHelpers.CreateImage(previewPanel, "PreviewArt", new Vector2(0, 120), new Vector2(260, 260), Color.white);
            previewArt.raycastTarget = false;
            var previewName = SceneBuilderHelpers.CreateText(previewPanel, "PreviewName", "", new Vector2(0, -50), new Vector2(280, 36), 28, TextAnchor.MiddleCenter);
            previewName.raycastTarget = false;
            var previewDesc = SceneBuilderHelpers.CreateText(previewPanel, "PreviewDesc", "", new Vector2(0, -130), new Vector2(300, 150), 18, TextAnchor.UpperCenter);
            previewDesc.raycastTarget = false;
            previewPanel.gameObject.SetActive(false);
            var addDialog = SceneBuilderHelpers.CreatePanel(root, "AddDialog", Vector2.zero, Vector2.one, new Color(0, 0, 0, 0.6f));
            var dialogBox = SceneBuilderHelpers.CreatePanel(addDialog, "DialogBox", new Vector2(0.34f, 0.24f), new Vector2(0.66f, 0.76f), new Color(0.9f, 0.9f, 0.95f, 1f));

            var dialogArt = SceneBuilderHelpers.CreateImage(dialogBox, "DialogArt", new Vector2(0, 120), new Vector2(180, 180), Color.white);
            var dialogName = SceneBuilderHelpers.CreateText(dialogBox, "DialogName", "", new Vector2(0, -5), new Vector2(300, 34), 26, TextAnchor.MiddleCenter);
            var dialogDesc = SceneBuilderHelpers.CreateText(dialogBox, "DialogDesc", "", new Vector2(0, -50), new Vector2(320, 70), 16, TextAnchor.UpperCenter);
            var dialogQuestion = SceneBuilderHelpers.CreateText(dialogBox, "DialogQuestion", "Add this card to your deck?", new Vector2(0, -130), new Vector2(320, 28), 20, TextAnchor.MiddleCenter);
            var confirmButton = SceneBuilderHelpers.CreateButton(dialogBox, "ConfirmButton", "CONFIRM", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-150, -62), new Vector2(-10, -14));
            var cancelButton = SceneBuilderHelpers.CreateButton(dialogBox, "CancelButton", "CANCEL", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(10, -62), new Vector2(150, -14));
            UnityEventTools.AddPersistentListener(confirmButton.onClick, editor.ConfirmAdd);
            UnityEventTools.AddPersistentListener(cancelButton.onClick, editor.CancelAdd);
            addDialog.gameObject.SetActive(false);

            var so = new SerializedObject(editor);
            var cards = SceneBuilderHelpers.FindAllCards();
            var cardsProp = so.FindProperty("allCards");
            so.Update();
            cardsProp.arraySize = cards.Length;
            for (int i = 0; i < cards.Length; i++)
            {
                cardsProp.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
            }
            SceneBuilderHelpers.SetProp(so, "collectionGrid", collectionContent);
            SceneBuilderHelpers.SetProp(so, "deckGrid", deckContent); SceneBuilderHelpers.SetProp(so, "tilePrefab", tilePrefab.GetComponent<DeckEditorCardRow>());
            SceneBuilderHelpers.SetProp(so, "deckCountText", deckCountText);
            SceneBuilderHelpers.SetProp(so, "addDialog", addDialog.gameObject);
            SceneBuilderHelpers.SetProp(so, "addDialogNameText", dialogName);
            SceneBuilderHelpers.SetProp(so, "addDialogDescText", dialogDesc);
            SceneBuilderHelpers.SetProp(so, "addDialogArtImage", dialogArt);
            SceneBuilderHelpers.SetProp(so, "previewPanel", previewPanel.gameObject);
            SceneBuilderHelpers.SetProp(so, "previewArtImage", previewArt);
            SceneBuilderHelpers.SetProp(so, "previewNameText", previewName);
            SceneBuilderHelpers.SetProp(so, "previewDescText", previewDesc);
            so.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, $"Assets/Scenes/{MenuSceneName}.unity");
            SceneBuilderHelpers.AddSceneToBuild(MenuSceneName);
            Debug.Log($"[SceneBuilder] Menu scene saved with {cards.Length} cards found in project.");
        }

        [MenuItem("Medical Deck/Create Battle Scene", priority = 2)]
        public static void BuildBattleScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SceneBuilderHelpers.CreateCanvasRoot("BattleCanvas", out var root);

            var matchGo = new GameObject("MatchManager");
            var match = matchGo.AddComponent<MatchManager>();
            matchGo.AddComponent<EnemyAI>();

            var playerDeckGo = new GameObject("PlayerDeckManager");
            var playerDeck = playerDeckGo.AddComponent<DeckManager>();
            var enemyDeckGo = new GameObject("EnemyDeckManager");
            var enemyDeck = enemyDeckGo.AddComponent<DeckManager>();

            var playerFamilyRow = SceneBuilderHelpers.CreatePanel(root, "PlayerFamilyRow", new Vector2(0.2f, 0.30f), new Vector2(0.8f, 0.52f));
            var enemyFamilyRow = SceneBuilderHelpers.CreatePanel(root, "EnemyFamilyRow", new Vector2(0.2f, 0.60f), new Vector2(0.8f, 0.82f));

            var members = SceneBuilderHelpers.FindAllFamilyMembers();
            int familyCount = Mathf.Max(3, members.Length > 0 ? members.Length : 3);
            var playerViews = new MedicalDeck.UI.FamilyMemberView[familyCount];
            var enemyViews = new MedicalDeck.UI.FamilyMemberView[familyCount];
            for (int i = 0; i < familyCount; i++)
            {
                playerViews[i] = SceneBuilderHelpers.CreateFamilyMemberView(playerFamilyRow, $"PlayerMember{i}", i, familyCount);
                enemyViews[i] = SceneBuilderHelpers.CreateFamilyMemberView(enemyFamilyRow, $"EnemyMember{i}", i, familyCount);
            }

            var handRow = SceneBuilderHelpers.CreatePanel(root, "PlayerHandRow", new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.26f));
            var cardPrefab = SceneBuilderHelpers.CreateCardPrefabAsset();
            var handViewGo = new GameObject("CardHandView");
            var handView = handViewGo.AddComponent<MedicalDeck.UI.CardHandView>();

            var battleUIGo = new GameObject("BattleUI");
            var battleUI = battleUIGo.AddComponent<MedicalDeck.UI.BattleUIController>();

            BuildBattleHud(root, battleUI, matchGo, match, playerDeck, enemyDeck);

            var so = new SerializedObject(match);
            SceneBuilderHelpers.SetProp(so, "playerDeck", playerDeck);
            SceneBuilderHelpers.SetProp(so, "enemyDeck", enemyDeck);
            SceneBuilderHelpers.SetProp(so, "battleUI", battleUI);
            SceneBuilderHelpers.SetProp(so, "playerHandView", handView);
            var pfField = so.FindProperty("playerFamilyViews");
            pfField.arraySize = familyCount;
            for (int i = 0; i < familyCount; i++) pfField.GetArrayElementAtIndex(i).objectReferenceValue = playerViews[i];
            var efField = so.FindProperty("enemyFamilyViews");
            efField.arraySize = familyCount;
            for (int i = 0; i < familyCount; i++) efField.GetArrayElementAtIndex(i).objectReferenceValue = enemyViews[i];
            var pdField = so.FindProperty("playerFamilyData");
            pdField.arraySize = members.Length;
            for (int i = 0; i < members.Length; i++) pdField.GetArrayElementAtIndex(i).objectReferenceValue = members[i];
            var edField = so.FindProperty("enemyFamilyData");
            edField.arraySize = members.Length;
            for (int i = 0; i < members.Length; i++) edField.GetArrayElementAtIndex(i).objectReferenceValue = members[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            var handSo = new SerializedObject(handView);
            SceneBuilderHelpers.SetProp(handSo, "match", match);
            SceneBuilderHelpers.SetProp(handSo, "cardContainer", handRow);
            SceneBuilderHelpers.SetProp(handSo, "cardViewPrefab", cardPrefab.GetComponent<MedicalDeck.UI.CardView>());
            handSo.ApplyModifiedPropertiesWithoutUndo();

            var playerDeckSo = new SerializedObject(playerDeck);
            var playerCards = SceneBuilderHelpers.FindAllCards();
            playerDeckSo.Update();
            var masterDeckField = playerDeckSo.FindProperty("masterDeck");
            masterDeckField.arraySize = playerCards.Length;
            for (int i = 0; i < playerCards.Length; i++) masterDeckField.GetArrayElementAtIndex(i).objectReferenceValue = playerCards[i];
            playerDeckSo.ApplyModifiedPropertiesWithoutUndo();

            var enemyDeckSo = new SerializedObject(enemyDeck);
            enemyDeckSo.Update();
            var enemyMasterField = enemyDeckSo.FindProperty("masterDeck");
            enemyMasterField.arraySize = playerCards.Length;
            for (int i = 0; i < playerCards.Length; i++) enemyMasterField.GetArrayElementAtIndex(i).objectReferenceValue = playerCards[i];
            enemyDeckSo.ApplyModifiedPropertiesWithoutUndo();

            var loaderGo = new GameObject("PlayerDeckLoader");
            var loader = loaderGo.AddComponent<PlayerDeckLoader>();
            var loaderSo = new SerializedObject(loader);
            SceneBuilderHelpers.SetProp(loaderSo, "playerDeck", playerDeck);
            var allCardsProp = loaderSo.FindProperty("allCards");
            allCardsProp.arraySize = playerCards.Length;
            for (int i = 0; i < playerCards.Length; i++) allCardsProp.GetArrayElementAtIndex(i).objectReferenceValue = playerCards[i];
            loaderSo.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            EditorSceneManager.SaveScene(scene, $"Assets/Scenes/{BattleSceneName}.unity");
            SceneBuilderHelpers.AddSceneToBuild(BattleSceneName);
            Debug.Log("[SceneBuilder] Battle scene saved and fully wired.");
        }

        private static void StretchTop(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -25);
            rect.sizeDelta = new Vector2(400, 30);
        }

        private static void BuildBattleHud(RectTransform root, MedicalDeck.UI.BattleUIController battleUI, GameObject matchGo, MatchManager match, DeckManager playerDeck, DeckManager enemyDeck)
        {
            var roundLabel = SceneBuilderHelpers.CreateText(root, "RoundLabel", "Round 1/15", new Vector2(0, -30), new Vector2(200, 40), 26, TextAnchor.MiddleCenter);
            ((RectTransform)roundLabel.transform).anchorMin = new Vector2(0.5f, 1f);
            ((RectTransform)roundLabel.transform).anchorMax = new Vector2(0.5f, 1f);

            var phaseLabel = SceneBuilderHelpers.CreateText(root, "PhaseLabel", "", new Vector2(0, -70), new Vector2(300, 40), 22, TextAnchor.MiddleCenter);
            ((RectTransform)phaseLabel.transform).anchorMin = new Vector2(0.5f, 1f);
            ((RectTransform)phaseLabel.transform).anchorMax = new Vector2(0.5f, 1f);

            var promptText = SceneBuilderHelpers.CreateText(root, "PromptText", "Your turn", new Vector2(0, 20), new Vector2(600, 40), 20, TextAnchor.MiddleCenter);
            ((RectTransform)promptText.transform).anchorMin = new Vector2(0.5f, 0.27f);
            ((RectTransform)promptText.transform).anchorMax = new Vector2(0.5f, 0.27f);

            var playerDeckText = SceneBuilderHelpers.CreateText(root, "PlayerDeckCount", "Deck: 0", new Vector2(120, 60), new Vector2(200, 40), 20, TextAnchor.MiddleLeft);
            ((RectTransform)playerDeckText.transform).anchorMin = new Vector2(0f, 0.5f);
            ((RectTransform)playerDeckText.transform).anchorMax = new Vector2(0f, 0.5f);

            var playerDiscardText = SceneBuilderHelpers.CreateText(root, "PlayerDiscardCount", "Discard: 0", new Vector2(120, 10), new Vector2(200, 40), 20, TextAnchor.MiddleLeft);
            ((RectTransform)playerDiscardText.transform).anchorMin = new Vector2(0f, 0.5f);
            ((RectTransform)playerDiscardText.transform).anchorMax = new Vector2(0f, 0.5f);

            var enemyDeckText = SceneBuilderHelpers.CreateText(root, "EnemyDeckCount", "Enemy Deck: 0", new Vector2(-120, 60), new Vector2(200, 40), 20, TextAnchor.MiddleRight);
            ((RectTransform)enemyDeckText.transform).anchorMin = new Vector2(1f, 0.5f);
            ((RectTransform)enemyDeckText.transform).anchorMax = new Vector2(1f, 0.5f);

            var enemyDiscardText = SceneBuilderHelpers.CreateText(root, "EnemyDiscardCount", "Enemy Discard: 0", new Vector2(-120, 10), new Vector2(200, 40), 20, TextAnchor.MiddleRight);
            ((RectTransform)enemyDiscardText.transform).anchorMin = new Vector2(1f, 0.5f);
            ((RectTransform)enemyDiscardText.transform).anchorMax = new Vector2(1f, 0.5f);

            var playButton = SceneBuilderHelpers.CreateButton(root, "PlayButton", "PLAY", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-320, 40), new Vector2(-160, 90));
            var discardButton = SceneBuilderHelpers.CreateButton(root, "DiscardButton", "DISCARD", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-150, 40), new Vector2(10, 90));
            var endTurnButton = SceneBuilderHelpers.CreateButton(root, "EndTurnButton", "END TURN", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-320, 110), new Vector2(-160, 160));

            var gameOverPanel = SceneBuilderHelpers.CreatePanel(root, "GameOverPanel", new Vector2(0.3f, 0.35f), new Vector2(0.7f, 0.65f));
            gameOverPanel.gameObject.SetActive(false);
            var gameOverText = SceneBuilderHelpers.CreateText(gameOverPanel, "GameOverText", "", new Vector2(0, 20), new Vector2(500, 60), 30, TextAnchor.MiddleCenter);
            var backToMenuButton = SceneBuilderHelpers.CreateButton(gameOverPanel, "BackToMenuButton", "BACK TO MENU", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-150, -60), new Vector2(150, -20));
            UnityEventTools.AddPersistentListener(backToMenuButton.onClick, battleUI.LoadMenuScene); var rematchButton = SceneBuilderHelpers.CreateButton(gameOverPanel, "RematchButton", "REMATCH", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(10, -60), new Vector2(170, -20));
            UnityEventTools.AddPersistentListener(rematchButton.onClick, battleUI.LoadBattleScene);

            var so = new SerializedObject(battleUI);
            SceneBuilderHelpers.SetProp(so, "roundLabel", roundLabel);
            SceneBuilderHelpers.SetProp(so, "phaseLabel", phaseLabel);
            SceneBuilderHelpers.SetProp(so, "promptText", promptText);
            SceneBuilderHelpers.SetProp(so, "enemyDeckCountText", enemyDeckText);
            SceneBuilderHelpers.SetProp(so, "enemyDiscardCountText", enemyDiscardText);
            SceneBuilderHelpers.SetProp(so, "playerDeckCountText", playerDeckText);
            SceneBuilderHelpers.SetProp(so, "playerDiscardCountText", playerDiscardText);
            SceneBuilderHelpers.SetProp(so, "playButton", playButton);
            SceneBuilderHelpers.SetProp(so, "discardButton", discardButton);
            SceneBuilderHelpers.SetProp(so, "endTurnButton", endTurnButton);
            SceneBuilderHelpers.SetProp(so, "gameOverPanel", gameOverPanel.gameObject);
            SceneBuilderHelpers.SetProp(so, "gameOverText", gameOverText);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}