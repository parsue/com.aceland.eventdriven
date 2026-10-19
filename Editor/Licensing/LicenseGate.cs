namespace AceLand.EventDriven.Editor.Licensing
{
    /// <summary>
    /// 授權閘門的全域接縫。工具程式碼一律透過 <see cref="Current"/> 取用授權;
    /// 預設為 fail-closed 的 <see cref="MissingLicenseGate"/>(視為未安裝 Licensing)。
    ///
    /// 當使用者已安裝 <c>com.aceland.licensing</c> 時,source 橋接
    /// (<c>AceLand.EventDriven.LicenseBridge</c>)會於 <c>[InitializeOnLoad]</c> 呼叫
    /// <see cref="Install"/> 注入真實 gate,之後所有 check point 便走真實授權判定。
    /// </summary>
    internal static class LicenseGate
    {
        private static ILicenseGate _current = new MissingLicenseGate();

        /// <summary>目前生效的授權閘門(永不為 null)。</summary>
        public static ILicenseGate Current => _current;

        /// <summary>
        /// 由 source 橋接於載入時注入真實 gate。傳入 null 時忽略(維持 fail-closed 預設)。
        /// </summary>
        public static void Install(ILicenseGate gate)
        {
            if (gate != null) _current = gate;
        }
    }
}
