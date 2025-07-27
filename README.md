# FunctionsTest1

Azure Functions の作り方が昔と全然違う気がする

## プロジェクトの作り方

VSCode の拡張機能に Azure Functions というまんまな名前のものがあるので、それを使う。  
[Visual Studio Code を使用して Azure Functions を開発する | Microsoft Learn](https://learn.microsoft.com/ja-jp/azure/azure-functions/functions-develop-vs-code?tabs=node-v4%2Cpython-v2%2Cisolated-process%2Cquick-create&pivots=programming-language-csharp)
に書いてあるが、なんか違うような気がする。

1. まずプロジェクトフォルダを選ぶ（専用のリポジトリを用意したのでプロジェクトフォルダをそれにした）
2. 言語を選択。 C# を選ぶと .NET のバージョンを選ぶことになるので .NET8 を選択。
3. トリガーの選択になるので、 HTTP Trigger を選択。
4. トリガーの名称を書くので、デフォルトの HttpTrigger1 のままにした。
5. namespace を決めることになるので、デフォルトの Company.Function のままにした。
6. 最後に AccessRights とかいうのを聞かれるけど何か分からないので Functions を選択した。

## 違った。こっちだった↓

[Visual Studio Code を使用して C# 関数を作成する - Azure Functions | Microsoft Learn](https://learn.microsoft.com/ja-jp/azure/azure-functions/create-first-function-vs-code-csharp)

書いてある通りに選択していく。

1. `FunctionsTest1` フォルダを作成
2. コマンドパレットを開き、 `Azure Functions: Create New Project...` を実行する
3. プロジェクトフォルダの選択になるので、作成した `FunctionsTest1` を選択する
4. 言語の選択になるので、 `C#` を選択する
5. ランタイムの選択になるので `.NET 8.0 Isolated (LTS)` を選択する
6. トリガーの選択になるので `HTTP Trigger` を選択する
7. トリガーの名称を決めるので、 `HttpExample` にする
8. namespace を決めることにないるので、 `My.Functions` にする
9. AccessRights というのは承認レベルのことらしい。 `Anonymous` にする

あとは実装していく

### パッケージの追加

NuGet パッケージを追加する。以下のコマンドを FunctionsTest1 フォルダに移動してから実行する。

```console
dotnet add package HtmlAgilityPack
```

# Azure REST API を呼び出す HTTP Trigger の追加手順

Azure サービスの REST API を認証付きで呼び出す HTTP Trigger を追加する場合の手順例です。

## 1. 新しい HTTP Trigger 関数の追加

`FunctionsTest1` プロジェクトの `FunctionsTest1` フォルダに新しい C# クラス（例: `ServiceList.cs`）を作成し、以下のような雛形で追加します。

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Core;
using Azure.Identity;

namespace FunctionsTest1
{
    public class ServiceList
    {
        private readonly ILogger _logger;
        private static readonly HttpClient _httpClient = new HttpClient();

        public ServiceList(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ServiceList>();
        }

        [Function("ServiceList")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            var apiUrl = "https://management.azure.com/providers/Microsoft.Support/services?api-version=2024-04-01";
            _logger.LogInformation($"Calling API: {apiUrl}");

            // 環境変数や設定から認証情報を取得
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            // Azure Management API 用のスコープ
            var tokenRequestContext = new TokenRequestContext(new[] { "https://management.azure.com/.default" });
            var accessToken = await credential.GetTokenAsync(tokenRequestContext);

            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken.Token);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"API Response: {content}");

            var res = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await res.WriteStringAsync("API call completed. Check logs for details.");
            return res;
        }
    }
}
```

## 2. 必要な NuGet パッケージの追加

```bash
dotnet add FunctionsTest1 package Azure.Identity
```
→ Azure REST API 認証用のパッケージを追加します。

## 3. Azure Entra ID（旧 Azure AD）でのアプリ登録

1. Azure ポータルで「Azure Entra ID」→「アプリの登録」→「新規登録」
2. アプリケーションID（クライアントID）、ディレクトリID（テナントID）、クライアントシークレットを取得
3. 「API のアクセス許可」で `Azure Service Management` の `user_impersonation` など必要な権限を追加し、管理者の同意を行う

## 4. 環境変数の設定

Azure Functions のローカル実行時は `local.settings.json` に以下のように設定します。

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "AZURE_TENANT_ID": "<テナントID>",
    "AZURE_CLIENT_ID": "<クライアントID>",
    "AZURE_CLIENT_SECRET": "<クライアントシークレット>"
  }
}
```

---

これにより、Azure REST API を認証付きで呼び出す HTTP Trigger 関数を追加できます。

### デプロイ方法

めっちゃ分かりづらいけど以下の手順通り  
[Visual Studio Code を使用して C# 関数を作成する - Azure Functions | Microsoft Learn](https://learn.microsoft.com/ja-jp/azure/azure-functions/create-first-function-vs-code-csharp)

VSCode で Azure にログインして事前に作成しておいた Functions にデプロイした。  
成功したら、 URL が払い出されるので認証なしでアクセスできる。


# DAL プロジェクトの追加と Entity Framework Core のスキャフォールド手順

このプロジェクトでは、SQL Server へのデータアクセス層（DAL）を `FunctionsTest1.DAL` プロジェクトとして分離しています。以下はその構成手順と各コマンドの役割です。

## 1. DAL プロジェクトの作成

プロジェクトフォルダ直下で以下を実行します。

```bash
dotnet new classlib -n FunctionsTest1.DAL -o FunctionsTest1.DAL
```
→ 新しいクラスライブラリ（DAL）プロジェクトを作成します。

```bash
dotnet sln FunctionsTest1.sln add FunctionsTest1.DAL/FunctionsTest1.DAL.csproj
```
→ 作成した DAL プロジェクトをソリューションに追加します。

## 2. プロジェクト参照の追加

```bash
dotnet add FunctionsTest1/FunctionsTest1.csproj reference FunctionsTest1.DAL/FunctionsTest1.DAL.csproj
```
→ FunctionsTest1 から FunctionsTest1.DAL を参照できるようにします。

## 3. Entity Framework Core のセットアップとスキャフォールド

### 必要なツール・パッケージのインストール

```bash
dotnet tool install --global dotnet-ef
```
→ Entity Framework Core のコマンドラインツールをグローバルインストールします。

```bash
dotnet add FunctionsTest1.DAL package Microsoft.EntityFrameworkCore.SqlServer
```
→ SQL Server 用の EF Core プロバイダーを追加します。

```bash
dotnet add FunctionsTest1.DAL package Microsoft.EntityFrameworkCore.Design
```
→ スキャフォールドやマイグレーションに必要なデザインパッケージを追加します。

### スキャフォールド（DbContext/エンティティ自動生成）

```bash
dotnet ef dbcontext scaffold "Server=192.168.33.150,1433;Database=TestDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;" Microsoft.EntityFrameworkCore.SqlServer --output-dir Models --context-dir Contexts --context TestDbContext --force --project FunctionsTest1.DAL/FunctionsTest1.DAL.csproj
```
→ 指定した SQL Server からスキーマ情報を取得し、DAL プロジェクト内に `DbContext` とエンティティクラスを自動生成します。

> ※ `--project` オプションを付けることで、DAL プロジェクトを明示的に指定しています。
