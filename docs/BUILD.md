# QuizCraft - Instrucoes de Build e Instalador

## Pre-requisitos

### Para compilar o projeto

- [.NET 9 SDK](https://dotnet.microsoft.com/pt-br/download/dotnet/9.0) (v9.0 ou superior)
- Windows 10 ou superior

### Para gerar o instalador

- [Inno Setup 6](https://jrsoftware.org/isinfo.php) (v6.0 ou superior)
- Certifique-se de que o comando `iscc` esteja disponivel no PATH do sistema, ou utilize o caminho completo (ex: `"C:\Program Files (x86)\Inno Setup 6\ISCC.exe"`)

## Estrutura de Diretorios

```
QuizCraft/
├── src/
│   └── QuizCraft.Presentation/
│       └── bin/Release/net9.0-windows/publish/   <- saida do publish
├── installer/
│   ├── QuizCraft.iss                              <- script do Inno Setup
│   └── Output/                                    <- instalador gerado
├── docs/
│   └── BUILD.md                                   <- este arquivo
└── QuizCraft.sln
```

## Build do Projeto

### Build padrao (requer .NET Runtime no computador do usuario)

```bash
dotnet publish src/QuizCraft.Presentation/QuizCraft.Presentation.csproj ^
  -c Release ^
  -o src/QuizCraft.Presentation/bin/Release/net9.0-windows/publish/ ^
  --self-contained false
```

> **Nota:** Nesta modalidade, o usuario final precisa ter o [.NET 9.0 Desktop Runtime](https://dotnet.microsoft.com/pt-br/download/dotnet/9.0) instalado no computador. O instalador ira verificar e sugerir o download caso nao esteja presente.

### Build self-contained (nao requer .NET Runtime no computador do usuario)

```bash
dotnet publish src/QuizCraft.Presentation/QuizCraft.Presentation.csproj ^
  -c Release ^
  -o src/QuizCraft.Presentation/bin/Release/net9.0-windows/publish/ ^
  --self-contained true ^
  -r win-x64
```

> **Nota:** O build self-contained inclui o runtime do .NET junto com o aplicativo, resultando em um pacote maior (aproximadamente 150-200 MB), porem elimina a necessidade de instalacao separada do runtime.

### Opcoes adicionais de publish

| Opcao | Descricao |
|-------|-----------|
| `-p:PublishSingleFile=true` | Gera um unico executavel (recomendado para self-contained) |
| `-p:PublishTrimmed=true` | Remove codigo nao utilizado, reduzindo o tamanho final |
| `-p:IncludeNativeLibrariesForSelfExtract=true` | Inclui bibliotecas nativas no executavel unico |
| `-p:EnableCompressionInSingleFile=true` | Comprime o executavel unico |

Exemplo completo com todas as otimizacoes:

```bash
dotnet publish src/QuizCraft.Presentation/QuizCraft.Presentation.csproj ^
  -c Release ^
  -o src/QuizCraft.Presentation/bin/Release/net9.0-windows/publish/ ^
  --self-contained true ^
  -r win-x64 ^
  -p:PublishSingleFile=true ^
  -p:PublishTrimmed=true ^
  -p:EnableCompressionInSingleFile=true
```

## Geracao do Instalador

### Usando a linha de comando

```bash
iscc installer/QuizCraft.iss
```

Ou, caso o Inno Setup nao esteja no PATH:

```bash
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer/QuizCraft.iss
```

### Usando a interface grafica

1. Abra o Inno Setup Compiler
2. Selecione `Arquivo > Abrir` e navegue ate `installer/QuizCraft.iss`
3. Pressione `Ctrl+F9` ou clique em `Build > Compile`

### Saida

O instalador sera gerado em:

```
installer/Output/QuizCraft_Setup_v1.0.0.exe
```

## Processo Completo (Build + Instalador)

Execute os comandos abaixo na raiz do projeto:

```bash
:: 1. Limpar build anterior
dotnet clean src/QuizCraft.Presentation/QuizCraft.Presentation.csproj -c Release

:: 2. Restaurar dependencias
dotnet restore src/QuizCraft.Presentation/QuizCraft.Presentation.csproj

:: 3. Publicar o projeto
dotnet publish src/QuizCraft.Presentation/QuizCraft.Presentation.csproj ^
  -c Release ^
  -o src/QuizCraft.Presentation/bin/Release/net9.0-windows/publish/ ^
  --self-contained false

:: 4. Gerar o instalador
iscc installer/QuizCraft.iss
```

## Diretorios de Dados do Aplicativo

O instalador cria automaticamente os seguintes diretorios no primeiro uso:

| Diretorio | Finalidade |
|-----------|-----------|
| `%APPDATA%\QuizCraft` | Diretorio base de dados do usuario |
| `%APPDATA%\QuizCraft\logs` | Arquivos de log do aplicativo |
| `%APPDATA%\QuizCraft\backups` | Backups automaticos dos dados |
| `%APPDATA%\QuizCraft\attachments` | Anexos e arquivos de midia |

> **Nota:** Durante a desinstalacao, o usuario sera perguntado se deseja remover estes dados.

## Observacoes Importantes

- O script do instalador utiliza `AppMutex` para impedir que o instalador seja executado enquanto o QuizCraft estiver aberto.
- O idioma padrao do instalador e Portugues Brasileiro, com fallback para Ingles.
- O instalador requer Windows 10 ou superior (`MinVersion=10.0`).
- Para distribuicao, recomenda-se o build **self-contained** para evitar problemas de dependencia no computador do usuario final.
- O icone do aplicativo (`quizcraft.ico`) deve estar presente na pasta `assets/` na raiz do projeto antes de compilar o instalador. Caso o icone nao exista, comente a linha `SetupIconFile` no arquivo `.iss`.

## Solucao de Problemas

### Erro: "O .NET Runtime nao foi encontrado"
Instale o [.NET 9.0 Desktop Runtime](https://dotnet.microsoft.com/pt-br/download/dotnet/9.0) ou utilize o build self-contained.

### Erro: "ISCC nao e reconhecido como comando"
Adicione o diretorio do Inno Setup ao PATH do sistema ou utilize o caminho completo do executavel.

### Erro: "Arquivo de origem nao encontrado" durante a compilacao do instalador
Certifique-se de executar o `dotnet publish` antes de compilar o instalador. A pasta `publish/` deve conter os arquivos compilados.

### Erro: "Aplicativo ja esta em execucao"
Feche todas as instancias do QuizCraft antes de executar o instalador ou desinstalador.
