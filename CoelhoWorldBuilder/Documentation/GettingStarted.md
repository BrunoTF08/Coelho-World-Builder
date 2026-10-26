# Easy Level Design Tool 1.3 — Categorias e Smart Grid

## Categorias e seleção rápida

A biblioteca aparece no painel **Smart Grid & Directional Placement** e na página **Asset Library**. São pastas virtuais: nenhum arquivo original é movido, copiado ou apagado. Na primeira abertura, a lista de prefabs da versão anterior é migrada uma vez para **Outros**. As categorias de exemplo são editáveis.

- **+ Criar categoria** usa o nome informado. Expanda uma pasta para gerenciá-la. **Ativar** define a pasta dos atalhos, mesmo que o prefab atual pertença a outra.
- Arraste um ou vários prefabs raiz do Project sobre uma pasta. O drop múltiplo é salvo de uma vez e ignora duplicatas dentro da mesma pasta. O mesmo prefab pode pertencer a pastas diferentes.
- O campo **Prefab selecionado** também cadastra o prefab na categoria ativa. Clique numa referência da árvore para selecionar o prefab e ativar sua pasta.
- Edite o nome da pasta e confirme com Enter ou ao sair do campo. **↑/↓** mudam a ordem de categorias ou referências. **Mover** transfere uma referência para outra pasta. **×** remove a referência e **Excluir** remove a pasta com suas referências; os assets continuam no Project.
- Com construção ativa e foco na Scene View, **Ctrl+Q** seleciona o anterior e **Ctrl+E** o próximo, em ciclo e exclusivamente na categoria ativa. Se a seleção não pertence à pasta ativa, E começa pelo primeiro e Q pelo último. Referências ausentes são puladas; pastas vazias não fazem a navegação saltar para outra categoria.
- Os atalhos são ignorados fora da Scene View, com a construção parada, durante edição de texto ou com modificadores extras Alt/Shift/Command. Esses guardas são verificados antes da troca.
- Ctrl+Q/E tem prioridade sobre Ctrl direcional: apenas troca o prefab, sem colocar objetos ou trocar a referência da última rua. Mantendo Ctrl pressionado depois da troca, o encaixe direcional continua com a referência existente. A rotação padrão de cada perfil é respeitada.

O indicador mostra pasta ativa, prefab, índice/total de referências, tamanho da grade e miniatura. Selecionar outro prefab atualiza ghost e Smart Adaptive Grid mantendo a origem, instâncias e construção. Grade manual e dimensões manuais por prefab continuam respeitadas. `[Ausente]` identifica uma referência sem asset; remova a entrada ou restaure o asset com seu GUID. Referências ausentes continuam contando no total da pasta, mas são ignoradas pelos atalhos.

Categorias, referências por GUID, ordem, expansão e pasta ativa ficam em **ProjectSettings/CoelhoWorldBuilderLibrary.asset**, um ScriptableObject salvo por ScriptableSingleton. Esse arquivo pode ser versionado com o projeto. As preferências de posicionamento continuam separadas em UserSettings. Não há arquivo da biblioteca embutido no pacote que sobrescreva as categorias do projeto ao atualizar.

As listas usam virtualização e ícones em cache; os modelos são resolvidos sob demanda para a seleção/navegação. O painel de propriedades é atualizado ao trocar a seleção, mas a árvore de pastas permanece. Operações estruturais (criar, mover, remover, alterar a ordem) reconstruem a árvore. Não há drag interno para reordenar: use ↑/↓ e o menu Mover. Categorias da biblioteca não alteram automaticamente a classificação Roads/Floors/Decorative dos perfis, que permanece independente.

Teste manualmente o drop, expansão, menu Mover e atalhos físicos na Scene View, inclusive sobre overlays e campos de texto. Uma cena com Ctrl direcional deve continuar encaixando após trocar de prefab com Q/E. Feche/reabra a janela e reinicie o Unity para verificar a biblioteca do seu projeto.

