# Registro de validação — 2026-10-25

## Distribuição 1.3.1 e manual

`Tools/Release/Build-Release.ps1` gera o pacote UPM .tgz, um ZIP com package.json/manual/LEIA-ME, o manual HTML offline e SHA256SUMS.txt em dist (ignorada pelo Git). O manual também está em Documentation/Manual-pt-BR.md e .html dentro do pacote.

O .tgz foi instalado via manifest em um projeto Unity 2022.3.22f1 isolado. Passaram a importação do tarball, compilação C#, conferência do nome/versão, presença do manual offline, execução do menu e criação da janela com o título Easy Level Design Tool. Log local: `.validation/release-check.log`. Isso valida instalação e estrutura; não representa um novo ensaio visual de todas as ferramentas.

## 1.3.0 — Categorias e seleção rápida

**46 verificações aprovadas em Unity 2022.3.22f1 e Unity 6000.0.76f1**, com `Tools/Validation/CategoryValidation.cs`. Logs: `.validation/categories2022-final.log` e `.validation/categories6-final.log`.

Em novos processos, **4 verificações adicionais passaram em cada versão**: quantidade de categorias, nome/ordem, pasta ativa/referências ordenadas e navegação após reinício com referência ausente. Logs: `.validation/categories2022-restart-final.log` e `.validation/categories6-restart-final.log`.

Regressão: as **34 verificações de Smart Adaptive Grid/Ctrl Directional Snap passaram nas duas versões**, incluindo contato de bordas, transformações, Undo/Redo, referências invalidadas e conexões manuais. Logs: `.validation/categories-regression2022.log` e `.validation/categories-regression6.log`.

Cobertura: criar categorias e registrar quatro ruas; impedir duplicatas; Ctrl+E/Q anterior/próximo; ciclos nos dois sentidos; adaptação da grade; construção ativa; troca sem instâncias novas e origem preservada; isolamento em outra categoria; categoria de item único; foco desativado; edição de texto; construção parada; integração com referência e preview do Ctrl direcional após Ctrl+E; grade manual e dimensões por perfil; reordenar, mover, renomear e excluir; referência ausente após exclusão do asset; categoria vazia; assets originais preservados; painel/Asset Library e reabertura da janela; identidade da árvore de pastas preservada ao trocar a seleção.

Os atalhos foram testados com eventos Unity construídos e chamadas ao mesmo handler privado utilizado na Scene View. O parâmetro de foco foi simulado, e o guarda de texto IMGUI foi ativado programaticamente. Isso não substitui testar Ctrl+Q/E físicos, foco real, overlays/texto UI Toolkit, drag-and-drop, aparência/miniaturas ou desempenho com centenas de prefabs. Os controles estruturais foram validados pelo serviço de biblioteca; drop múltiplo e botões não foram acionados fisicamente. Execução batch/nographics; sem NUnit Test Runner nesta rodada.

Arquivos criados: `Editor/Library/PrefabLibrary.cs`, `Editor/UI/PrefabLibraryView.cs` e `Tools/Validation/CategoryValidation.cs`, além dos metadados Unity. Arquivos modificados: `Editor/Core/BuilderSettings.cs`, `Editor/Core/DemoBuilder.cs`, `Editor/Placement/ScenePainter.cs`, `Editor/UI/SmartPlacementPanel.cs`, `Editor/UI/WorldBuilderWindow.cs`, `package.json`, README, CHANGELOG e documentação. A demo também registra seu prefab na biblioteca virtual. Foram mantidos Smart Grid, Ctrl direcional, Ghost e conversão para MeshCollider.

Biblioteca: ScriptableObject baseado em ScriptableSingleton, salvo em `ProjectSettings/CoelhoWorldBuilderLibrary.asset`, com IDs de categoria e GUIDs dos prefabs. A lista antiga é importada uma única vez para Outros. As operações de pasta não movem/apagam assets. As listas são virtualizadas; troca de seleção atualiza propriedades/linhas visíveis sem reconstruir a árvore de pastas. Alterações estruturais e alterações do AssetDatabase invalidam o cache e podem reconstruir a árvore.

Reprodução: em projeto isolado, copie o harness para Assets/Editor e execute `-executeMethod CategoryValidation.Run`. Ele redefine preferências, cria prefabs de teste e exclui um desses assets deliberadamente para testar referências ausentes. Não execute sobre o projeto de produção. Para persistência em novo processo, execute `-executeMethod CategoryValidation.VerifyRestart` no mesmo projeto.

## Mesh Colliders

Harness SmartGridValidation: **25 verificações aprovadas em Unity 2022.3.22f1 e 6000.0.76f1**, incluindo conversão das instâncias para MeshCollider, preservação do BoxCollider no prefab original, encaixe e Undo/Redo. Logs: `.validation/mesh2022.log` e `.validation/mesh6.log`. A importação inicial 2022 registrou referência temporariamente ausente ao script novo; a recompilação seguinte concluiu e executou os testes. Triggers/Rigidbodies dinâmicos e malhas complexas convexas não foram exercitados nessas fixtures.

