# Easy Level Design Tool
## Manual de instruções • versão 1.3.1 • Coelho Labs

Ferramenta de construção de cenários para o Unity Editor. Compatibilidade-alvo: Unity 2022.3 LTS e Unity 6. Você usa seus próprios prefabs; não precisa de assets pagos para as operações básicas.

## 1. Baixar e instalar

Opção recomendada — pacote .tgz:

1. Na release do GitHub, baixe com.coelholabs.worldbuilder-1.3.1.tgz.
2. No Unity, abra Window > Package Manager.
3. Clique em + > Add package from tarball e escolha o arquivo .tgz baixado.
4. Aguarde a importação e a compilação.
5. Abra Tools > Coelho Labs > Easy Level Design Tool.

Alternativa — arquivo ZIP:

1. Baixe Easy-Level-Design-Tool-1.3.1.zip e extraia seu conteúdo para uma pasta permanente.
2. No Unity, abra Window > Package Manager > + > Add package from disk.
3. Selecione CoelhoWorldBuilder/package.json dentro da pasta extraída.
4. Mantenha essa pasta no disco: a instalação por disco depende dela.

O ZIP não é um arquivo .unitypackage. Não use Assets > Import Package para instalar este pacote UPM. Instale uma única cópia; evite manter simultaneamente cópias em Assets e no Package Manager.

Se já houver uma instalação por Git ou disco, remova a entrada do pacote no Package Manager antes de escolher outra origem. Isso remove a referência ao pacote, não seus prefabs ou cenas. Reabra a janela da ferramenta após atualizar.

## 2. Primeiro uso em poucos minutos

1. Abra o painel Smart Grid & Directional Placement.
2. Em Prefab selecionado, escolha a raiz de um prefab do Project, não um objeto da Hierarchy.
3. Deixe Auto Grid Size e Snap Enabled ligados.
4. Para módulos regulares, use âncora BottomCorner e escala aleatória mín/máx igual a 1/1.
5. Clique em Activate Placement e coloque o foco na Scene View.
6. Mova o mouse: a prévia transparente mostra onde a peça ficará. Clique para confirmar e continue clicando para colocar cópias.
7. Pressione Escape ou clique em Stop Placement para encerrar. Salve a cena.

Em Grid, não é necessário Collider: a ferramenta usa o plano horizontal da grade. Em Surface, ela tenta os Colliders da cena ativa e usa o plano como alternativa.

Não tem prefabs para experimentar? No Dashboard, use Criar demonstração. Salve antes todas as cenas sem nome. A ferramenta cria um cubo e uma nova cena aditiva em Assets/CoelhoWorldBuilderDemo, mantendo as outras cenas abertas.

## 3. Organizar a biblioteca

As pastas são virtuais e não movem os arquivos originais. Abra Asset Library ou a biblioteca no painel de construção.

- Criar: escreva um nome e clique em + Criar categoria.
- Expandir/recolher: use a seta da pasta.
- Adicionar: arraste um ou vários prefabs raiz do Project para a pasta. Duplicatas na mesma pasta são ignoradas.
- Ativar: expanda a pasta e clique em Ativar. Essa será a categoria dos atalhos.
- Selecionar: clique no nome de um prefab; sua pasta também passa a ser ativa.
- Renomear: edite o nome da pasta e confirme com Enter ou saindo do campo.
- Reordenar: use ↑/↓ nas categorias e referências.
- Transferir: use Mover e escolha outra pasta.
- Remover: × remove a referência; Excluir remove a pasta e suas referências. Os assets continuam no Project.

A biblioteca anterior é importada uma vez para Outros. Uma referência marcada [Ausente] aponta para um asset que não foi encontrado: remova a entrada ou restaure o arquivo junto com seu .meta original.

## 4. Trocar de peça sem voltar ao painel

Com construção ativa e foco na Scene View:

- Ctrl + Q: prefab anterior da categoria ativa.
- Ctrl + E: próximo prefab da categoria ativa.

A navegação é circular. No último item, E volta ao primeiro; no primeiro, Q vai ao último. Referências ausentes são ignoradas e a navegação nunca troca de categoria por conta própria.

O indicador mostra categoria, prefab, índice/total e tamanho da grade. A troca atualiza o ghost e a grade automática, preservando a origem e os objetos colocados. Em modo manual, a dimensão de grade escolhida é mantida.

Os atalhos não funcionam com a construção parada ou outra janela em foco. Evite modificadores adicionais Alt, Shift ou Command. Durante edição de texto, eles devem permanecer disponíveis para o campo, sem trocar prefabs.

## 5. Configurar a grade adaptativa

Auto Grid Size ajusta a grade inteira pelas dimensões geométricas do prefab selecionado, considerando filhos, pivô, rotação e escala padrão. Exemplo: selecionar uma rua 15 × 15 e depois um detalhe 2 × 2 muda as células de 15 × 15 para 2 × 2. A origem e as peças existentes não se movem.

