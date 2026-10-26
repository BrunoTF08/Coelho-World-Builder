using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CoelhoLabs.WorldBuilder.Editor
{
    internal sealed class PrefabLibraryView : VisualElement, IDisposable
    {
        private readonly ScenePainter painter;
        private readonly PrefabLibrary library;
        private readonly VisualElement folders = new VisualElement();
        private readonly Label info = new Label();
        private readonly Image thumbnail = new Image();
        private readonly List<ListView> lists = new List<ListView>();
        private bool subscribed;
        private string thumbnailGuid;
        internal PrefabLibraryView(ScenePainter painter)
        {
            this.painter = painter; library = PrefabLibrary.instance; library.Initialize();
            Add(new Label("Biblioteca de prefabs — pastas virtuais"));
            var create = new VisualElement(); create.style.flexDirection = FlexDirection.Row;
            var name = new TextField { value = "Nova categoria" }; name.style.flexGrow = 1; create.Add(name);
            create.Add(new Button(() => library.CreateCategory(name.value)) { text = "+ Criar categoria" }); Add(create);
            thumbnail.style.width = 64; thumbnail.style.height = 64; Add(thumbnail);
            info.style.whiteSpace = WhiteSpace.Normal; Add(info); Add(folders);
            Add(new HelpBox("Arraste prefabs do Project para uma pasta. Ativar define a categoria dos atalhos. ↑/↓ reorganizam; Mover transfere referências. Excluir pasta ou referência não exclui assets.", HelpBoxMessageType.Info));
            RegisterCallback<AttachToPanelEvent>(_ => Subscribe()); RegisterCallback<DetachFromPanelEvent>(_ => Dispose());
            Rebuild();
            info.schedule.Execute(RefreshSummary).Every(200);
        }
        private void Subscribe()
        {
            if (subscribed) return; subscribed = true;
            library.Changed += Rebuild; painter.SelectionChanged += RefreshSelection;
        }
        public void Dispose()
        {
            if (!subscribed) return; subscribed = false;
            library.Changed -= Rebuild; painter.SelectionChanged -= RefreshSelection;
        }
        private void RefreshSelection()
        {
            RefreshSummary();
            foreach (var list in lists) list.RefreshItems();
        }
        private void RefreshSummary()
        {
            var category = library.Active; string guid = BuilderSettings.instance.selectedPrefabGuid;
            int index = category == null ? -1 : category.prefabs.IndexOf(guid);
            var selected = BuilderSettings.instance.SelectedPrefab;
            if (thumbnailGuid != guid)
            {
                thumbnailGuid = guid;
                thumbnail.image = selected == null ? null : AssetPreview.GetMiniThumbnail(selected);
            }
            info.text = $"Categoria: {category?.name ?? "Nenhuma"}\nPrefab: {(selected == null ? "Nenhum" : selected.name)}\nObjeto: {(index < 0 ? 0 : index + 1)} / {category?.prefabs.Count ?? 0}\nGrade: {BuilderSettings.instance.gridCell.x:0.###} × {BuilderSettings.instance.gridCell.y:0.###}\nCtrl + Q ◀ Anterior    |    Ctrl + E Próximo";
        }
        private void Rebuild()
        {
            folders.Clear(); lists.Clear();
            foreach (var category in library.categories)
            {
                var foldout = new Foldout { text = category.name, value = category.expanded };
                foldout.RegisterValueChangedCallback(e =>
                {
                    if (e.target != foldout) return;
                    category.expanded = e.newValue; library.SaveExpansion();
                });
                var actions = new VisualElement(); actions.style.flexDirection = FlexDirection.Row;
                actions.Add(new Button(() => library.Activate(category)) { text = library.Active == category ? "✓ Ativa" : "Ativar" });
                var rename = new TextField { value = category.name, isDelayed = true }; rename.style.flexGrow = 1;
                rename.RegisterValueChangedCallback(e => library.Rename(category, e.newValue)); actions.Add(rename);
                actions.Add(new Button(() => library.ReorderCategory(category, -1)) { text = "↑", tooltip = "Subir categoria" });
                actions.Add(new Button(() => library.ReorderCategory(category, 1)) { text = "↓", tooltip = "Descer categoria" });
                actions.Add(new Button(() => library.Delete(category)) { text = "Excluir", tooltip = "Remove só a pasta virtual" });
                foldout.Add(actions);
                var list = new ListView { itemsSource = category.prefabs, fixedItemHeight = 28, selectionType = SelectionType.None,
                    virtualizationMethod = CollectionVirtualizationMethod.FixedHeight };
                list.style.height = Mathf.Clamp(category.prefabs.Count * 28, 28, 224);
                list.makeItem = () => MakeRow(category);
                list.bindItem = (row, index) => BindRow(row, category.prefabs[index]);
                lists.Add(list); foldout.Add(list); foldout.Add(new Label("Solte prefabs aqui"));
                foldout.RegisterCallback<DragUpdatedEvent>(e =>
                {
                    if (!HasPrefabDrag()) return;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy; e.StopPropagation();
                });
                foldout.RegisterCallback<DragPerformEvent>(e =>
                {
                    if (!HasPrefabDrag()) return;
                    DragAndDrop.AcceptDrag(); var entries = DragAndDrop.objectReferences;
                    library.AddRange(category, entries);
                    e.StopPropagation();
                });
                folders.Add(foldout);
            }
            RefreshSelection();
        }
        private static bool HasPrefabDrag()
        {
            foreach (var obj in DragAndDrop.objectReferences)
                if (obj is GameObject go && PrefabUtility.IsPartOfPrefabAsset(go) && go.transform.parent == null) return true;
            return false;
        }
        private VisualElement MakeRow(PrefabCategory category)
        {
            var row = new VisualElement(); row.style.flexDirection = FlexDirection.Row;
            var icon = new Image { name = "icon" }; icon.style.width = 20; row.Add(icon);
            var choose = new Button(() =>
            {
                var asset = library.Resolve(row.userData as string);
                if (asset == null) { BuilderLog.Info("Referência ausente ou inválida. Remova a entrada ou restaure o asset."); return; }
                library.Activate(category); painter.SelectPrefab(asset);
            }) { name = "choose" }; choose.style.flexGrow = 1; row.Add(choose);
            row.Add(new Button(() => library.ReorderPrefab(category, (string)row.userData, -1)) { text = "↑" });
            row.Add(new Button(() => library.ReorderPrefab(category, (string)row.userData, 1)) { text = "↓" });
            row.Add(new Button(() =>
            {
                var guid = (string)row.userData; var menu = new GenericMenu();
                foreach (var destination in library.categories)
                {
                    if (destination == category) continue;
                    var target = destination;
                    menu.AddItem(new GUIContent(target.name.Replace("/", "∕")), false, () => library.Move(category, target, guid));
                }
                if (library.categories.Count < 2) menu.AddDisabledItem(new GUIContent("Crie outra categoria"));
                menu.ShowAsContext();
            }) { text = "Mover" });
            row.Add(new Button(() => library.Remove(category, (string)row.userData)) { text = "×", tooltip = "Remover referência" });
            return row;
        }
        private static void BindRow(VisualElement row, string guid)
        {
            row.userData = guid; string path = AssetDatabase.GUIDToAssetPath(guid);
            var choose = row.Q<Button>("choose"); bool selected = guid == BuilderSettings.instance.selectedPrefabGuid;
            choose.text = (selected ? "✓ " : "") + (string.IsNullOrEmpty(path) ? "[Ausente] " + guid : Path.GetFileNameWithoutExtension(path));
            choose.tooltip = path; choose.style.unityFontStyleAndWeight = selected ? FontStyle.Bold : FontStyle.Normal;
            row.Q<Image>("icon").image = string.IsNullOrEmpty(path) ? null : AssetDatabase.GetCachedIcon(path);
        }
    }
}
