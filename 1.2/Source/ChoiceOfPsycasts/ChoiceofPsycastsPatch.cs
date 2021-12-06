using Verse;
using HarmonyLib;
using System.Reflection;

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
                    pawn.abilities.RemoveAbility(pawn.abilities.abilities[pawn.abilities.abilities.Count - 1].def);
                    if (pawn.GetPsylinkLevel() > 0 && pawn.GetPsylinkLevel() < 7)
                    {
                        pawn.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast.Add(pawn.GetPsylinkLevel());
                    }
                    else Log.Error("Tried giving incorrect level Psycast chooser ability");
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
                    user.abilities.RemoveAbility(user.abilities.abilities[user.abilities.abilities.Count - 1].def);
                    if (user.GetPsylinkLevel() > 0 && user.GetPsylinkLevel() < 7)
                    {
                        user.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast.Add(user.GetPsylinkLevel());
                    }
                    else Log.Error("Tried giving incorrect level Psycast chooser ability");
                }
            }
        }
    }
}