## 1.2.0 — Smart Adaptive Grid + Ctrl Directional Snap

Causa encontrada: Auto Grid Size da 1.1 era apenas sugestão aplicada por botão. Selecionar outro prefab também parava o ScenePainter. Na 1.2, seleção/alteração de configuração chama PrefabDimensionAnalyzer.ApplyAdaptiveGrid, atualiza as células usadas pelo desenho de Handles e repinta a Scene View, sem escrever nos Transforms das instâncias. Trocar a seleção preserva a construção ativa.

**34 verificações aprovadas em Unity 2022.3.22f1 e Unity 6000.0.76f1**, por `Tools/Validation/AdaptiveGridValidation.cs`. Logs: `.validation/adaptive2022-final.log` e `.validation/adaptive6-final.log`.

Após encerrar e reiniciar cada versão, **mais 4 verificações passaram em cada uma**: preferências adaptativas, perfil/categoria/snap por GUID, recuperação da referência modular e metadados de encaixe serializados na cena. Logs: `.validation/adaptive2022-restart.log` e `.validation/adaptive6-restart.log`.

Cobertura: seleção 8 × 8 sem Apply; referência modular; preview nas quatro direções com contato de bordas e sem alterar rotação/escala; confirmação atualizando referência; Undo/Redo restaurando referência; troca ativa para 15 × 15; origem/instâncias preservadas; contato entre 8 e 15 com pivôs deslocados; transição 15 → 2 → 15; decoração preservando referência; soltar Ctrl; modo manual global; override manual por prefab; rotação/escala; projeções de módulos girados; escolha de conexão oposta e transformação de ponto manual; exclusão de auxiliares; referência desativada, em outra cena e excluída; Ctrl sem referência; snap desligado; parada explícita; construção do novo painel e fechamento/reabertura da janela. Fixtures são prefabs de cubos filhos e pivôs deslocados, não assets comerciais.

Regressão da base: as **23 verificações anteriores passaram no Unity 2022.3.22f1** com o novo fluxo de grade/tracker integrado (`.validation/adaptive-regression2022.log`). Testes NUnit adicionais de direções estão em Tests/Editor/DirectionalSnapTests.cs; não foram executados pelo NUnit Test Runner.

As direções e transições de Ctrl foram exercitadas diretamente pelos métodos reais usados na Scene View. A execução em batch/nographics não valida teclas físicas, arrasto real, aparência do ghost, linhas renderizadas ou navegação/overlays. O cancelamento foi validado por SetActive(false), que é chamado pelo handler de Escape; a entrega real da tecla Escape continua no roteiro manual. O teste de conexão manual verifica a matemática do conector e a seleção da face oposta, sem uma cena de sockets irregular pronta. Não se afirma encaixe perfeito para geometrias irregulares.

Arquivos novos: PrefabDimensionAnalyzer.cs, LastPlacedPrefabTracker.cs, DirectionalSnapMath.cs, Runtime/Components/DimensionIgnore.cs, Tests/Editor/DirectionalSnapTests.cs e Tools/Validation/AdaptiveGridValidation.cs. Modificados: ScenePainter.cs, PrefabPlacementProfile.cs, SmartPlacementMath.cs, SmartPlacementPanel.cs, WorldBuilderWindow.cs, BuilderSettings.cs, GeneratedObject.cs, package.json e documentação. Metadados Unity foram gerados e os scripts locais de validação foram atualizados.

Para reproduzir, copie o harness para Assets/Editor de um projeto isolado com o pacote instalado. Execute `-executeMethod AdaptiveGridValidation.Run`. Ele cria assets/cenas de teste e redefine preferências: não o execute no projeto de produção. Em um segundo processo no mesmo projeto, use `-executeMethod AdaptiveGridValidation.VerifyRestart` para conferir preferências, categoria e recuperação da referência a partir da cena salva.

## 1.1.0 — Placement & Smart Grid

Harness reproduzível: `Tools/Validation/SmartGridValidation.cs`, na raiz do workspace. Copie para Assets/Editor de um projeto isolado com o pacote instalado. Execute Unity em batch com `-executeMethod SmartGridValidation.Run`. O harness cria cenas/assets de teste e modifica preferências; não deve ser executado sobre o projeto de trabalho do usuário. Para persistência após encerramento, execute outro processo com `-executeMethod SmartGridValidation.VerifyRestart` no mesmo projeto.

Resultados: **23 verificações aprovadas em Unity 2022.3.22f1 e 6000.0.76f1**, incluindo bounds compostos com pivô deslocado, rotação/escala não uniforme, origem fixa, handlers R/G, raycast, plano sem Collider, fallback Surface, preview sem GameObjects, cache de malhas/material, instanciação conectada ao prefab, duas ruas 8 × 8 encostadas sem lacunas, Undo/Redo com IDs/GUIDs preservados, bloqueio de posição ocupada, mesma célula bloqueada durante Paint, dimensões 15 × 15 e liberação do ghost. O painel UI Toolkit também foi construído e fechado pelo harness.

