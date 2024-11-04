using System.Text.Json;
using LLama.Common;

public static class ChatHistoryExtensions
{
    // 如果紀錄中包含非 ASCII 字元，則使用此方法，否則該字元會被強制轉譯成 ASCII 從而導致難以閱讀
    public static string ToJsonWithoutEnsureAscii(this ChatHistory chatHistory)
    {
        var options = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        return ToJson(chatHistory, options);
    }

    public static string ToJson(this ChatHistory chatHistory, JsonSerializerOptions options)
    {
        return JsonSerializer.Serialize(chatHistory, options);
    }
}
