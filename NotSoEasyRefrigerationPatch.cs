using System.Reflection;
using HarmonyLib;
using Verse;

namespace NotSoEasyRefrigeration
{
	public class HarmonyPatches : Verse.Mod
	{
		public HarmonyPatches(ModContentPack content) : base(content)
		{
			var harmony = new Harmony("Azuraal.NotSoEasyRefrigeration");
			var assembly = Assembly.GetExecutingAssembly();
			harmony.PatchAll(assembly);
		}
	}

	[HarmonyPatch(typeof(Verse.GenTemperature), "RotRateAtTemperature")]
	class RotRatePatch
	{
		static void Postfix(float temperature, ref float __result)
		{
			if (temperature >= 10f)
			{
				__result = 1f;
				return;
			}
			else if (temperature <= -20f)
			{
				__result = 0.01f;
				return;
			}
			else
			{
				__result = (float)System.Math.Pow(1.16f, temperature) / 4.411435f;
				return;
			}
		}
	}
}
