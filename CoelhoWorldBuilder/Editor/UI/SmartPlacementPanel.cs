using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CoelhoLabs.WorldBuilder.Editor
{
    internal static class SmartPlacementPanel
    {
        internal static void Build(VisualElement root, ScenePainter painter)
        {
            var s = BuilderSettings.instance;
            Action save = () => { s.Sanitize(); PrefabDimensionAnalyzer.ApplyAdaptiveGrid(s); s.Persist(); SceneView.RepaintAll(); };
            var library = new PrefabLibraryView(painter); root.Add(library);
            var settings = new VisualElement(); root.Add(settings);
            Action render = null;
            render = () =>
            {
                settings.Clear();
                var picker = new ObjectField("Prefab selecionado") { objectType = typeof(GameObject), allowSceneObjects = false, value = s.SelectedPrefab };
                picker.RegisterValueChangedCallback(e =>
                {
                    var prefab = e.newValue as GameObject;
                    if (prefab != null && (!PrefabUtility.IsPartOfPrefabAsset(prefab) || prefab.transform.parent != null))
                    { picker.SetValueWithoutNotify(s.SelectedPrefab); BuilderLog.Info("Selecione a raiz de um prefab do Project."); return; }
                    var store = PrefabLibrary.instance;
                    if (prefab != null) store.Add(store.Active ?? store.CreateCategory("Outros"), prefab);
                    painter.SelectPrefab(prefab);
                }); settings.Add(picker);
                var prefabObject = s.SelectedPrefab;
                if (prefabObject == null)
                {
                    Toggle(settings, "Show Grid", s.showGrid, value => { s.showGrid = value; save(); });
                    Vector2(settings, "Célula X/Z", s.gridCell, value => { s.gridCell = value; save(); });
                    Vector2(settings, "Área de exibição X/Z", s.gridArea, value => { s.gridArea = value; save(); });
                    Vector3(settings, "Grid Origin", s.gridOrigin, value => { s.gridOrigin = value; save(); });
                    Float(settings, "Grid Height", s.gridHeight, value => { s.gridHeight = value; save(); });
                    return;
                }
                bool newProfile = !s.profiles.Exists(p => p.guid == s.selectedPrefabGuid);
                var p = s.Profile(s.selectedPrefabGuid);
                bool hasBounds = PrefabDimensionAnalyzer.Analyze(prefabObject, out var bounds);
                if (PrefabDimensionAnalyzer.ApplyAdaptiveGrid(s)) s.Persist();
                Func<Vector3> dimensions = () => SmartPlacementMath.TransformBounds(bounds,
                    Matrix4x4.TRS(UnityEngine.Vector3.zero, Quaternion.Euler(p.rotation) * prefabObject.transform.rotation, UnityEngine.Vector3.Scale(prefabObject.transform.localScale, p.scale))).size;
                if (newProfile && hasBounds) { var d = dimensions(); p.cell = new Vector2(Mathf.Max(.01f, d.x), Mathf.Max(.01f, d.z)); save(); }
                var detected = new Label(); var current = new Label();
                FloatField widthField = null, lengthField = null;
                Toggle autoField = null, manualField = null;
                Action updateLabels = () =>
                {
                    var d = dimensions();
                    detected.text = hasBounds ? $"Bounds estimados X/Y/Z: {d.x:0.###} / {d.y:0.###} / {d.z:0.###}" : "Sem Renderer/Collider suportado: configure dimensões manualmente.";
                    current.text = $"Grade atual: {s.gridCell.x:0.###} × {s.gridCell.y:0.###} • origem fixa: {s.gridOrigin}";
                    widthField?.SetValueWithoutNotify(s.gridCell.x); lengthField?.SetValueWithoutNotify(s.gridCell.y);
                    autoField?.SetValueWithoutNotify(s.autoGridSize); manualField?.SetValueWithoutNotify(p.manualDimensions);
                };
                settings.Add(new Button(() => { save(); painter.SetActive(true); SceneView.lastActiveSceneView?.Focus(); }) { text = "Activate Placement" });
                settings.Add(new Button(() => painter.SetActive(false)) { text = "Stop Placement" });
                var live = new Label(); live.style.whiteSpace = WhiteSpace.Normal;
                Action updateLive = () => live.text = $"Última peça: {painter.LastReference} | Direção: {painter.CurrentDirection} | Snap: {(p.snap ? "ligado" : "desligado")} | Construção: {(painter.Active ? "ativa" : "parada")}";
                updateLive(); live.schedule.Execute(updateLive).Every(150); settings.Add(live);
                settings.Add(new Label("Ctrl Directional Snap — Roads / Floors"));
                Toggle(settings, "Enable Ctrl Directional Snap", s.ctrlDirectionalSnap, value => { s.ctrlDirectionalSnap = value; save(); });
                Enum(settings, "Snap Category", p.category, value => { p.category = (ModularCategory)value; save(); });
                Toggle(settings, "Edge Alignment (alinhar bordas laterais)", s.edgeAlignment, value => { s.edgeAlignment = value; save(); });
                Vector3(settings, "Snap Offset (mundo)", s.snapOffset, value => { s.snapOffset = value; save(); });
                Toggle(settings, "Usar pontos de conexão locais", p.useConnectionPoints, value => { p.useConnectionPoints = value; save(); });
                Vector3(settings, "Conexão Right (local)", p.rightConnection, value => { p.rightConnection = value; save(); });
                Vector3(settings, "Conexão Left (local)", p.leftConnection, value => { p.leftConnection = value; save(); });
                Vector3(settings, "Conexão Forward (local)", p.forwardConnection, value => { p.forwardConnection = value; save(); });
                Vector3(settings, "Conexão Back (local)", p.backConnection, value => { p.backConnection = value; save(); });
                Toggle(settings, "Show Snap Preview", s.showSnapPreview, value => { s.showSnapPreview = value; save(); });
                settings.Add(new HelpBox("Classifique ruas como Roads, pisos como Floors e decoração como Decorative. Ctrl encaixa à última peça da mesma categoria e só coloca por clique, mesmo em Paint. A direção segue os eixos horizontais da referência. Soltar Ctrl retorna ao modo normal.", HelpBoxMessageType.Info));
                Enum(settings, "Single / Paint", s.mode, value => { s.mode = (PlacementMode)value; save(); });
                Enum(settings, "Modo de Snap", s.snapMode, value => { s.snapMode = (SnapMode)value; save(); });
                Vector3(settings, "Rotação padrão (°)", p.rotation, value => { p.rotation = value; save(); updateLabels(); });
                Vector3(settings, "Escala padrão", p.scale, value => { p.scale = UnityEngine.Vector3.Max(value, UnityEngine.Vector3.one * .001f); save(); updateLabels(); });
                Vector3(settings, "Offset por prefab", p.offset, value => { p.offset = value; save(); });
                Float(settings, "Offset vertical global", s.offset, value => { s.offset = value; save(); });
                Toggle(settings, "Alinhar à superfície", p.alignToSurface, value => { p.alignToSurface = value; save(); });
                Enum(settings, "Âncora", p.anchorMode, value => { p.anchorMode = (AnchorMode)value; save(); });
                Vector3(settings, "Âncora local personalizada", p.customAnchor, value => { p.customAnchor = value; save(); });
                Toggle(settings, "Evitar sobreposição (Bounds/Colliders)", s.avoidOverlap, value => { s.avoidOverlap = value; save(); });
                Float(settings, "Espaçamento mínimo", s.spacing, value => { s.spacing = value; save(); });
                Float(settings, "Raio Paint", s.radius, value => { s.radius = value; save(); });
                var density = new SliderInt("Densidade por carimbo", 1, 100) { value = s.density, showInputField = true };
                density.RegisterValueChangedCallback(e => { s.density = e.newValue; save(); }); settings.Add(density);
                Vector2(settings, "Escala aleatória mín/máx", s.scale, value => { s.scale = value; save(); });
                Vector2(settings, "Rotação aleatória livre mín/máx", s.yaw, value => { s.yaw = value; save(); });
                settings.Add(new HelpBox("Para módulos sem lacunas, mantenha escala aleatória 1/1 e use BottomCorner. No snap, rotação aleatória é ignorada; R usa o incremento definido. Offsets manuais podem deslocar a peça para fora da grade.", HelpBoxMessageType.Info));
                settings.Add(new Label("Smart Grid")); settings.Add(detected); settings.Add(current);
                Toggle(settings, "Show Grid", s.showGrid, value => { s.showGrid = value; save(); });
                autoField = Toggle(settings, "Auto Grid Size", s.autoGridSize, value => { s.autoGridSize = value; save(); render(); });
                manualField = Toggle(settings, "Dimensões de encaixe manuais por prefab", p.manualDimensions, value => { p.manualDimensions = value; save(); render(); });
                Vector2(settings, "Manual Grid Size X/Z (por prefab)", p.cell, value => { p.cell = UnityEngine.Vector2.Max(value, UnityEngine.Vector2.one * .01f); p.manualDimensions = true; save(); updateLabels(); });
                widthField = Float(settings, "Grid Width (célula X)", s.gridCell.x, value => { s.autoGridSize = false; s.gridCell.x = value; save(); updateLabels(); });
                lengthField = Float(settings, "Grid Length (célula Z)", s.gridCell.y, value => { s.autoGridSize = false; s.gridCell.y = value; save(); updateLabels(); });
                Vector2(settings, "Área de exibição X/Z", s.gridArea, value => { s.gridArea = value; save(); });
                Vector3(settings, "Grid Origin", s.gridOrigin, value => { s.gridOrigin = value; save(); updateLabels(); });
                Float(settings, "Grid Height", s.gridHeight, value => { s.gridHeight = value; save(); });
                Toggle(settings, "Snap Enabled (por prefab)", p.snap, value => { p.snap = value; save(); });
                Toggle(settings, "Rotation Snap", s.rotationSnap, value => { s.rotationSnap = value; save(); });
                Float(settings, "Incremento de rotação (°)", s.rotationStep, value => { s.rotationStep = value; save(); });
                settings.Add(new Button(() =>
                {
                    if (!s.autoGridSize) s.gridCell = p.manualDimensions ? PrefabDimensionAnalyzer.CellSize(prefabObject, bounds, p) : p.cell;
                    save(); render(); BuilderLog.Info("Dimensão da grade aplicada. Origem e objetos existentes preservados.");
                }) { text = "Apply Suggested Grid Size", tooltip = "No modo automático, a grade já é atualizada ao selecionar. No manual, aplica a célula do perfil." });
                updateLabels();
                current.schedule.Execute(updateLabels).Every(150);
            };
            painter.SettingsChanged = render;
            render();
        }
        private static Toggle Toggle(VisualElement root, string label, bool value, Action<bool> change)
        { var f = new Toggle(label) { value = value }; f.RegisterValueChangedCallback(e => change(e.newValue)); root.Add(f); return f; }
        private static FloatField Float(VisualElement root, string label, float value, Action<float> change)
        { var f = new FloatField(label) { value = value }; f.RegisterValueChangedCallback(e => change(e.newValue)); root.Add(f); return f; }
        private static void Vector2(VisualElement root, string label, Vector2 value, Action<Vector2> change)
        { var f = new Vector2Field(label) { value = value }; f.RegisterValueChangedCallback(e => change(e.newValue)); root.Add(f); }
        private static void Vector3(VisualElement root, string label, Vector3 value, Action<Vector3> change)
        { var f = new Vector3Field(label) { value = value }; f.RegisterValueChangedCallback(e => change(e.newValue)); root.Add(f); }
        private static void Enum(VisualElement root, string label, System.Enum value, Action<System.Enum> change)
        { var f = new EnumField(label, value); f.RegisterValueChangedCallback(e => change(e.newValue)); root.Add(f); }
    }
}