- Show Grid: mostra ou oculta as linhas, sem desligar o snap.
- Grid Width / Grid Length: largura X e comprimento Z de cada célula. Editar esses campos desliga Auto Grid Size.
- Grid Origin: posição fixa da origem no mundo.
- Grid Height: altura somada à origem Y.
- Área de exibição: tamanho da região desenhada; há um limite de linhas por eixo.
- Snap Enabled: ativa/desativa encaixe para o prefab atual.
- Rotation Snap: ajusta o giro Y ao incremento configurado.

Para ignorar limites visuais inadequados, habilite Dimensões de encaixe manuais por prefab e preencha Manual Grid Size X/Z. Esses valores são medidos após a escala original da raiz, antes da escala padrão do perfil e da rotação. Para uma grade global fixa, desligue Auto Grid Size. Apply Suggested Grid Size aplica a célula do perfil no modo manual.

Auxiliares podem distorcer a dimensão detectada. No auxiliar, adicione o componente Coelho Labs > Ignore Placement Dimensions para ignorar seu ramo na análise. Isso não apaga o objeto do prefab.

## 6. Construir ruas e pisos com Ctrl

As pastas da biblioteca e Snap Category são configurações diferentes. Colocar uma peça na pasta Ruas não altera automaticamente sua classificação modular.

1. Selecione a rua e defina Snap Category como Roads. Para pisos, use Floors; para objetos comuns, mantenha Decorative.
2. Ative Enable Ctrl Directional Snap e Snap Enabled.
3. Coloque a primeira peça normalmente, sem Ctrl. Ela se torna a referência.
4. Segure Ctrl e mova o mouse à direita, à esquerda, à frente ou atrás da referência.
5. A prévia encosta na borda correspondente sem girar ou redimensionar automaticamente. Clique para colocar.
6. A nova peça passa a ser a referência. Solte Ctrl para voltar ao posicionamento normal.

As direções seguem os eixos horizontais da referência. A referência precisa estar ativa, na cena ativa e na mesma categoria modular. Colocar decoração não substitui a última rua/piso.

Ctrl + Q/E continua trocando o prefab e não coloca objetos. Mantendo Ctrl pressionado, o encaixe direcional utiliza a mesma referência após a troca. Mesmo em Paint, Ctrl só coloca por clique, nunca pelo arrasto.

Edge Alignment alinha as bordas laterais; desligado, centraliza a peça no eixo perpendicular. Snap Offset aplica um deslocamento em coordenadas globais. Valores que causem penetração na referência deixam a posição inválida; valores que afastem a peça podem criar uma folga intencional.

Peças de tamanhos diferentes e pivôs deslocados usam limites transformados. Para peças regulares 8 × 8, mantenha escala aleatória em 1/1 e offsets em zero para começar.

## 7. Âncoras, alinhamento e conexões manuais

- BottomCorner: canto inferior da caixa de encaixe orientada; recomendado para ruas e pisos.
- BottomCenter: centro inferior da peça.
- Pivot: pivô original do prefab.
- Custom: ponto local informado em Âncora local personalizada.

Offset por prefab e Offset vertical global são somados à posição. Eles podem deslocar a peça para fora da grade. Alinhar à superfície segue a normal no posicionamento normal; o Ctrl preserva a pose da prévia durante o gesto.

Para conexões manuais, ligue Usar pontos de conexão locais e informe Right, Left, Forward e Back no espaço local da raiz. Configure e coloque a referência com esse recurso ligado. Se ambas as peças tiverem pontos, o sistema une o ponto direcional da referência ao ponto oposto da nova peça. Caso contrário, usa limites geométricos. Os pontos são copiados para a instância ao colocá-la; editar o perfil depois não altera peças já colocadas.

## 8. Posicionamento e pintura

- Single: cada clique coloca uma instância.
- Brush/Paint: clique e arraste para distribuir peças com raio, densidade, espaçamento e seed configurados.
- Grid: plano horizontal fixo; snap em X/Z quando habilitado.
- Surface: raycast em Colliders permitidos pela máscara de layers, com plano alternativo.
- Free: posicionamento normal sem snap de posição. Ctrl direcional continua disponível quando solicitado para peças modulares compatíveis.

Densidade é o máximo de candidatos por carimbo, não objetos por metro quadrado. Espaçamento e Evitar sobreposição podem reduzir a quantidade colocada. No snap, a mesma célula não é repetida durante um gesto. Ctrl+Z desfaz o arrasto inteiro.

Rotação aleatória atua fora do snap normal. Escala aleatória multiplica a escala padrão e a original. Para encaixe modular previsível, use escala aleatória 1/1. Movimentos muito rápidos podem deixar intervalos porque o processamento por evento é limitado.

## 9. Referência rápida de atalhos

