using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LLama;
using LLama.Common;
using LLama.Sampling;
using Naukri.InspectorMaid;
using Naukri.InspectorMaid.Layout;
using UnityEngine;
using UnityEngine.UIElements;

[
    ScriptField,
    Slot(nameof(modelName), nameof(assistantStyle)),
    GroupScope("Parameters"),
    Slot(
        nameof(contextSize),
        nameof(multiThread),
        nameof(threads),
        nameof(gpuLayerCount),
        nameof(batchSize),
        nameof(maxTokens)
    ),
    EndScope,
    RowScope,
    Slot(nameof(Chat)),
    Style(flexGrow: "1"),
    Button("Reset", nameof(ResetSession)),
    EndScope
]
public class Chatbot : MonoBehaviour
{
    [
        HelpBox(
            "You need to locate a '.gguf' model first, place it in the 'StreamingAssets/model' directory, and then type its name here.",
            HelpBoxMessageType.Error
        ),
        ShowIf(nameof(modelName), "")
    ]
    public string modelName = "ggml-model-q6_k";

    [
        HelpBox(
            "You need to locate a '.json' style first, place it in the 'StreamingAssets/style' directory, and then type its name here.",
            HelpBoxMessageType.Error
        ),
        ShowIf(nameof(assistantStyle), "")
    ]
    public string assistantStyle = "catmaid";

    // 上下文長度 -c (Context Length)
    [Rename("contextSize (-c)", useNicifyName: true)]
    public uint contextSize = 2048;

    // 併發數量 -np (Number of Processors)
    [Rename("multiThread", useNicifyName: true)]
    public bool multiThread = false;

    [Rename("threads (-np)", useNicifyName: true), ShowIf(nameof(multiThread))]
    public int threads = 1;

    // GPU 層數 -ngl (Number of GPU Layers)
    [Rename("gpuLayerCount (-ngl)", useNicifyName: true)]
    public int gpuLayerCount = 200;

    // Prompt 數量 -npp (Number of Prompt per Process)
    [Rename("batchSize (-npp)", useNicifyName: true)]
    public uint batchSize = 768;

    // 生成文本數量 -ntg (Number of Tokens to Generate)
    [Rename("maxTokens (-ntg)", useNicifyName: true)]
    public int maxTokens = 384;

    private LLamaWeights model;

    private InferenceParams inferenceParams;

    private LLama.LLamaContext context;

    private ChatSession session;

    private LLama.SessionState resetState;

    protected virtual void Start()
    {
        inferenceParams = new()
        {
            // -ntg
            MaxTokens = maxTokens,
            // 讓 AI 在生成使用者對話時中斷生成 (否則會一直生成一問一答)
            AntiPrompts = new List<string> { "User:" },
            SamplingPipeline = new DefaultSamplingPipeline(),
        };

        StartAsync();
    }

    [Target]
    public void ResetSession()
    {
        EnsureSessionIsCreated();

        session.LoadSession(resetState);
        print("The chat session has been reset.");
    }

    [Target]
    public async void Chat(string message = "C# 是甚麼東西?")
    {
        EnsureSessionIsCreated();

        print($"User: {message}");

        var sb = new StringBuilder();

        var chatSnapshotLength = 100;

        await foreach (
            var text in session.ChatAsync(
                new ChatHistory.Message(AuthorRole.User, message),
                inferenceParams
            )
        )
        {
            if (sb.Length > chatSnapshotLength)
            {
                print($"(snapshot {chatSnapshotLength / 100}) {sb}");
                chatSnapshotLength += 100;
            }
            sb.Append(text);
        }

        // 移除 "User:"
        sb.Length -= "User:".Length;
        // 移除多餘的換行
        while (sb[^1] == '\n')
        {
            sb.Length--;
        }

        print($"Assistant: {sb}");
    }

    private async void StartAsync()
    {
        var actualModelPath =
            $"{Path.Combine(Application.streamingAssetsPath, "Models", modelName)}.gguf";
        var actualChatHistoryJsonPath =
            $"{Path.Combine(Application.streamingAssetsPath, "Styles", assistantStyle)}.json";
        int? actualThread = multiThread ? null : threads;

        var parameters = new ModelParams(actualModelPath)
        {
            // -c
            ContextSize = contextSize,

            // -np
            Threads = actualThread,
            BatchThreads = actualThread,

            // -ngl
            GpuLayerCount = gpuLayerCount,

            // // -npp
            BatchSize = batchSize,
            UBatchSize = batchSize,
        };

        model = LLamaWeights.LoadFromFile(parameters);
        context = model.CreateContext(parameters);

        var executor = new InteractiveExecutor(context);

        // 讀取助手風格
        var chatHistoryJson = File.ReadAllText(actualChatHistoryJsonPath);
        var chatHistory =
            ChatHistory.FromJson(chatHistoryJson)
            ?? throw new IOException("Failed to load assistantStyle.");

        // 不使用 new Session(executor, chatHistory) 因為 chatHistory 的處理會被延後到首次調用 (ChatAsync)
        // 而使用 InitializeSessionFromHistoryAsync 可以預處理好 chatHistory，並儲存狀態供之後重置使用。
        var prototypeSession = await ChatSession.InitializeSessionFromHistoryAsync(
            executor,
            chatHistory
        );

        var transforms = new LLamaTransforms.KeywordTextOutputStreamTransform(
            new string[] { "User:", "Assistant:" },
            redundancyLength: 8
        );

        prototypeSession.WithOutputTransform(transforms);

        resetState = prototypeSession.GetSessionState();
        session = new(executor);
        session.LoadSession(resetState);

        print("The chat session has been created.");
    }

    private void EnsureSessionIsCreated()
    {
        if (session == null)
        {
            throw new InvalidOperationException("The chat session has not been created yet.");
        }
    }
}
