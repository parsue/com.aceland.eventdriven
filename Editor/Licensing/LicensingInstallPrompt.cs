using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace AceLand.EventDriven.Editor.Licensing
{
    /// <summary>
    /// 未安裝 <c>com.aceland.licensing</c> 時的引導 UI。付費 Editor 工具在缺少共用授權
    /// Core 時,以此提示使用者安裝(一鍵 UPM 安裝),而非拋錯。安裝完成後 Unity 重編譯,
    /// source 橋接便會注入真實 gate,工具即恢復授權路徑。
    ///
    /// Runtime / build 完全不受影響——本提示僅存在於 Editor 付費工具的進入點。
    /// </summary>
    internal static class LicensingInstallPrompt
    {
        /// <summary>選用授權 Core 的套件名稱。</summary>
        public const string LicensingPackageName = "com.aceland.licensing";

        // 以 git URL 安裝:空專案未設定 OpenUPM scoped registry 時,單以套件名
        // Client.Add("com.aceland.licensing") 無任何 registry 可解析而靜默失敗。
        // git URL 不需 scoped registry 即可解析,為最可靠的一鍵安裝來源。
        private const string LicensingGitUrl =
            "https://github.com/parsue/com.aceland.licensing.git";

        // 日誌前綴(此產品)。
        private const string LogPrefix = "[AceLand.EventDriven]";

        // 進行中的 UPM Add 請求(避免重複點擊觸發多次安裝)。
        private static AddRequest _request;

        // 缺套件時 UI 顯示用的說明文字(不觸碰任何 Licensing 型別)。
        private const string MissingMessage =
            "This Editor tool requires the shared AceLand Licensing package " +
            "(com.aceland.licensing), which is not installed.\n\n" +
            "Core and runtime are unaffected: your project keeps compiling, " +
            "building and running.";

        /// <summary>選單類進入點:彈窗提示安裝,按「Install」即透過 UPM 安裝。</summary>
        public static void PromptInstall(string toolName)
        {
            var install = EditorUtility.DisplayDialog(
                toolName,
                MissingMessage + "\n\nInstall AceLand Licensing now?",
                "Install Licensing",
                "Cancel");

            if (install) InstallLicensing();
        }

        /// <summary>工具視窗內繪製「需安裝 Licensing」引導頁,取代工具內容。</summary>
        public static void DrawInstallPage(string toolName)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField(toolName, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(MissingMessage, MessageType.Warning);
            EditorGUILayout.Space(6);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Install AceLand Licensing", GUILayout.Height(28)))
                    InstallLicensing();
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField(
                "After installation Unity recompiles and this tool unlocks its license flow.",
                EditorStyles.wordWrappedMiniLabel);
        }

        // 透過 UPM 新增 Licensing 套件。Client.Add 為非同步;安裝後 Unity 觸發重編譯,
        // 橋接的 versionDefines 生效並注入真實 gate。
        //
        // 以 git URL 安裝而非單純套件名:空專案(或未設定 OpenUPM scoped registry
        // 的專案)以套件名安裝時,UPM 無 registry 可解析而靜默失敗(僅印出本行 Log
        // 卻未安裝)。git URL 不依賴任何 registry,是最可靠的一鍵安裝來源。
        private static void InstallLicensing()
        {
            if (_request is { IsCompleted: false })
            {
                Debug.Log($"{LogPrefix} {LicensingPackageName} install already in progress…");
                return;
            }

            Debug.Log($"{LogPrefix} Installing {LicensingPackageName} via UPM ({LicensingGitUrl})…");
            _request = Client.Add(LicensingGitUrl);
            EditorApplication.update += PollInstall;
        }

        // 輪詢 UPM Add 結果並回報成功 / 失敗,讓使用者在空專案也能明確得知結果,
        // 而非只見到一行「Installing…」卻無後續。
        private static void PollInstall()
        {
            if (_request is not { IsCompleted: true }) return;

            EditorApplication.update -= PollInstall;

            if (_request.Status == StatusCode.Success)
            {
                Debug.Log(
                    $"{LogPrefix} Installed {_request.Result.packageId}. " +
                    "Unity will recompile and this tool unlocks its license flow.");
            }
            else
            {
                Debug.LogError(
                    $"{LogPrefix} Failed to install {LicensingPackageName}: " +
                    $"{_request.Error?.message}\n" +
                    "You can add it manually via Package Manager → Add package from git URL: " +
                    $"{LicensingGitUrl}");
            }

            _request = null;
        }
    }
}