## Iniciar

Instale/atualize pelo Package Manager > Add package from disk usando o package.json deste pacote. Abra Tools > Coelho Labs > Easy Level Design Tool. Reabra a janela após atualizar para ver a nova navegação.

1. Abra **Smart Grid & Directional Placement**. Placement & Smart Grid e Prefab Painter continuam disponíveis e abrem o mesmo painel.
2. Escolha a raiz de um prefab no campo Prefab selecionado ou clique no nome de uma peça da biblioteca. A seleção pelo ObjectField adiciona o asset à lista, sem duplicar ou modificar o arquivo original.
3. Com **Auto Grid Size** ligado (padrão), a grade inteira recebe automaticamente as dimensões X/Z detectadas. Não é necessário clicar Apply. Rua 15 × 15 → detalhe 2 × 2 → rua 15 × 15 atualiza toda a grade para 15 → 2 → 15.
4. A origem e os objetos existentes ficam preservados. Trocar o prefab durante a construção mantém a ferramenta ativa e atualiza a prévia.
5. Use **Activate Placement** e clique na Scene View. Verde indica posição válida e vermelho indica impedimento. O ghost desenha as malhas com material temporário transparente, sem instanciar objetos de prévia ou alterar materiais originais.
6. Para ruas/pisos regulares, use BottomCorner, escala padrão coerente com os módulos e escala aleatória 1/1. Salve a cena após construir.

## Auto Grid e dimensões manuais

PrefabDimensionAnalyzer usa bounds geométricos no espaço local da raiz, considerando transformações de filhos, pivôs deslocados, escala original, escala padrão do perfil e rotação. O modo automático também considera a quantização Rotation Snap e o multiplicador de escala aleatória quando mínimo e máximo são iguais. Com escala aleatória variável, a grade representa a escala padrão e não muda a cada amostra do pincel.

Renderers desativados, objetos inativos, auxiliares com HideFlags de editor e hierarquias marcadas EditorOnly são ignorados na análise. Adicione **Coelho Labs > Ignore Placement Dimensions** ao objeto auxiliar para excluir seu ramo dos limites de encaixe. Isso não remove a geometria do prefab nem altera seus materiais. Na ausência de Renderers utilizáveis, o analisador tenta Colliders Box/Sphere/Capsule/Mesh habilitados e não trigger.

**Dimensões de encaixe manuais por prefab** permite definir um tamanho de encaixe diferente do visual. Manual Grid Size X/Z é medido após a escala original da raiz, antes da escala padrão do perfil e da rotação. Mantém-se o centro dos bounds/pivô. Essas dimensões são usadas tanto para a grade automática quanto para o encaixe direcional e âncoras.

Para uma grade global fixa, desligue Auto Grid Size. Grid Width/Length ajustam células X/Z diretamente e desligam o modo automático. Apply Suggested Grid Size permanece disponível para aplicar a célula do perfil no modo manual. Trocar de prefab nesse modo preserva a grade global.

Grid Origin é fixa no mundo. Grid Height soma altura à origem Y. Área de exibição controla a extensão, limitada a 201 linhas por eixo. Show Grid oculta somente a visualização, mantendo o snap. Não há GameObjects para desenhar a grade. Ela aparece enquanto a janela estiver aberta, mesmo com Placement parado.

## Ctrl Directional Snap

1. Em **Snap Category**, classifique a peça como **Roads** ou **Floors**. Decorative é o padrão seguro para objetos comuns.
2. Posicione uma primeira peça normalmente. Ela se torna a referência modular.
3. Com **Enable Ctrl Directional Snap** e **Snap Enabled** ligados, segure Ctrl e mova o mouse ao redor da referência.
4. O maior deslocamento horizontal escolhe direita/esquerda/frente/trás nos eixos horizontais da referência. A prévia encosta na borda correspondente.
5. Clique para colocar. A nova peça passa a ser a referência; Ctrl continua ativo enquanto estiver pressionado. No modo Paint, Ctrl também coloca somente por clique, sem distribuir durante o arrasto.
6. Solte Ctrl para voltar ao posicionamento normal. Ctrl não controla rotação: preserva a orientação e a escala da prévia durante o gesto. Ao trocar explicitamente de prefab, passa a usar a configuração da nova peça.

