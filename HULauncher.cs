using BepInEx;

namespace HiddenUnits {

	[BepInPlugin("teamgrad.hiddenunits", "Hidden Units", "1.0.4")]
	public class HULauncher : BaseUnityPlugin {

		public HULauncher() { HUBinder.UnitGlad(); }
	}
}
