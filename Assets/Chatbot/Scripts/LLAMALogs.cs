using System.Text;
using LLama.Native;
using Naukri.InspectorMaid;
using Naukri.InspectorMaid.Layout;
using UnityEngine;
using UnityEngine.UIElements;

[
    ScriptField,
    HelpBox(
        @"To verify if the model has been properly loaded into GPU memory, check the log entries beginning with 'llm_load_tensors'. These entries display information about layer offloading and memory allocation across CPU and GPU.",
        HelpBoxMessageType.Info
    ),
    GroupScope("Show logs"),
    HelpBox("", binding: nameof(Log)),
    Button("Copy", binding: nameof(CopyLog)),
    EndScope
]
public class LLAMALog : MonoBehaviour
{
    private StringBuilder logBuilder;

    private string Log => logBuilder?.ToString() ?? "No logs yet!";

    protected virtual void Awake()
    {
        logBuilder = new StringBuilder();
        NativeLibraryConfig.All.WithLogCallback(
            (LLamaLogLevel level, string message) =>
            {
                logBuilder.Append($"[level {level}] {message}");

                if (logBuilder[^1] != '\n')
                {
                    logBuilder.AppendLine();
                }
            }
        );
    }

    public void CopyLog()
    {
#if UNITY_EDITOR
        UnityEditor.EditorGUIUtility.systemCopyBuffer = Log;
#endif
    }
}
