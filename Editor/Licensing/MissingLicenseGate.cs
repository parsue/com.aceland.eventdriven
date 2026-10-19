using System;

namespace AceLand.EventDriven.Editor.Licensing
{
    /// <summary>
    /// fail-closed 的預設閘門:代表「未安裝 <c>com.aceland.licensing</c>」。
    ///
    /// <see cref="IsEntitled"/> 恆為 false,所有進入點一律導向
    /// <see cref="LicensingInstallPrompt"/> 引導使用者安裝授權 Core。當 Licensing 缺席時,
    /// <see cref="LicenseGate.Current"/> 維持本實作,工具因此停用但不拋錯。
    /// </summary>
    internal sealed class MissingLicenseGate : ILicenseGate
    {
        public bool IsEntitled => false;

        // 未安裝時沒有狀態變更來源;空實作以滿足介面。
        public event Action Changed { add { } remove { } }

        public void EnsureChecked() { }

        public bool EnsureEntitledForMenu(string toolName)
        {
            LicensingInstallPrompt.PromptInstall(toolName);
            return false;
        }

        public void DrawBlockedPage(string toolName) =>
            LicensingInstallPrompt.DrawInstallPage(toolName);

        public void OpenLicenseWindow() =>
            LicensingInstallPrompt.PromptInstall("AceLand Event Driven — License");
    }
}
