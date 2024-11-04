# Chatbot

基於 [LLamaSharp](https://github.com/SciSharp/LLamaSharp) 開發的簡易聊天機器人。

https://github.com/user-attachments/assets/b284a521-603f-4b62-a782-b2752d1a7c95

## 快速開始

1. 開啟專案，首次啟動會報錯，選擇 Ignore 無視掉。進去後 PackageManager 會自動下載相依套件安裝完後應該就不會報錯了，如果還是不行就自己手動：
    1. 安裝 [InspectorMaid](https://github.com/naukri7707/InspectorMaid)  和 [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity)。
    2. 用 NuGetForUnity 安裝 LLamaSharp 與對應的 backend。
2. 預設 backend 是 CUDA12，你需要安裝對應的 [CUDA Toolkit](https://developer.nvidia.com/cuda-toolkit-archive)  才能正常工作。

   > 如果想用其他版本的 backend 可以在 NuGetForUnity 面板搜尋 LLamaSharp 安裝對應的 backend，按照官方說法當前版本是可以同時存在兩個 backend 讓 LLamaSharp 自己選擇，但實際情況會有些問題所以建議裝一個就好。
3. 準備一個 llama 模型（gguf 格式），沒有的話可以選擇以下任一模型使用：

   - [羊駝大模型](https://github.com/ymcui/Chinese-LLaMA-Alpaca-3) （比較聰明，但過激的對話內容會受限​）
   - [櫻花大模型](https://github.com/SakuraLLM/SakuraLLM) （比較呆，不過對話內容不會被限制）
4. 將模型放到 `Assets/StreamingAssets/Models` 中。

   > 注意 : 把大型檔案放在 Assets 中 Unity 會卡一段時間，要等一下。
5. 把 `Chatbot` 組件的 `modelName` 欄位改成對應的模型名稱即可。

##  建立助手風格

>  你可以在 `Assets/StreamingAssets/Styles` 找到當前所有的助手風格設定檔來參考。

1. 在 `Assets/StreamingAssets/Styles` 新增一個目標風格的 JSON 設定檔，其中 `author_role` 為講者可以是 `System` 、 `User` 或 `Assistant`， `content` 則為對話內容；所有的對話資料皆應儲存在 `messages` 陣列之中。

```
{
  "messages": [
    {
      "author_role": "<author_role>",
      "content": "<content>"
    }
  ]
}
```

1. 開頭用 System 來描述助手的對話風格、邏輯和人設。
2. 之後用 `User` 和 `Assistant` 交替對話來方式建立具體的模範對話，建立時請注意以下幾點：

   - 必須是  `User` -> `Assistant` -> `User` ... 這種對話方式，不能有類似 `User` -> `User` 講者連說兩次話的情況。
   - 一般來說，如果對話內容比較長的話 2~3 組即可、較短的話可以適當增加；只要能讓 AI 回應固化為模範對話的格式即可，過多的對話只會徒增記憶體使用量。
3. 設定完成後把 `Chatbot` 組件的 `assistantStyle` 欄位改成對應的風格名稱即可。
