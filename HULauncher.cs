using System.Collections;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace HiddenUnits {

	[BepInPlugin("teamgrad.hiddenunits", "Hidden Units", "1.1.0")]
	public class HULauncher : BaseUnityPlugin 
	{
		public void Awake()
		{
			DoConfig();
			StartCoroutine(LaunchMod());
		}
		
		private static IEnumerator LaunchMod() 
		{
			yield return new WaitUntil(() => FindObjectOfType<ServiceLocator>() != null);
			
			yield return new WaitForSeconds(0.5f);
			
			new HUMain();
		}

		private void DoConfig()
		{
			ConfigInfiniteScalingEnabled = Config.Bind("Bug", "InfiniteScalingEnabled", true, "Enables/disables Mathematician/Philosopher projectiles infinitely scaling unit parts.");
		}
		
		public static ConfigEntry<bool> ConfigInfiniteScalingEnabled;
	}
}
