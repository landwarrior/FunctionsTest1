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