Peças de tamanhos diferentes usam as duas caixas de encaixe transformadas, incluindo pivôs e rotação, em vez de somar posições dos pivôs. **Edge Alignment** alinha as bordas mínimas no eixo perpendicular; desligado, centraliza nesse eixo. **Snap Offset** aplica ajuste em coordenadas do mundo. Um offset que penetre a referência deixa a posição inválida. Offsets positivos podem criar uma folga intencional.

A referência precisa estar ativa, na cena ativa e na mesma categoria Roads/Floors da peça escolhida. Sem referência compatível, Ctrl mostra impedimento: solte Ctrl e coloque primeiro uma peça da categoria desejada. Decoração não substitui a referência modular. Exclusão, desativação e mudança de cena invalidam a referência. Undo/Redo reconcilia a última peça modular restante usando os metadados de ordem salvos na cena. Reativar Placement também recupera a referência mais recente da cena ativa.

Para conectores manuais, habilite **Usar pontos de conexão locais** no perfil e informe os quatro pontos Right/Left/Forward/Back em coordenadas locais da raiz. Configure e coloque a referência com esses pontos habilitados. Quando referência e nova peça possuem pontos, o encaixe conecta o ponto direcional da referência ao ponto oposto da peça atual, considerando suas rotações e escalas, sem girar a peça. Sem pontos em ambas, utiliza bounds. Os pontos são copiados para os metadados da instância ao colocá-la; editar o perfil não altera instâncias existentes. A validação de sobreposição continua ativa. Não há editor visual de sockets nesta versão.

O painel mostra nome do prefab, dimensões detectadas, tamanho atual da grade, última referência, direção e estado. **Show Snap Preview** pode ocultar a prévia direcional; a posição continua sendo calculada para o clique. Desligar Snap Enabled ou Enable Ctrl Directional Snap retorna ao fluxo normal, sem direção forçada.

## Placement e atalhos

- **Grid:** usa o plano horizontal fixo em Grid Origin.y + Grid Height. Snap alinha X/Z à origem; desligado, permite movimento livre nesse plano.
- **Surface:** raycast em Colliders da cena ativa nas layers de Settings, ignorando triggers e objetos gerenciados. Sem Collider, usa o plano. Snap encaixa X/Z e reprojeta verticalmente.
- **Free:** não usa snap de posição, embora Ctrl direcional possa ser solicitado explicitamente para Roads/Floors com Snap Enabled.
- **R:** gira pelo incremento configurado. **G:** alterna Snap Enabled. **Escape** ou **Stop Placement:** encerra. Alt e botões de navegação continuam disponíveis. Ctrl+R/G não é consumido pelo sistema de rotação.
- **Rotation Snap:** quantiza rotação Y. O Ctrl direcional não alinha automaticamente a orientação à referência.

BottomCorner usa o canto inferior dos bounds de encaixe orientados, BottomCenter usa o centro inferior, Pivot usa o pivô original e Custom usa o ponto local informado. Offset por prefab e Offset vertical global são aplicados depois da posição calculada; podem causar deslocamentos intencionais em relação às bordas. A configuração de alinhamento à superfície atua no posicionamento normal; Ctrl conserva a pose capturada do ghost.

## Paint e validação

