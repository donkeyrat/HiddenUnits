using BepInEx;

namespace HiddenUnits {

	[BepInPlugin("teamgrad.hiddenunits", "Hidden Units", "1.1.0")]
	public class HULauncher : BaseUnityPlugin 
	{
		public HULauncher() { HUBinder.UnitGlad(); }
	}
}
