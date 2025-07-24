using Microsoft.Extensions.Logging;
using FunctionsTest1.Models;
using System.Xml;
using System.Globalization;

namespace FunctionsTest1;

public class Actions
{
    private static readonly HttpClient _httpClient = new();
    private static readonly ILogger _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<Actions>();

    // 日本時間に調整（Actions.pyと同じ）
    private static readonly DateTime Now = DateTime.UtcNow.AddHours(9);
    private static readonly DateTime Yesterday = Now.AddDays(-1).AddMinutes(-3);
    private static readonly string Header = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.88 Safari/537.36";

    /// <summary>
    /// XMLのタグ名を基に文字列を取得（Actions.pyのget_text関数）
    /// </summary>
    private static string GetText(XmlNodeList nodes, string tagName)
    {
        foreach (XmlNode node in nodes)
        {
            if (node.Name.ToLower().Contains(tagName.ToLower()))
            {
                return node.InnerText;
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// アットマークITの全フォーラムの新着記事（Actions.pyのaitNewAll）
    /// </summary>
    public static async Task<List<Content>> AitNewAllAsync()
    {
        var url = "https://rss.itmedia.co.jp/rss/2.0/ait.xml";
        _logger.Info($"GET {url} header: {Header}", "AitNewAllAsync");
        var contents = new List<Content>();

        try
        {
            var response = await _httpClient.GetAsync(url);
            var xmlContent = await response.Content.ReadAsStringAsync();
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);

            var items = xmlDoc.GetElementsByTagName("item");
            foreach (XmlNode child in items)
            {
                var title = GetText(child.ChildNodes, "title");
                if (title.StartsWith("PR:") || title.StartsWith("PR： "))
                    continue;

                var pubDateStr = GetText(child.ChildNodes, "pubDate");
                if (DateTime.TryParseExact(pubDateStr[..25], "ddd, dd MMM yyyy HH:mm:ss",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var pubDate))
                {
                    if (Yesterday <= pubDate)
                    {
                        contents.Add(new Content
                        {
                            Title = title,
                            Link = GetText(child.ChildNodes, "link")
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Error in AitNewAllAsync", ex);
        }

        return contents;
    }

    /// <summary>
    /// アットマークITの本日の総合ランキング（Actions.pyのaitRanking）
    /// </summary>
    public static async Task<List<Content>> AitRankingAsync()
    {
        var url = "https://www.atmarkit.co.jp/json/ait/rss_rankindex_all_day.json";
        _logger.Info($"GET {url} header: {Header}");
        var contents = new List<Content>();

        try
        {
            var response = await _httpClient.GetAsync(url);
            var jsonStr = await response.Content.ReadAsStringAsync();
            jsonStr = jsonStr.Replace("rankingindex(", "").Replace(")", "").Replace("'", "\"");

            using var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonStr);
            var data = jsonDoc.RootElement.GetProperty("data");

            int count = 0;
            foreach (var item in data.EnumerateArray())
            {
                if (count >= 10) break;
                if (item.ValueKind != System.Text.Json.JsonValueKind.Null)
                {
                    contents.Add(new Content
                    {
                        Title = item.GetProperty("title").GetString()?.Replace(" ", "") ?? "",
                        Link = item.GetProperty("link").GetString() ?? ""
                    });
                    count++;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Error in AitRankingAsync", ex);
        }

        return contents;
    }

    /// <summary>
    /// ITmedia NEWS 最新記事一覧（Actions.pyのitmediaNews）
    /// </summary>
    public static async Task<List<Content>> ItmediaNewsAsync()
    {
        var url = "https://rss.itmedia.co.jp/rss/2.0/news_bursts.xml";
        _logger.Info($"GET {url} header: {Header}");
        var contents = new List<Content>();

        try
        {
            var response = await _httpClient.GetAsync(url);
            var xmlContent = await response.Content.ReadAsStringAsync();
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);

            var items = xmlDoc.GetElementsByTagName("item");
            foreach (XmlNode child in items)
            {
                var pubDateStr = GetText(child.ChildNodes, "pubDate");
                if (DateTime.TryParseExact(pubDateStr[..25], "ddd, dd MMM yyyy HH:mm:%S",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var pubDate))
                {
                    if (Yesterday <= pubDate)
                    {
                        contents.Add(new Content
                        {
                            Title = GetText(child.ChildNodes, "title"),
                            Link = GetText(child.ChildNodes, "link")
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Error in ItmediaNewsAsync", ex);
        }

        return contents;
    }

    /// <summary>
    /// Qiita新着記事取得（Actions.pyのqiita）
    /// </summary>
    public static async Task<List<Content>> QiitaAsync()
    {
        var url = "https://qiita.com/api/v2/items?page=1&per_page=3";
        _logger.Info($"GET {url} header: {Header}");
        var contents = new List<Content>();

        try
        {
            var response = await _httpClient.GetAsync(url);
            var jsonStr = await response.Content.ReadAsStringAsync();

            using var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonStr);
            foreach (var item in jsonDoc.RootElement.EnumerateArray())
            {
                contents.Add(new Content
                {
                    Title = item.GetProperty("title").GetString() ?? "",
                    Link = item.GetProperty("url").GetString() ?? ""
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in QiitaAsync");
        }

        return contents;
    }

    // 他のメソッドも同様に実装できます...
}