Single coloca uma peça por clique. Brush distribui pelo centro e por amostras no disco horizontal, usando densidade, raio, seed, espaçamento e sobreposição configurados. Rotação aleatória atua fora do snap normal. Uma célula não é reutilizada no mesmo gesto; Undo/Redo agrupa o arrasto. Paint interpola até 32 amostras, com no máximo 100 candidatos por evento; arrastos muito rápidos podem deixar intervalos.

A validação permite contato de bordas com tolerância numérica. Objetos gerenciados são indexados espacialmente; demais obstáculos usam consultas de Collider. O índice é atualizado após hierarquia, transformações, troca de cena e Undo; durante um arrasto, novas instâncias entram imediatamente sem varrer a cena a cada movimento. O modo direcional testa a referência pelas caixas projetadas para não rejeitar contato de módulos girados apenas porque suas AABBs globais se cruzam.

## Persistência, recursos e ferramentas anteriores

Ao posicionar, Box Colliders com malhas MeshFilter disponíveis são substituídos por Mesh Colliders na instância da cena. O prefab original não muda. A substituição participa do mesmo Undo/Redo da colocação. Triggers e objetos com Rigidbody dinâmico usam Convex, portanto nesses casos a colisão é uma envoltória convexa. Sem malha disponível, o Box Collider é preservado. Objetos já colocados não são convertidos retroativamente. O prefab da demonstração também é criado com Mesh Collider. A validação preventiva de encaixe continua usando bounds e consultas físicas conservadoras.

Preferências gerais e perfis por GUID ficam em **UserSettings/CoelhoWorldBuilder.asset**. Perfis armazenam célula manual, override, escala/rotação, offsets, âncora, categoria, alinhamento e snap. Instâncias e metadados de encaixe/ordem ficam na cena, que precisa ser salva. Prefabs originais não são alterados.

Parar, fechar ou recompilar libera material/malhas temporários e remove callbacks. A ferramenta anterior do Unity é restaurada quando aplicável. Play Mode e Prefab Mode não permitem construção; esta entrega usa a cena ativa.

**Grade** cria WorldGrid independente pelo Inspector/Transform: é uma guia visual e não define o snap do painel principal. **Linhas verdes** cria guias poligonais com pontos editáveis e Undo/Redo. Ative Gizmos para essas guias. O Dashboard cria demonstração aditiva em Assets/CoelhoWorldBuilderDemo com nomes únicos; salve antes as cenas sem nome.

## Limitações e conferência manual

Bounds/projeções oferecem encaixe de caixas; não garantem união perfeita de peças curvas, côncavas, irregulares, inclinadas, com shear ou conectores incompatíveis. Para essas peças, ajuste manualmente dimensões, âncoras e offsets. Não há editor de sockets ou detecção topológica automática. Caixas de encaixe menores que a geometria podem permitir sobreposição visual intencional com a referência; obstáculos externos continuam usando seus Colliders/bounds. O contato é calculado nos eixos horizontais da referência.

Ghost suporta MeshFilter/MeshRenderer e SkinnedMeshRenderer em pose inicial; não reproduz VFX, animações ou shaders originais. Buffers físicos têm 256 resultados e saturação bloqueia conservadoramente. Peças antigas criadas antes dos metadados de categoria não são classificadas por nome automaticamente; coloque uma nova peça classificada para iniciar a referência.

Confirme no Editor: aparência/cores/transparência do ghost; grade completa ao trocar 8/15/2; quatro direções com Ctrl físico; ausência de giro/escala inesperados; Ctrl em Paint sem arrasto; liberação do Ctrl; Escape; Alt; overlays; Undo/Redo; excluir/desativar referência; mudar cena; fechar/reabrir janela; reiniciar. Teste Unity 2022.3 e Unity 6, temas claro/escuro e cenas grandes.

Testes NUnit: instale Unity Test Framework no projeto consumidor e inclua `"testables": ["com.coelholabs.worldbuilder"]` no manifest. Execute EditMode em uma cena vazia. Consulte Validation.md para as verificações efetivamente executadas e os harnesses reproduzíveis.
