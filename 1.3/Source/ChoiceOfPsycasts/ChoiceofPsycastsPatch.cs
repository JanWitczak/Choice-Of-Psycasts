using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace RimWorld
{
	namespace ChoiceOfPsycasts
	{
		public class HarmonyPatches : Verse.Mod
		{
			public HarmonyPatches(ModContentPack content) : base(content)
			{
				var harmony = new Harmony("Azuraal.ChoiceOfPsycasts");
				var assembly = Assembly.GetExecutingAssembly();
				harmony.PatchAll(assembly);
			}

		}

		[HarmonyPatch(typeof(RimWorld.PawnUtility), "ChangePsylinkLevel")]
		class ChangePsylinkLevelPatch
		{
			static void Postfix(ref Pawn pawn)
			{
				if (pawn.IsColonist)
				{
					if (pawn.GetComp<ChoiceOfPsycastsComp>() != null)
					{
						pawn.abilities.RemoveAbility(pawn.abilities.abilities[pawn.abilities.abilities.Count - 1].def);
						if (pawn.GetPsylinkLevel() > 0 && pawn.GetPsylinkLevel() < 7)
						{
							if (pawn.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast == null) pawn.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast = new List<int>();
							pawn.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast.Add(pawn.GetPsylinkLevel());
						}
						else Log.Error("Tried giving incorrect level Psycast");
					}
					else Log.Error("Pawn doesn't inherif after BasePawn and so doesn't have ChoiceOfPsycastsComp.");
				}
			}
		}

		[HarmonyPatch(typeof(CompUseEffect_InstallImplant), "DoEffect")]
		class NeuroformerPatch
		{
			static void Postfix(ref Pawn user, CompUseEffect_InstallImplant __instance)
			{
				if (__instance.Props.hediffDef == DefDatabase<HediffDef>.GetNamed("PsychicAmplifier") && user.IsColonist)
				{
					if (user.GetComp<ChoiceOfPsycastsComp>() != null)
					{
						user.abilities.RemoveAbility(user.abilities.abilities[user.abilities.abilities.Count - 1].def);
						if (user.GetPsylinkLevel() > 0 && user.GetPsylinkLevel() < 7)
						{
							if (user.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast == null) user.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast = new List<int>();
							user.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast.Add(user.GetPsylinkLevel());
						}
						else Log.Error("Tried giving incorrect level Psycast");
					}
					else Log.Error("Pawn doesn't inherif after BasePawn and so doesn't have ChoiceOfPsycastsComp.");
				}
			}
		}
	}
}
