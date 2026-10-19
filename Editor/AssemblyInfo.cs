using System.Runtime.CompilerServices;

// 授權接縫(ILicenseGate / LicenseGate / MissingLicenseGate)刻意維持 internal,
// 不對外擴大 public 介面。source 橋接(AceLand.EventDriven.LicenseBridge)需於
// [InitializeOnLoad] 注入 RealLicenseGate,故開放其存取本組件 internal 成員。
[assembly: InternalsVisibleTo("AceLand.EventDriven.LicenseBridge")]