- Clique esquerdo: colocar.
- Arrastar com botão esquerdo: pintar no modo Brush, sem Ctrl.
- Ctrl: encaixe direcional Roads/Floors.
- Ctrl + Q / Ctrl + E: anterior / próximo na categoria ativa.
- R: girar pelo incremento configurado.
- G: alternar Snap Enabled.
- Escape: parar a construção.
- Alt: navegar na Scene View com os controles habituais.
- Ctrl + Z / Ctrl + Y: desfazer / refazer, conforme o mapeamento do Unity.

## 10. Mesh Colliders e objetos gerados

Novas instâncias substituem Box Colliders por Mesh Colliders quando há MeshFilters disponíveis. O prefab original permanece intacto. Triggers e objetos com Rigidbody dinâmico usam Convex, portanto nesses casos a forma de colisão é convexa. Sem malha disponível, o Box Collider é preservado.

A conversão participa do Undo/Redo. Objetos colocados antes da mudança não são convertidos retroativamente. A validação preventiva usa limites e consultas físicas conservadoras; ela pode bloquear algumas geometrias mesmo quando suas malhas não se cruzam.

## 11. Grade e linhas independentes

A página Grade cria uma guia WorldGrid editável pelo Inspector e pelo Transform. Essa guia não define o snap do painel principal. Ative Gizmos para vê-la.

Linhas verdes cria guias poligonais. Selecione a linha, arraste os pontos ou use Adicionar pontos na Scene View. Informe um índice no Inspector para remover um ponto. É possível fechar o traçado e editar cor e espessura. São guias do editor, não estradas prontas nem linhas renderizadas no jogo.

## 12. Salvar, compartilhar e atualizar

- Salve a cena para manter instâncias, guias e metadados de geração.
- Categorias, referências por GUID e sua ordem: ProjectSettings/CoelhoWorldBuilderLibrary.asset.
- Preferências gerais e perfis por prefab: UserSettings/CoelhoWorldBuilder.asset.
- Prefabs e seus arquivos .meta devem acompanhar o projeto para manter os GUIDs.

Ao compartilhar, inclua a biblioteca de ProjectSettings, as cenas e os prefabs. UserSettings costuma ser ignorada no versionamento; compartilhe suas preferências separadamente se desejar. Os caminhos internos antigos foram preservados após a mudança de nome para manter compatibilidade.

Para atualizar, substitua a instalação do pacote pela nova versão. Não apague a biblioteca do projeto nem as cenas. Prefabs originais não são regravados pela ferramenta.

## 13. Resolver problemas comuns

- Não aparece prévia: confirme Activate Placement, um prefab raiz válido e foco na Scene View. Saia de Play Mode e Prefab Mode. Confira erros no Console.
- Prévia vermelha: verifique objetos existentes, espaçamento, Evitar sobreposição, offsets e mensagem ao lado da prévia. Com Ctrl, confirme uma referência da mesma categoria.
- Ctrl não encaixa: defina Roads/Floors, ligue os dois controles de snap e coloque uma primeira referência sem Ctrl. A referência não pode estar desativada ou em outra cena.
- Ctrl+Q/E não troca: confirme categoria ativa com referências válidas, construção ativa e foco na Scene View; saia do campo de texto.
- Grade não muda: ligue Auto Grid Size e confira o override manual, escala e geometria do prefab. A guia da página Grade é independente da grade de snap.
- Peças apresentam folgas: comece com BottomCorner, offsets zero e escala aleatória 1/1. Confira dimensões, rotação e auxiliares da malha. Ajuste dimensões ou conexões manualmente quando necessário.
- Superfície não é detectada: use Surface, confira Collider habilitado e a máscara de layers em Settings. Triggers e objetos gerenciados são ignorados pelo raycast.
- Prefab aparece como [Ausente]: restaure o asset e seu .meta original ou remova e adicione novamente a referência.
- Demo não é criada: salve antes todas as cenas abertas que ainda não têm nome.
- Menu não aparece: confirme a instalação no Package Manager e a ausência de erros de compilação. O menu fica sob Tools > Coelho Labs.

## 14. Limites da versão

A ferramenta atual oferece posicionamento, pintura, grade, biblioteca e encaixe modular. Não oferece geração procedural de cidades/estradas, fratura de malhas ou editor visual de regras.

O encaixe por caixas/projeções não garante união perfeita de peças curvas, côncavas, inclinadas, irregulares ou com conectores incompatíveis. O ghost cobre MeshRenderer/MeshFilter e SkinnedMeshRenderer em pose inicial, sem reproduzir VFX, animações ou shaders originais. A construção atua somente na cena ativa e é bloqueada em Prefab Mode e Play Mode.

Testes automatizados em Unity 2022.3.22f1 e 6000.0.76f1 cobriram os sistemas principais. Eles não substituem a conferência visual dos gestos, do ghost e dos atalhos no seu projeto. Consulte Documentation/Validation.md para os resultados e limites dos ensaios. Consulte LICENSE.md para os termos de uso do código.
