using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using RimWorld;

namespace ChoiceOfPsycasts
{
	[StaticConstructorOnStartup]
	static class HarmonyPatches
	{
		static HarmonyPatches()
		{
			Harmony harmony = new Harmony("Azuraal.ChoiceOfPsycasts");
			Assembly assembly = Assembly.GetExecutingAssembly();
			harmony.PatchAll(assembly);
		}
	}

	[HarmonyPatch(typeof(RimWorld.PawnUtility), "ChangePsylinkLevel")]
	class ChangePsylinkLevelPatch
	{
		static void Prefix(ref Pawn pawn, int levelOffset)
		{
			if (levelOffset == 1 && pawn.IsColonist) PatchingMethods.AddDummyPsycasts(ref pawn);
		}
		static void Postfix(ref Pawn pawn, int levelOffset)
		{
			if (levelOffset == 1 && pawn.IsColonist) PatchingMethods.AddSelectionFlag(ref pawn);
		}
	}

	[HarmonyPatch(typeof(CompUseEffect_InstallImplant), "DoEffect")]
	class NeuroformerPatch
	{
		static void Prefix(ref Pawn user, CompUseEffect_InstallImplant __instance)
		{
			if (__instance.Props.hediffDef == DefDatabase<HediffDef>.GetNamed("PsychicAmplifier") && user.IsColonist) PatchingMethods.AddDummyPsycasts(ref user);
		}
		static void Postfix(ref Pawn user, CompUseEffect_InstallImplant __instance)
		{
			if (__instance.Props.hediffDef == DefDatabase<HediffDef>.GetNamed("PsychicAmplifier") && user.IsColonist) PatchingMethods.AddSelectionFlag(ref user);
		}
	}
	class PatchingMethods
	{
		public static void AddSelectionFlag(ref Pawn pawn)
		{
			if (pawn.GetComp<ChoiceOfPsycastsComp>() != null)
			{
				pawn.abilities.abilities.Remove(AbilityLibrary.DummyPsycasts[pawn.GetPsylinkLevel()]);
				if (pawn.GetPsylinkLevel() > 0 && pawn.GetPsylinkLevel() < 7)
				{
					if (pawn.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast == null) pawn.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast = new List<int>();
					pawn.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast.Add(pawn.GetPsylinkLevel());
				}
				else Log.Error("ChoiceOfPsycasts: Tried giving incorrect level Psycast");
			}
			else Log.Error("ChoiceOfPsycasts: Pawn doesn't have ChoiceOfPsycastsComp.");
		}
		public static void AddDummyPsycasts(ref Pawn pawn)
		{
			if (pawn.GetComp<ChoiceOfPsycastsComp>() != null)
			{
				if (pawn.GetPsylinkLevel() < 6) pawn.abilities.abilities.Add(AbilityLibrary.DummyPsycasts[pawn.GetPsylinkLevel() + 1]);
			}
		}
	}
}