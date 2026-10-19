using System;

namespace AceLand.EventDriven.Editor.Licensing
{
    /// <summary>
    /// 授權閘門抽象(選用依賴隔離層)。
    ///
    /// 這個介面是「編輯器工具」與「共用 Licensing Core(選用套件)」之間唯一的接縫。
    /// 工具程式碼一律只透過此介面取用授權狀態,**不直接參考 <c>AceLand.Licensing.*</c>**,
    /// 因此當使用者未安裝 <c>com.aceland.licensing</c> 時,本工具仍可乾淨載入
    /// (不會 TypeLoadException / 不洗版 console)。
    ///
    /// 真實實作(<c>RealLicenseGate</c>)位於由 Unity 編譯、以 versionDefines 條件編譯的
    /// source 橋接(<c>AceLand.EventDriven.LicenseBridge</c>);當 Licensing 缺席時,
    /// <see cref="LicenseGate.Current"/> 維持 fail-closed 的 <see cref="MissingLicenseGate"/>。
    /// </summary>
    internal interface ILicenseGate
    {
        /// <summary>目前是否有權使用付費工具。純讀快取,絕不丟例外。</summary>
        bool IsEntitled { get; }

        /// <summary>授權狀態變更通知(供 window repaint)。</summary>
        event Action Changed;

        /// <summary>於付費進入點 / 視窗開啟時呼叫,受節流。</summary>
        void EnsureChecked();

        /// <summary>選單類進入點:未授權時彈提示並開授權視窗,回傳是否放行。</summary>
        bool EnsureEntitledForMenu(string toolName);

        /// <summary>付費工具視窗內未授權時繪製的引導頁,取代工具內容。</summary>
        void DrawBlockedPage(string toolName);

        /// <summary>開啟授權管理視窗(選單 / 引導頁按鈕使用)。</summary>
        void OpenLicenseWindow();
    }
}
