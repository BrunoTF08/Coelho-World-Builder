# Changelog

## 1.3.1 — Easy Level Design Tool

- Nome da ferramenta atualizado no Package Manager, menu Tools, janela, cabeçalho, ajuda, logs e documentação.
- Identificadores de pacote/assemblies, namespaces, GUIDs e caminhos de persistência preservados para compatibilidade.

## 1.3.0 — Categorias e seleção rápida

- Biblioteca ScriptableObject por projeto em ProjectSettings/CoelhoWorldBuilderLibrary.asset, com categorias virtuais, GUIDs e migração da lista antiga para Outros.
- Pastas expansíveis, criação/renomeação/exclusão, ordem de pastas e prefabs, transferência de referências e drop múltiplo do Project.
- Ctrl+Q/E anterior/próximo circular somente na categoria ativa, com guardas de foco/construção/texto e prioridade sobre Ctrl direcional.
- Troca reaproveita SelectPrefab: preserva construção, grade adaptativa/manual, origem e objetos existentes.
- Indicador de categoria, nome/índice/total, grade e miniatura; referências ausentes sinalizadas e ignoradas na navegação.
- Listas virtualizadas, cache de assets selecionados e atualização de seleção sem reconstruir a árvore de pastas.

## Ajuste — Mesh Colliders

- Novas instâncias substituem Box Colliders por Mesh Colliders quando há MeshFilter disponível, sem modificar o asset original.
- Conversão incluída no Undo/Redo; demonstração criada com Mesh Collider.

## 1.2.0 — Smart Adaptive Grid + Ctrl Directional Snap

- Corrigido Auto Grid Size: aplica imediatamente ao selecionar prefab e editar rotação/escala, sem mover origem ou instâncias existentes.
- PrefabDimensionAnalyzer com bounds locais compostos, override manual de encaixe e exclusão de auxiliares via DimensionIgnore/EditorOnly.
- Troca de prefab preserva modo de construção ativo e atualiza o ghost.
- Categorias Decorative/Roads/Floors e LastPlacedPrefabTracker com metadados de encaixe persistentes e validação por cena/Undo/Redo.
- Ctrl escolhe quatro direções nos eixos horizontais da referência e translada o preview por bordas projetadas, sem girar ou redimensionar a peça.
- Ctrl coloca apenas por clique, preserva rotação/escala da prévia e retorna ao posicionamento normal ao soltar.
- Painel Smart Grid & Directional Placement, direção/referência/estado ao vivo e controles de override, alinhamento e offset.
- Harness AdaptiveGridValidation e testes NUnit de encaixe direcional.

## 1.1.0 — Placement & Smart Grid

- Corrigido bloqueio de posicionamento sem Collider, com plano horizontal configurável, e bloqueio por seleção de guias.
- Seleção explícita do prefab, biblioteca existente preservada e painel unificado Placement & Smart Grid.
- Ghost de malhas estáticas/skinned com material temporário transparente; verde/vermelho conforme validação.
- Grade fixa sem GameObjects, dimensões orientadas sugeridas por bounds e aplicação explícita preservando origem/instâncias.
- Perfis por GUID com escala/rotação, célula, offsets, âncora, alinhamento e snap persistentes.
- Modos Grid/Free/Surface, snap por âncora, R/G/Escape e pintura com deduplicação por célula e Undo por gesto.
- Cache de geometria, índice espacial de objetos gerenciados e consultas físicas com buffers reutilizados.
- Testes de matemática e harness de integração reproduzível em Tools/Validation.

## Adição — grade verde

- Grade visual plana na Scene View com células, extensão, cores e linhas principais configuráveis.
- Controle de sobreposição, visibilidade e Transform; persistência na cena e Undo/Redo.
- Criação pela página Grade e menu GameObject.

## Adição — linhas-guia editáveis

- Linhas verdes independentes, pontos locais persistentes, segmentos abertos/fechados, cor e espessura.
- Criação pela janela e menu GameObject; edição com handles e Inspector, adição por clique e remoção por índice.
- Undo/Redo e suspensão da pintura quando uma linha está selecionada.

## 1.0.0 — 2026-10-25

- Fundação UPM com assemblies separados de editor/runtime.
- Janela UI Toolkit, navegação, preferências locais e logs.
- Pintura Single/Brush, prefabs múltiplos, seed, raio, densidade, espaçamento, escala, rotação, normal, offset e layers.
- Prévia de limites na Scene View, Undo/Redo e IDs persistentes.
- Gerador local de demonstração, testes EditMode e guia de instalação.
