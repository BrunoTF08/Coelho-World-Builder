# Coelho World Builder · Coelho Labs

Versão 1.3.0. Pacote UPM para Unity 2022.3 LTS e Unity 6, com ferramentas locais de construção de cenários.

## Instalação

No Unity: **Window > Package Manager > + > Add package from disk**. Selecione `CoelhoWorldBuilder/package.json`. Abra **Tools > Coelho Labs > Coelho World Builder**. Se a janela já estava aberta, reabra-a para atualizar a navegação.

## Smart Grid & Directional Placement

Selecione um prefab da biblioteca ou do Project, ative Placement e clique na Scene View. Prévia transparente, grade verde fixa, grade adaptativa automática, encaixe por âncora, Ctrl direcional para Roads/Floors e pintura com Undo/Redo. Funciona em plano sem Collider e em superfícies com Collider. R gira; G alterna snap; Escape encerra.

[Guia de uso](Documentation/GettingStarted.md) · [Validação e limites](Documentation/Validation.md)

A grade de posicionamento usa preferências do projeto; as grades e linhas independentes criadas anteriormente continuam disponíveis e salvas nas cenas. Sistemas de cidades, estradas geradas, destruição, regras e otimização permanecem planejados.

## Arquitetura

- Editor/Core: preferências e perfis por GUID, logs, criação da demo.
- Editor/UI: janela UI Toolkit e painel Placement & Smart Grid.
- Editor/Placement: posicionamento, ghost, matemática de bounds/snap e edição de guias.
- Runtime/Components: metadados e componentes de autoria; sem comportamento de construção em runtime.
- Tests/Editor: testes NUnit para a lógica principal.
- Tools/Validation na raiz do workspace: harness reproduzível de integração em projetos isolados.


