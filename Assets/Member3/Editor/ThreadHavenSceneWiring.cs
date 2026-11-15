#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Member3.Data;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Editor utility that creates and wires the Member 3 UI in the checked-in scenes.</summary>
public static class ThreadHavenSceneWiring
{
    private const string MenuScene = "Assets/Member3/UI/MainMenuScene.unity";
    private const string ShowroomScene = "Assets/Member1/Scenes/ThreadHaven_Showroom.unity";
    private const string ProductJson = "Assets/Member3/Products/sample-products.json";
    // Editorial retail palette: warm paper, charcoal typography, olive and restrained brass.
    private static readonly Color Ink = new Color32(36, 39, 35, 255);
    private static readonly Color Panel = new Color32(242, 239, 231, 255);
    private static readonly Color Teal = new Color32(83, 99, 76, 255);
    private static readonly Color Gold = new Color32(163, 128, 78, 255);
    private static readonly Color Cream = new Color32(250, 248, 243, 255);
    private static readonly Color Muted = new Color32(105, 108, 101, 255);
    private static readonly Color CardSurface = new Color32(252, 251, 247, 255);
    private static readonly Color Line = new Color32(219, 215, 205, 255);
    private static TMP_FontAsset font;

    [MenuItem("ThreadHaven/Wire Menu and Showroom UI")]
    public static void WireAllScenes()
    {
        EnsureTextMeshProResources();
        BuildMenuScene();
        BuildShowroomScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(MenuScene, true), new EditorBuildSettingsScene(ShowroomScene, true) };
        EditorSceneManager.OpenScene(MenuScene, OpenSceneMode.Single);
        Debug.Log("ThreadHaven UI wiring complete. Open the MainMenuScene and press Play to verify.");
    }



    [MenuItem("ThreadHaven/Run Main Menu in Play Mode")]
    public static void RunMainMenuInPlayMode()
    {
        EditorSceneManager.OpenScene(MenuScene, OpenSceneMode.Single);
        EditorApplication.isPlaying = true;
    }

    [MenuItem("ThreadHaven/Build macOS App")]
    public static void BuildMacOSApp()
    {
        string output = "/Users/dishimukati/Documents/Codex/2026-11-13/member-3-20-git-commits-phase/outputs/Submission_Package/Build/ThreadHaven.app";
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
            locationPathName = output,
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log("macOS build result: " + report.summary.result + ", errors: " + report.summary.totalErrors + ", output: " + output);
    }

    [MenuItem("ThreadHaven/Capture Screenshot/Main Menu")]
    public static void CaptureMainMenuScreenshot() => CaptureRuntimeScreenshot("01-main-menu.png");

    [MenuItem("ThreadHaven/Capture Screenshot/Showroom")]
    public static void CaptureShowroomScreenshot() => CaptureRuntimeScreenshot("02-showroom.png");

    [MenuItem("ThreadHaven/Capture Screenshot/Product Inspection")]
    public static void CaptureInspectionScreenshot() => CaptureRuntimeScreenshot("03-product-inspection.png");

    [MenuItem("ThreadHaven/Capture Screenshot/Catalogue")]
    public static void CaptureCatalogueScreenshot() => CaptureRuntimeScreenshot("04-catalogue.png");

    private static void CaptureRuntimeScreenshot(string filename)
    {
        if (!EditorApplication.isPlaying)
        {
            Debug.LogWarning("Enter Play Mode and show the requested view before capturing a real Unity runtime screenshot.");
            return;
        }
        Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
        string folder = "/Users/dishimukati/Documents/Codex/2026-11-13/member-3-20-git-commits-phase/outputs/Submission_Package/Unity_Screenshots";
        Directory.CreateDirectory(folder);
        File.WriteAllBytes(Path.Combine(folder, filename), image.EncodeToPNG());
        Object.DestroyImmediate(image);
        Debug.Log("Captured Unity runtime screenshot: " + Path.Combine(folder, filename));
    }

    private static void EnsureTextMeshProResources()
    {
        font = TMP_Settings.defaultFontAsset;
        if (font != null) return;
        string[] hits = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
        if (hits.Length > 0)
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(hits[0]));
        if (font == null)
        {
            EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
            AssetDatabase.Refresh();
            font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                hits = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
                if (hits.Length > 0)
                    font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(hits[0]));
            }
        }
        if (font == null)
            throw new InvalidOperationException("TextMeshPro's default font is missing. Import TMP Essential Resources in Window > TextMeshPro, then run this menu again.");
    }

    private static void BuildMenuScene()
    {
        Scene scene = EditorSceneManager.OpenScene(MenuScene, OpenSceneMode.Single);
        Canvas canvas = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).FirstOrDefault(c => c.name == "MainMenuCanvas");
        if (canvas == null)
            canvas = CreateCanvas("MainMenuCanvas");
        else ConfigureCanvas(canvas);
        EnsureEventSystem();
        EnsureMenuCamera();
        RemoveGeneratedChildren(canvas.transform);

        RectTransform root = PanelObject("ThreadHavenMainMenu", canvas.transform, new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(1920,1080), Panel);
        RectTransform bg = FullPanel("Warm paper background", root, Panel);
        AddSoftAccent(bg);
        GameObject home = PanelObject("Home Panel", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear).gameObject;
        GameObject categories = PanelObject("Category Panel", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Panel).gameObject;
        GameObject catalogue = PanelObject("Catalogue Panel", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Panel).gameObject;
        home.transform.SetAsLastSibling();
        categories.SetActive(false); catalogue.SetActive(false);

        Label(home.transform, "A VIRTUAL GARMENT SHOWROOM", new Vector2(0.09f,0.82f), new Vector2(0.62f,0.87f), 17, Teal, true, TextAlignmentOptions.Left);
        Label(home.transform, "ThreadHaven", new Vector2(0.09f,0.69f), new Vector2(0.68f,0.81f), 62, Ink, true, TextAlignmentOptions.Left);
        Label(home.transform, "Explore the collection.\nExperience every detail.", new Vector2(0.09f,0.53f), new Vector2(0.67f,0.67f), 31, Ink, false, TextAlignmentOptions.Left);
        Label(home.transform, "A considered edit of garments, brought together in an\ninteractive 3D showroom.", new Vector2(0.09f,0.43f), new Vector2(0.67f,0.51f), 20, Muted, false, TextAlignmentOptions.Left);
        Button enter = ButtonObject(home.transform, "Enter Showroom", new Vector2(0.09f,0.30f), new Vector2(0.36f,0.38f), Ink, 20);
        Button explore = ButtonObject(home.transform, "Explore Products", new Vector2(0.09f,0.19f), new Vector2(0.36f,0.27f), CardSurface, 19);
        Button exit = ButtonObject(home.transform, "Exit", new Vector2(0.09f,0.09f), new Vector2(0.36f,0.16f), Panel, 17);
        AddEditorialHero(home.transform);
        Label(home.transform, "GARMENTS  /  PRODUCT INFORMATION  /  VIRTUAL EXPLORATION", new Vector2(0.09f,0.025f), new Vector2(0.91f,0.07f), 12, Muted, false, TextAlignmentOptions.Left);

        BuildCategoryPanel(categories.transform, home, catalogue);
        ProductInformationPanel info = BuildCataloguePanel(catalogue.transform, home, categories, out List<ProductCard> cards, out TMP_InputField search, out Button clear, out TMP_Text result, out GameObject empty);
        ProductCatalogueNavigation nav = canvas.gameObject.GetComponent<ProductCatalogueNavigation>() ?? canvas.gameObject.AddComponent<ProductCatalogueNavigation>();
        SetFields(nav, ("homePanel",home), ("categoryPanel",categories), ("cataloguePanel",catalogue), ("categoryHomeButton",FindNamedButton(categories.transform,"Home")), ("categoryViewProductsButton",FindNamedButton(categories.transform,"View Products")), ("catalogueHomeButton",FindNamedButton(catalogue.transform,"Home")), ("catalogueBackButton",FindNamedButton(catalogue.transform,"Back")));
        ProductCatalogue pc = canvas.gameObject.GetComponent<ProductCatalogue>() ?? canvas.gameObject.AddComponent<ProductCatalogue>();
        ConfigureCatalogue(pc, info, cards, search, clear, result, empty);
        MainMenuButtons menuButtons = home.GetComponent<MainMenuButtons>() ?? home.AddComponent<MainMenuButtons>();
        SetFields(menuButtons, ("enterShowroomButton",enter), ("exploreProductsButton",explore), ("exitButton",exit), ("enterShowroomLabel",enter.GetComponentInChildren<TMP_Text>()), ("exploreProductsLabel",explore.GetComponentInChildren<TMP_Text>()), ("exitLabel",exit.GetComponentInChildren<TMP_Text>()), ("enterButtonColor",Ink), ("exploreButtonColor",CardSurface), ("exitButtonColor",Line), ("productCataloguePanel",catalogue), ("mainMenuPanel",home));

        SaveScene(scene, "Main menu UI");
    }

    private static void BuildShowroomScene()
    {
        Scene scene = EditorSceneManager.OpenScene(ShowroomScene, OpenSceneMode.Single);
        EnsureEventSystem();
        Camera sceneCamera = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault(c => c.name == "Main Camera");
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Member2/Prefabs/Player.prefab");
        if (playerPrefab == null) throw new InvalidOperationException("Member 2 Player.prefab was not found.");
        GameObject player = Object.FindObjectsByType<FirstPersonController>(FindObjectsSortMode.None).FirstOrDefault()?.gameObject;
        if (player == null)
        {
            player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            Undo.RegisterCreatedObjectUndo(player, "Add showroom player");
            player.name = "Player - Member 2 Controls";
        }
        player.transform.SetPositionAndRotation(new Vector3(0f, 0.05f, -7.5f), Quaternion.identity);
        FirstPersonController movement = player.GetComponent<FirstPersonController>();
        ProductInteractor interactor = player.GetComponent<ProductInteractor>();
        ProductInspector inspector = player.GetComponent<ProductInspector>();
        Camera playerCamera = player.GetComponentInChildren<Camera>(true);
        if (movement == null || interactor == null || inspector == null || playerCamera == null)
            throw new InvalidOperationException("Player.prefab is missing one of its movement, interaction or camera components.");
        SetFields(inspector, ("playerCamera",playerCamera), ("movement",movement), ("interactor",interactor));
        SetFields(interactor, ("playerCamera",playerCamera), ("showDebug",false));
        if (sceneCamera != null && sceneCamera.gameObject != playerCamera.gameObject)
        {
            sceneCamera.enabled = false;
            AudioListener oldListener = sceneCamera.GetComponent<AudioListener>();
            if (oldListener != null) oldListener.enabled = false;
        }

        AttachProductMetadata("t shirt_1", "Classic Cotton Crew Neck T-Shirt", "Shirts", "A breathable everyday crew neck shirt made from soft cotton jersey.", "Material: 100% cotton\nFit: Regular\nSizes: S, M, L, XL\nCare: Machine wash cold");
        AttachProductMetadata("t shirt_2", "Oxford Button-Down Shirt", "Shirts", "A durable woven shirt with a button-down collar for work or casual wear.", "Material: Cotton Oxford cloth\nFit: Regular\nSizes: S, M, L, XL\nCare: Machine wash cold; iron warm");
        AttachProductMetadata("Product_Shorts", "Everyday Garment Shorts", "Trousers", "A casual garment displayed in the showroom for interactive inspection.", "Category: Trousers\nFit: Regular\nCare: Follow garment label");
        AttachProductMetadata("SHIRT LONG SLEEVE", "Long Sleeve Shirt", "Shirts", "A long-sleeve garment displayed in the showroom.", "Category: Shirts\nFit: Regular\nCare: Follow garment label");

        Canvas canvas = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).FirstOrDefault(c => c.name == "ThreadHaven Showroom UI");
        if (canvas == null) canvas = CreateCanvas("ThreadHaven Showroom UI"); else ConfigureCanvas(canvas);
        RemoveGeneratedChildren(canvas.transform);
        RectTransform root = PanelObject("Showroom UI Root", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear);
        GameObject home = PanelObject("Showroom Home Panel", root, new Vector2(0.035f,0.05f), new Vector2(0.43f,0.37f), Vector2.zero, Vector2.zero, new Color32(36,39,35,235)).gameObject;
        GameObject categories = PanelObject("Category Panel", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Panel).gameObject;
        GameObject catalogue = PanelObject("Catalogue Panel", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Panel).gameObject;
        Label(home.transform, "THREADHAVEN  /  SHOWROOM", new Vector2(0.08f,0.72f), new Vector2(0.92f,0.91f), 22, Cream, true, TextAlignmentOptions.Left);
        Label(home.transform, "WASD move · Mouse look · Shift sprint\nE inspect · Drag rotate · Scroll zoom · Q exit", new Vector2(0.08f,0.45f), new Vector2(0.92f,0.70f), 15, new Color32(224,222,214,255), false, TextAlignmentOptions.Left);
        Button browse = ButtonObject(home.transform, "Explore Products", new Vector2(0.08f,0.24f), new Vector2(0.54f,0.40f), Teal, 17);
        Button resume = ButtonObject(home.transform, "Resume Showroom", new Vector2(0.57f,0.24f), new Vector2(0.92f,0.40f), new Color32(62,67,60,255), 15);
        Button toMenu = ButtonObject(home.transform, "Main Menu", new Vector2(0.08f,0.07f), new Vector2(0.32f,0.20f), new Color32(72,64,55,255), 14);
        BuildCategoryPanel(categories.transform, home, catalogue);
        ProductInformationPanel info = BuildCataloguePanel(catalogue.transform, home, categories, out List<ProductCard> cards, out TMP_InputField search, out Button clear, out TMP_Text result, out GameObject empty);
        ProductCatalogueNavigation nav = canvas.gameObject.GetComponent<ProductCatalogueNavigation>() ?? canvas.gameObject.AddComponent<ProductCatalogueNavigation>();
        SetFields(nav, ("homePanel",home), ("categoryPanel",categories), ("cataloguePanel",catalogue), ("categoryHomeButton",FindNamedButton(categories.transform,"Home")), ("categoryViewProductsButton",FindNamedButton(categories.transform,"View Products")), ("catalogueHomeButton",FindNamedButton(catalogue.transform,"Home")), ("catalogueBackButton",FindNamedButton(catalogue.transform,"Back")));
        ProductCatalogue pc = canvas.gameObject.GetComponent<ProductCatalogue>() ?? canvas.gameObject.AddComponent<ProductCatalogue>();
        ConfigureCatalogue(pc, info, cards, search, clear, result, empty);
        InspectModeUIBridge bridge = canvas.gameObject.GetComponent<InspectModeUIBridge>() ?? canvas.gameObject.AddComponent<InspectModeUIBridge>();
        SetFields(bridge, ("informationPanel",info));
        ShowroomUIActions actions = canvas.gameObject.GetComponent<ShowroomUIActions>() ?? canvas.gameObject.AddComponent<ShowroomUIActions>();
        SetFields(actions, ("navigation",nav), ("playerMovement",movement), ("homePanel",home), ("categoryPanel",categories), ("cataloguePanel",catalogue), ("informationPanel",info));
        UnityEventTools.AddPersistentListener(browse.onClick, actions.OpenCategories);
        UnityEventTools.AddPersistentListener(resume.onClick, actions.ResumeShowroom);
        UnityEventTools.AddPersistentListener(toMenu.onClick, actions.ReturnToMenu);

        SaveScene(scene, "Showroom player and Member 3 UI");
    }

    private static void BuildCategoryPanel(Transform parent, GameObject home, GameObject catalogue)
    {
        Label(parent, "Shop by category", new Vector2(0.08f,0.77f), new Vector2(0.92f,0.90f), 34, Ink, true, TextAlignmentOptions.Left);
        Label(parent, "Explore the collection by garment type.", new Vector2(0.08f,0.70f), new Vector2(0.92f,0.76f), 18, Muted, false, TextAlignmentOptions.Left);
        Button all = ButtonObject(parent, "All Products", new Vector2(0.08f,0.53f), new Vector2(0.42f,0.64f), Teal, 20);
        Button shirts = ButtonObject(parent, "Shirts", new Vector2(0.55f,0.53f), new Vector2(0.92f,0.64f), CardSurface, 20);
        Button jackets = ButtonObject(parent, "Jackets", new Vector2(0.08f,0.37f), new Vector2(0.42f,0.48f), CardSurface, 20);
        Button trousers = ButtonObject(parent, "Trousers", new Vector2(0.55f,0.37f), new Vector2(0.92f,0.48f), CardSurface, 20);
        Button dresses = ButtonObject(parent, "Dresses", new Vector2(0.08f,0.21f), new Vector2(0.42f,0.32f), CardSurface, 20);
        Button view = ButtonObject(parent, "View Products", new Vector2(0.55f,0.21f), new Vector2(0.92f,0.32f), Ink, 19);
        Button homeButton = ButtonObject(parent, "Home", new Vector2(0.08f,0.07f), new Vector2(0.27f,0.16f), Panel, 16);
        CategorySelectionPanel selector = parent.GetComponent<CategorySelectionPanel>() ?? parent.gameObject.AddComponent<CategorySelectionPanel>();
        SetFields(selector, ("categoryPanel",parent.gameObject), ("allButton",all), ("shirtsButton",shirts), ("jacketsButton",jackets), ("trousersButton",trousers), ("dressesButton",dresses), ("allLabel",all.GetComponentInChildren<TMP_Text>()), ("shirtsLabel",shirts.GetComponentInChildren<TMP_Text>()), ("jacketsLabel",jackets.GetComponentInChildren<TMP_Text>()), ("trousersLabel",trousers.GetComponentInChildren<TMP_Text>()), ("dressesLabel",dresses.GetComponentInChildren<TMP_Text>()));
    }

    private static ProductInformationPanel BuildCataloguePanel(Transform parent, GameObject home, GameObject categories,
        out List<ProductCard> cards, out TMP_InputField search, out Button clear, out TMP_Text result, out GameObject empty)
    {
        Label(parent, "The collection", new Vector2(0.04f,0.88f), new Vector2(0.72f,0.97f), 34, Ink, true, TextAlignmentOptions.Left);
        Button back = ButtonObject(parent, "Back", new Vector2(0.76f,0.90f), new Vector2(0.85f,0.97f), CardSurface, 15);
        Button homeButton = ButtonObject(parent, "Home", new Vector2(0.87f,0.90f), new Vector2(0.96f,0.97f), Ink, 15);
        search = SearchInput(parent, "Search garment by name...", new Vector2(0.04f,0.81f), new Vector2(0.54f,0.87f));
        clear = ButtonObject(parent, "Clear", new Vector2(0.56f,0.81f), new Vector2(0.65f,0.87f), Panel, 14);
        result = Label(parent, "8 products", new Vector2(0.68f,0.81f), new Vector2(0.96f,0.87f), 15, Muted, false, TextAlignmentOptions.Left);
        RectTransform cardsRoot = PanelObject("Product Cards", parent, new Vector2(0.04f,0.13f), new Vector2(0.62f,0.78f), Vector2.zero, Vector2.zero, new Color32(231,228,220,255));
        GridLayoutGroup grid = cardsRoot.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(310,130); grid.spacing = new Vector2(16,14); grid.padding = new RectOffset(16,16,16,16); grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 2; grid.childAlignment = TextAnchor.UpperLeft;
        ContentSizeFitter fitter = cardsRoot.gameObject.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(ProductJson);
        if (json == null) throw new InvalidOperationException("sample-products.json is missing.");
        ProductCatalogueData data = JsonUtility.FromJson<ProductCatalogueData>(json.text);
        if (data == null || data.products == null) throw new InvalidOperationException("sample-products.json could not be read.");
        cards = new List<ProductCard>();
        foreach (Member3.Data.Product record in data.products)
        {
            GameObject cardObject = new GameObject("Card " + record.Id, typeof(RectTransform), typeof(Image), typeof(Button));
            cardObject.transform.SetParent(cardsRoot, false);
            cardObject.GetComponent<Image>().color = CardSurface;
            Button button = cardObject.GetComponent<Button>(); button.transition = Selectable.Transition.ColorTint;
            TMP_Text name = Label(cardObject.transform, record.Name, new Vector2(0.06f,0.37f), new Vector2(0.94f,0.88f), 18, Ink, true, TextAlignmentOptions.Left);
            TMP_Text badge = Label(cardObject.transform, record.Category.ToUpperInvariant(), new Vector2(0.06f,0.08f), new Vector2(0.55f,0.29f), 13, Teal, true, TextAlignmentOptions.Left);
            TMP_Text sku = Label(cardObject.transform, record.Id, new Vector2(0.62f,0.08f), new Vector2(0.94f,0.29f), 12, Muted, false, TextAlignmentOptions.Right);
            ProductCard card = cardObject.AddComponent<ProductCard>();
            SetFields(card, ("productNameText",name), ("categoryBadgeText",badge), ("priceOrSkuText",sku), ("cardButton",button), ("cardBackground",cardObject.GetComponent<Image>()), ("productId",record.Id), ("productName",record.Name), ("productCategory",record.Category));
            cards.Add(card);
        }
        empty = Label(parent, "No products match your search.", new Vector2(0.08f,0.44f), new Vector2(0.58f,0.52f), 19, Ink, true, TextAlignmentOptions.Center).gameObject;
        empty.SetActive(false);
        GameObject infoRoot = PanelObject("Product Information Panel", parent, new Vector2(0.66f,0.13f), new Vector2(0.96f,0.78f), Vector2.zero, Vector2.zero, CardSurface).gameObject;
        Label(infoRoot.transform, "PRODUCT DETAILS", new Vector2(0.08f,0.86f), new Vector2(0.92f,0.97f), 17, Teal, true, TextAlignmentOptions.Left);
        TMP_Text productName = Label(infoRoot.transform, "Select a product", new Vector2(0.08f,0.72f), new Vector2(0.92f,0.85f), 22, Ink, true, TextAlignmentOptions.Left);
        TMP_Text category = Label(infoRoot.transform, "Category: —", new Vector2(0.08f,0.65f), new Vector2(0.92f,0.72f), 14, Teal, true, TextAlignmentOptions.Left);
        TMP_Text id = Label(infoRoot.transform, "Product ID: —", new Vector2(0.08f,0.59f), new Vector2(0.92f,0.65f), 13, Muted, false, TextAlignmentOptions.Left);
        TMP_Text description = Label(infoRoot.transform, "Choose a product card to view its details.", new Vector2(0.08f,0.39f), new Vector2(0.92f,0.57f), 16, Ink, false, TextAlignmentOptions.TopLeft);
        TMP_Text specs = Label(infoRoot.transform, "Specifications will appear here.", new Vector2(0.08f,0.13f), new Vector2(0.92f,0.37f), 15, Ink, false, TextAlignmentOptions.TopLeft);
        Image img = PanelObject("Image Placeholder", infoRoot.transform, new Vector2(0.64f,0.43f), new Vector2(0.92f,0.60f), Vector2.zero, Vector2.zero, Panel).GetComponent<Image>();
        TMP_Text placeholder = Label(infoRoot.transform, "GARMENT\nPREVIEW", new Vector2(0.65f,0.44f), new Vector2(0.91f,0.59f), 14, Gold, true, TextAlignmentOptions.Center);
        Button infoBack = ButtonObject(infoRoot.transform, "Back", new Vector2(0.08f,0.04f), new Vector2(0.34f,0.11f), Panel, 13);
        Button infoHome = ButtonObject(infoRoot.transform, "Home", new Vector2(0.38f,0.04f), new Vector2(0.64f,0.11f), Teal, 13);
        Button close = ButtonObject(infoRoot.transform, "Close", new Vector2(0.68f,0.04f), new Vector2(0.92f,0.11f), Ink, 13);
        GameObject infoHost = parent.GetComponentInParent<Canvas>().gameObject;
        ProductInformationPanel info = infoHost.GetComponent<ProductInformationPanel>() ?? infoHost.AddComponent<ProductInformationPanel>();
        SetFields(info, ("panelRoot",infoRoot), ("productImage",img), ("imagePlaceholder",placeholder.gameObject), ("productNameText",productName), ("categoryText",category), ("productIdText",id), ("descriptionText",description), ("specificationsText",specs), ("backButton",infoBack), ("homeButton",infoHome), ("closeButton",close));
        infoRoot.SetActive(false);
        return info;
    }

    private static void ConfigureCatalogue(ProductCatalogue pc, ProductInformationPanel info, List<ProductCard> cards, TMP_InputField search, Button clear, TMP_Text result, GameObject empty)
    {
        SetFields(pc, ("productDataJson",AssetDatabase.LoadAssetAtPath<TextAsset>(ProductJson)), ("informationPanel",info), ("productCards",cards.ToArray()), ("loadFromBackend",false), ("productApiUrl","http://127.0.0.1:3000/api/products"), ("searchInput",search), ("clearSearchButton",clear), ("resultCountText",result), ("noResultsMessage",empty));
    }

    private static void AttachProductMetadata(string targetName, string productName, string category, string description, string specs)
    {
        GameObject target = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).FirstOrDefault(g => string.Equals(g.name.Trim(), targetName, StringComparison.OrdinalIgnoreCase));
        if (target == null) { Debug.LogWarning("No world garment found named " + targetName + ". Its UI catalogue sample remains available."); return; }
        Product product = target.GetComponent<Product>();
        if (product == null) product = Undo.AddComponent<Product>(target);
        SetFields(product, ("productName",productName), ("category",category), ("description",description), ("specifications",specs));
    }

    private static TMP_InputField SearchInput(Transform parent, string placeholderText, Vector2 min, Vector2 max)
    {
        RectTransform rt = PanelObject("Product Search", parent, min, max, Vector2.zero, Vector2.zero, new Color32(239,239,232,255));
        TMP_InputField input = rt.gameObject.AddComponent<TMP_InputField>();
        RectTransform textArea = PanelObject("Text Area", rt, new Vector2(0.025f,0.08f), new Vector2(0.975f,0.92f), Vector2.zero, Vector2.zero, Color.clear);
        textArea.gameObject.AddComponent<RectMask2D>();
        TMP_Text text = Label(textArea, string.Empty, Vector2.zero, Vector2.one, 17, Ink, false, TextAlignmentOptions.Left);
        TMP_Text placeholder = Label(textArea, placeholderText, Vector2.zero, Vector2.one, 17, new Color32(107,120,117,255), false, TextAlignmentOptions.Left);
        input.textComponent = text;
        input.textViewport = textArea;
        input.placeholder = placeholder;
        input.targetGraphic = rt.GetComponent<Image>();
        input.lineType = TMP_InputField.LineType.SingleLine;
        return input;
    }

    private static Canvas CreateCanvas(string name)
    {
        GameObject g = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = g.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        ConfigureCanvas(canvas);
        return canvas;
    }

    private static void ConfigureCanvas(Canvas canvas)
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.pixelPerfect = true; canvas.sortingOrder = 50;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect != null) { canvasRect.localScale = Vector3.one; canvasRect.anchoredPosition = Vector2.zero; }
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>() ?? canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = 0.5f;
        if (canvas.GetComponent<GraphicRaycaster>() == null) canvas.gameObject.AddComponent<GraphicRaycaster>();
    }

    private static void EnsureEventSystem()
    {
        EventSystem system = Object.FindFirstObjectByType<EventSystem>();
        if (system == null) system = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        if (system.GetComponent<InputSystemUIInputModule>() == null)
        {
            system.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    private static void EnsureMenuCamera()
    {
        Camera camera = Object.FindAnyObjectByType<Camera>();
        if (camera != null) return;
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 0f, -10f), Quaternion.identity);
        Camera menuCamera = cameraObject.GetComponent<Camera>();
        menuCamera.clearFlags = CameraClearFlags.SolidColor;
        menuCamera.backgroundColor = Panel;
        menuCamera.orthographic = true;
        menuCamera.orthographicSize = 5f;
    }

    private static void RemoveGeneratedChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;
            if (child.name.StartsWith("ThreadHaven") || child.name == "Showroom UI Root")
                Undo.DestroyObjectImmediate(child);
        }
    }

    private static RectTransform FullPanel(string name, RectTransform parent, Color color) => PanelObject(name,parent,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,color);

    private static RectTransform PanelObject(string name, Transform parent, Vector2 min, Vector2 max, Vector2 position, Vector2 size, Color color)
    {
        GameObject g = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rt = g.GetComponent<RectTransform>(); rt.SetParent(parent,false); rt.anchorMin=min; rt.anchorMax=max; rt.anchoredPosition=position; rt.sizeDelta=size;
        Image image=g.GetComponent<Image>(); image.color=color; image.raycastTarget=color.a>0.02f;
        return rt;
    }

    private static TMP_Text Label(Transform parent, string content, Vector2 min, Vector2 max, float size, Color color, bool bold, TextAlignmentOptions align)
    {
        GameObject g=new GameObject("Text - "+(string.IsNullOrEmpty(content)?"Input":content.Substring(0,Math.Min(content.Length,30))),typeof(RectTransform),typeof(TextMeshProUGUI));
        RectTransform rt=g.GetComponent<RectTransform>();rt.SetParent(parent,false);rt.anchorMin=min;rt.anchorMax=max;rt.offsetMin=Vector2.zero;rt.offsetMax=Vector2.zero;
        TMP_Text text=g.GetComponent<TMP_Text>();text.text=content;text.font=font;text.fontSize=size;text.color=color;text.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;text.alignment=align;text.textWrappingMode=TextWrappingModes.Normal;text.raycastTarget=false;
        return text;
    }

    private static Button ButtonObject(Transform parent,string label,Vector2 min,Vector2 max,Color color,float size)
    {
        RectTransform rt=PanelObject(label,parent,min,max,Vector2.zero,Vector2.zero,color);
        Button b=rt.gameObject.AddComponent<Button>();b.targetGraphic=rt.GetComponent<Image>();b.transition=Selectable.Transition.ColorTint;
        ColorBlock cb=b.colors;cb.normalColor=color;cb.highlightedColor=Color.Lerp(color,Color.white,0.14f);cb.pressedColor=Color.Lerp(color,Color.black,0.16f);b.colors=cb;
        float luminance = color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f;
        Color labelColor = luminance < 0.46f ? Cream : Ink;
        TMP_Text t=Label(rt,label,Vector2.zero,Vector2.one,size,labelColor,true,TextAlignmentOptions.Center);
        t.margin=new Vector4(10,4,10,4);
        return b;
    }

    private static Button FindNamedButton(Transform parent,string exact)
    {
        return parent.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.gameObject.name==exact);
    }

    private static void AddSoftAccent(RectTransform parent)
    {
        RectTransform stripe=PanelObject("Gold edge",parent,new Vector2(0.0f,0.0f),new Vector2(0.004f,1.0f),Vector2.zero,Vector2.zero,Gold);
        stripe.SetAsLastSibling();
    }

    private static void AddEditorialHero(Transform parent)
    {
        RectTransform hero = PanelObject("Editorial collection card", parent, new Vector2(0.70f,0.16f), new Vector2(0.91f,0.83f), Vector2.zero, Vector2.zero, new Color32(226,222,212,255));
        PanelObject("Editorial brass rule", hero, new Vector2(0.08f,0.82f), new Vector2(0.92f,0.826f), Vector2.zero, Vector2.zero, Gold);
        Label(hero, "THE COLLECTION  /  01", new Vector2(0.08f,0.70f), new Vector2(0.92f,0.80f), 14, Teal, true, TextAlignmentOptions.Left);
        Label(hero, "Made to\nbe explored.", new Vector2(0.08f,0.39f), new Vector2(0.92f,0.66f), 38, Ink, true, TextAlignmentOptions.Left);
        Label(hero, "Garments, detail and craft\nin an immersive space.", new Vector2(0.08f,0.19f), new Vector2(0.92f,0.32f), 17, Muted, false, TextAlignmentOptions.Left);
        Label(hero, "THREADHAVEN  ·  VIRTUAL SHOWROOM", new Vector2(0.08f,0.07f), new Vector2(0.92f,0.13f), 10, Muted, false, TextAlignmentOptions.Left);
    }

    private static void SetFields(Object target, params (string name, object value)[] values)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        SerializedObject so=new SerializedObject(target);
        foreach(var pair in values)
        {
            SerializedProperty p=so.FindProperty(pair.name);
            if(p==null){Debug.LogWarning(target.GetType().Name+" has no serialized field '"+pair.name+"'.");continue;}
            if(pair.value is Object obj) p.objectReferenceValue=obj;
            else if(pair.value is bool boolean) p.boolValue=boolean;
            else if(pair.value is string text) p.stringValue=text;
            else if(pair.value is Color color) p.colorValue=color;
            else if(pair.value is ProductCard[] cards){p.arraySize=cards.Length;for(int i=0;i<cards.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=cards[i];}
            else if(pair.value is List<ProductCard> list){p.arraySize=list.Count;for(int i=0;i<list.Count;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=list[i];}
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        if (target is Component component && PrefabUtility.IsPartOfPrefabInstance(component))
            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        EditorUtility.SetDirty(target);
    }

    private static void SaveScene(Scene scene,string undoName)
    {
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Saved " + undoName + " in " + scene.path);
    }
}
#endif
