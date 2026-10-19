using UnityEditor;

namespace AceLand.EventDriven.Editor.Licensing
{
    /// <summary>
    /// EventDriven 的 License 選單(唯一定義處)。共用 Core 的 <c>LicenseWindow</c> 不含任何
    /// [MenuItem](多產品共用同一 DLL),故由本產品掛選單,並路由到目前生效的
    /// <see cref="LicenseGate.Current"/>:已安裝 Licensing → 開授權視窗;未安裝 → 安裝提示。
    /// </summary>
    internal static class LicenseMenu
    {
        // priority = -100:AceLand 全產品共用的「選單約定值」。此值同時決定
        // Tools/AceLand/EventDriven 子選單(folder)在 Tools/AceLand 層級的擺位
        // (folder 取其最低 priority 子項)。所有 AceLand 產品的 License 皆設為 -100,
        // 彼此 priority 相同(相差 0 < 10),故所有 folder 併成同一組(無分隔線)。
        // folder 間的先後順序交由 Unity 預設處理(不保證嚴格字母排序);新增 package 時
        // 沿用同一個 -100 即可,無需再為分組挑選 priority。
        // 於本產品內,License(-100)與工具群組(20+)相差 > 10,故 License 仍獨立成第一子組。
        [MenuItem("Tools/AceLand/EventDriven/License", false, -100)]
        private static void OpenLicense() => LicenseGate.Current.OpenLicenseWindow();
    }
}
