# Easy Level Design Tool

Extensão de construção de cenários 3D para Unity Editor, desenvolvida pela **Coelho Labs**. Versão **1.3.1**, destinada a Unity **2022.3 LTS** e **Unity 6**.

## Download e manual

- [Baixar pacote .tgz para o Package Manager](https://github.com/BrunoTF08/Coelho-World-Builder/releases/download/v1.3.1/com.coelholabs.worldbuilder-1.3.1.tgz)
- [Baixar ZIP com o pacote e manual offline](https://github.com/BrunoTF08/Coelho-World-Builder/releases/download/v1.3.1/Easy-Level-Design-Tool-1.3.1.zip)
- [Baixar manual HTML para abrir no navegador](https://github.com/BrunoTF08/Coelho-World-Builder/releases/download/v1.3.1/Manual-Easy-Level-Design-Tool.html)
- [Ler o manual de instruções em português](CoelhoWorldBuilder/Documentation/Manual-pt-BR.md)

Para instalar o .tgz: **Window > Package Manager > + > Add package from tarball**. Para o ZIP, extraia e use **Add package from disk** com `CoelhoWorldBuilder/package.json`. O ZIP não é um arquivo .unitypackage.

## Instalação

No Unity, abra **Window > Package Manager > + > Add package from git URL** e informe:

```text
https://github.com/BrunoTF08/Coelho-World-Builder.git?path=/CoelhoWorldBuilder
```

Também é possível baixar o repositório e usar **Add package from disk**, selecionando `CoelhoWorldBuilder/package.json`.

Abra **Tools > Coelho Labs > Easy Level Design Tool**.

## Recursos implementados

- Posicionamento Single/Paint, prévia transparente e Undo/Redo.
- Smart Adaptive Grid com origem fixa, dimensões automáticas/manuais e snap por âncora.
- Ctrl direcional para encaixar ruas/pisos à última peça modular.
- Biblioteca de categorias virtuais, arrastar/soltar e Ctrl+Q/E na categoria ativa.
- Mesh Colliders nas novas instâncias quando há malhas disponíveis.
- Guias de grade/linhas editáveis, configurações persistentes e criador de demonstração.

Geração de cidades, estradas procedurais, destruição, editor visual de regras e otimização avançada permanecem planejados.

## Documentação

- [Guia de uso e limitações](CoelhoWorldBuilder/Documentation/GettingStarted.md)
- [Validação executada](CoelhoWorldBuilder/Documentation/Validation.md)
- [Histórico de alterações](CoelhoWorldBuilder/CHANGELOG.md)
- [Licença](CoelhoWorldBuilder/LICENSE.md)

`CoelhoWorldBuilder/` contém o pacote UPM. `Tools/Validation/` contém harnesses de integração para projetos isolados; eles criam assets de teste e redefinem configurações, portanto consulte suas instruções antes de executar. Projetos e logs locais de validação não fazem parte do repositório.

As verificações automatizadas foram executadas em Unity 2022.3.22f1 e 6000.0.76f1. Testes em batch não substituem a conferência visual do ghost, dos gestos e dos atalhos físicos no Editor.
