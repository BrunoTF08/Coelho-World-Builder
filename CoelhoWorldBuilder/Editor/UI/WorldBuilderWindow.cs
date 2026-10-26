using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CoelhoLabs.WorldBuilder.Editor
{
    public sealed class WorldBuilderWindow : EditorWindow
    {
        private ScenePainter painter;
        private VisualElement content;
        private Label status;
        private Button activate;
        private BuilderSettings Settings => BuilderSettings.instance;
        [MenuItem("Tools/Coelho Labs/Easy Level Design Tool")]
        public static void Open() { GetWindow<WorldBuilderWindow>("Easy Level Design Tool"); }
        private void OnEnable() { painter = new ScenePainter(); BuilderLog.Changed += UpdateStatus; }
        private void OnDisable()
        {
            if (content != null) foreach (var browser in content.Query<PrefabLibraryView>().ToList()) browser.Dispose();
            painter?.Dispose(); painter = null; BuilderLog.Changed -= UpdateStatus; Settings.Persist();
        }
        private void UpdateStatus(string message)
        { if (status != null) status.text = message; UpdateActivation(); }
        private void UpdateActivation()
        { if (activate != null) activate.text = painter != null && painter.Active ? "Desativar pintura" : "Ativar pintura"; }
        public void CreateGUI()
        {
            titleContent = new GUIContent("Easy Level Design Tool");
            minSize = new Vector2(520, 420);
            rootVisualElement.Clear();
            var header = new Toolbar();
            var title = new Label("◉ COELHO LABS  /  Easy Level Design Tool  1.3.1");
            title.style.flexGrow = 1; title.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(title);
            header.Add(new ToolbarButton(() => ShowPage("Settings")) { text = "Configurações" });
            header.Add(new ToolbarButton(ShowHelp) { text = "Ajuda" });
            rootVisualElement.Add(header);
            var body = new VisualElement(); body.style.flexDirection = FlexDirection.Row; body.style.flexGrow = 1;
            var navigation = new ScrollView(); navigation.style.width = 155; navigation.style.flexShrink = 0;
            foreach (string page in new[] { "Dashboard", "Smart Grid & Directional Placement", "Placement & Smart Grid", "Prefab Painter", "Grade", "Linhas verdes", "Modular Builder", "Road Builder", "City Generator", "Destruction Tools", "Rule Editor", "Asset Library", "Optimization", "Settings" })
            {
                string captured = page;
                navigation.Add(new Button(() => ShowPage(captured)) { text = page, tooltip = "Abrir " + page });
            }
            body.Add(navigation);
            content = new ScrollView(); content.style.flexGrow = 1;
            content.style.paddingLeft = 12; content.style.paddingRight = 12;
            body.Add(content); rootVisualElement.Add(body);
            status = new Label(BuilderLog.Last); status.style.whiteSpace = WhiteSpace.Normal;
            status.style.paddingTop = 8; status.style.paddingBottom = 8; rootVisualElement.Add(status);
            ShowPage("Dashboard");
        }
        private void Heading(string text)
        {
            var label = new Label(text); label.style.fontSize = 20; label.style.marginTop = 12; label.style.marginBottom = 12;
            content.Add(label);
        }
        private void Note(string text) { content.Add(new HelpBox(text, HelpBoxMessageType.Info)); }
        private void Changed() { Settings.Sanitize(); Settings.Persist(); SceneView.RepaintAll(); }
        private void ShowPage(string page)
        {
            painter.SettingsChanged = null;
            foreach (var browser in content.Query<PrefabLibraryView>().ToList()) browser.Dispose();
            content.Clear(); activate = null; Heading(page);
            if (page == "Prefab Painter" || page == "Placement & Smart Grid" || page == "Smart Grid & Directional Placement") { SmartPlacementPanel.Build(content, painter); return; }
            if (page == "Asset Library") { content.Add(new PrefabLibraryView(painter)); return; }
            if (page == "Grade")
            {
                painter.SetActive(false);
                Note("Grade verde sobre o cenário, como uma malha de referência. Crie a grade e ajuste o tamanho das células, quantidade de linhas, cores e visibilidade no Inspector. Use Move/Rotate para ajustar posição, altura e orientação. Ative Gizmos na Scene View.");
                content.Add(new Button(WorldGridEditor.Create) { text = "+ Criar grade verde" });
                return;
            }
            if (page == "Linhas verdes")
            {
                painter.SetActive(false);
                Note("Crie linhas-guia editáveis. Selecione uma linha na Hierarchy e use o Inspector para adicionar/remover pontos, fechar o traçado e ajustar a cor e espessura. Arraste os pontos na Scene View. Ative Gizmos para visualizar todas as linhas. Salve a cena para persistir.");
                content.Add(new Button(GuideLineEditor.Create) { text = "+ Criar linha verde" });
                return;
            }
            if (page == "Settings")
            {
                var seed = new IntegerField("Seed") { value = Settings.seed };
                seed.RegisterValueChangedCallback(e => { Settings.seed = e.newValue; Changed(); }); content.Add(seed);
                var mask = new LayerMaskField("Layers de superfície", Settings.layers);
                mask.RegisterValueChangedCallback(e => { Settings.layers = e.newValue; Changed(); }); content.Add(mask);
                Note("Preferências por projeto em UserSettings/CoelhoWorldBuilder.asset. A seed reproduz a sequência quando os prefabs, superfícies e gestos são os mesmos.");
                return;
            }
            if (page == "Dashboard")
            {
                Note("Primeira entrega: fundação e pintura básica. Os módulos avançados estão planejados para fases posteriores.");
                content.Add(new Button(() => ShowPage("Prefab Painter")) { text = "Abrir Prefab Painter" });
                content.Add(new Button(DemoBuilder.Create) { text = "Criar demonstração em nova cena aditiva" });
                content.Add(new Button(ShowHelp) { text = "Abrir guia local" });
                return;
            }
            Note("Planejado para uma próxima fase. Este módulo ainda não está implementado. A pintura básica utiliza a lista de prefabs salva nas preferências do projeto.");
        }
        private static void ShowHelp()
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(WorldBuilderWindow).Assembly);
            if (package != null) EditorUtility.OpenWithDefaultApp(Path.Combine(package.resolvedPath, "Documentation", "GettingStarted.md"));
            else EditorUtility.DisplayDialog("Easy Level Design Tool", "Consulte Documentation/GettingStarted.md no pacote.", "OK");
        }
    }
}
