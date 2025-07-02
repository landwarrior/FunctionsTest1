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

### デプロイ方法

めっちゃ分かりづらいけど以下の手順通り  
[Visual Studio Code を使用して C# 関数を作成する - Azure Functions | Microsoft Learn](https://learn.microsoft.com/ja-jp/azure/azure-functions/create-first-function-vs-code-csharp)

VSCode で Azure にログインして事前に作成しておいた Functions にデプロイした。  
成功したら、 URL が払い出されるので認証なしでアクセスできる。