Mais **2 verificações aprovadas após reiniciar cada versão do Editor**: preferências gerais e perfil por GUID (incluindo rotação/offset/célula). Logs locais: `.validation/smart2022-final.log`, `.validation/smart6-final.log`, `.validation/smart2022-restart-final.log` e `.validation/smart6-restart-final.log`.

As execuções usaram batchmode/nographics. Isso valida código de geração, dados e construção da UI, mas não comprova aparência renderizada do ghost, blending em cada render pipeline, gestos reais de mouse, overlays, cancelamento por tecla Escape entregue à Scene View ou desempenho em cenas grandes. Os handlers de R/G foram chamados diretamente pelo harness, não por teclado simulado. O caso de Paint validou geração/deduplicação por célula e não um arrasto físico. Os testes NUnit incluídos não foram executados pelo Test Runner. O roteiro manual está em GettingStarted.md.

### Arquivos da atualização

Modificados: `Editor/Placement/ScenePainter.cs`, `Editor/Core/BuilderSettings.cs`, `Editor/UI/WorldBuilderWindow.cs`, `package.json`, `README.md`, `CHANGELOG.md` e a documentação.

Criados: `Editor/Placement/PrefabPlacementProfile.cs`, `Editor/Placement/SmartPlacementMath.cs`, `Editor/Placement/Ghost.shader`, `Editor/UI/SmartPlacementPanel.cs`, `Tests/Editor/SmartPlacementMathTests.cs`, respectivos metadados Unity e `Tools/Validation/SmartGridValidation.cs` fora do pacote. Os scripts de validação locais em `.validation` foram atualizados. Guias WorldGrid/GuideLine e componentes existentes foram preservados.

Causas identificadas no código anterior: falta de fallback quando o raycast não encontrava Collider, suspensão pela seleção de WorldGrid/GuideLine, ausência de seleção explícita e ghost limitado a caixas. O fluxo atual remove essas condições e assume a ferramenta de construção ao ativar, restaurando a ferramenta anterior ao parar/fechar.

## Grade verde — verificação adicional

Unity 2022.3.22f1 e 6000.0.76f1 concluíram o harness com `COELHO_GRID_VALIDATION_PASSED`: criação de WorldGrid, edição de espaçamento/extensão com Undo/Redo e persistência após salvar/reabrir a cena. Logs em `.validation/grid2022.log` e `.validation/grid6.log`. Na primeira importação 2022 houve diagnósticos transitórios antes de reconhecer os novos scripts; a recompilação seguinte concluiu e executou o harness. Aparência, sobreposição e manipulação real de Transform precisam de conferência visual na Scene View.

## Linhas verdes — verificação adicional

Unity 2022.3.22f1 e 6000.0.76f1: compilação concluída e harness batch aprovado para criação de GuideLine, adição de ponto, Undo/Redo da lista e fechamento, salvamento e reabertura da cena com pontos preservados. Logs locais: `.validation/lines2022-final.log` e `.validation/lines6.log`. O desenho dos Gizmos e os gestos reais com mouse ainda precisam de teste visual no Editor.

Executado em projetos isolados dentro de `.validation` no workspace. Não foi alterado o projeto aberto em outra instância do Unity.

| Verificação via Unity em batch mode | 2022.3.22f1 | 6000.0.76f1 |
| --- | --- | --- |
| Importação UPM e compilação C# | Passou | Passou |
| Amostras dentro do disco (10.000) | Passou | Passou |
| Sequência idêntica para mesma seed | Passou | Passou |
| Distribuição uniforme em área (média de raio² entre 0,48 e 0,52) | Passou | Passou |
| Abertura da EditorWindow | Passou | Passou |
| Criação e salvamento de prefab e cena demo | Passou | Passou |
| Undo de GameObject com metadados e Redo restaurando componente | Passou | Passou |

Os dois logs registraram `COELHO_VALIDATION_PASSED: distribution, determinism, window, demo, undo, redo`. Há mensagens do subsistema de licenciamento no início dos logs; elas não impediram a importação, compilação ou execução das verificações.

O primeiro ensaio detectou que Unity rejeita criação aditiva quando uma cena aberta não tem caminho salvo. DemoBuilder agora verifica isso antes de criar assets e informa o usuário. O ensaio posterior usou uma cena inicial salva e passou nas duas versões.

Essas verificações usaram um harness com executeMethod, não o NUnit Test Runner. Os testes EditMode incluídos no pacote ainda precisam ser executados pelo Test Framework. Aparência, interação real de mouse, raycast durante pintura, gestos de Undo, persistência após reinício e desempenho em cenas grandes precisam dos procedimentos manuais de GettingStarted.md. Não foram considerados aprovados por inferência.
