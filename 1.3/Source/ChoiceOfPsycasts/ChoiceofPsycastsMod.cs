using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace RimWorld
{
	namespace ChoiceOfPsycasts
	{

		public class ChoiceOfPsycastsProperties : CompProperties
		{
			public ChoiceOfPsycastsProperties()
			{
				compClass = typeof(ChoiceOfPsycastsComp);
			}
		}

		public class ChoiceOfPsycastsComp : ThingComp
		{
			public List<int> CanLearnPsycast;
			public Pawn Parent
			{
				get { return (Pawn)this.parent; }
			}

			public override IEnumerable<Gizmo> CompGetGizmosExtra()
			{
				if (ChoiceOfPsycastsMod.Settings.PsycastOptions > 0 && CanLearnPsycast != null)
				{
					foreach (var i in CanLearnPsycast)
					{
						yield return new LearnPsycasts(i, Parent);
					}
				}
			}

			public override void PostExposeData()
			{
				Scribe_Collections.Look(ref CanLearnPsycast, false, "CanLearnPsycast", LookMode.Value);
			}

			public override void Initialize(CompProperties props)
			{
				base.Initialize(props);
				CanLearnPsycast = new List<int>();
			}

			public override void ReceiveCompSignal(string signal)
			{
				Regex reg = new Regex("^ChoiceOfPsycasts:(([1-6])|<([1-6]),([1-6])>)$");
				Match m = reg.Match(signal);
				if (m.Success)
				{
					if (m.Groups[1].Value != "") Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast.Add(int.Parse(m.Groups[1].Value));
				}
				base.ReceiveCompSignal(signal);
			}
		}

		class LearnPsycasts : Command_Action
		{
			private int Level;
			Pawn Parent;
			public LearnPsycasts(int level, Pawn pawn)
			{
				action = Choice;
				Level = level;
				Parent = pawn;
				defaultLabel = $"{"LearnAPsycast".Translate()}";
				defaultDesc = $"{"LearnAPsycastDesc".Translate()}" + Level.ToString();
				icon = ContentFinder<Texture2D>.Get("Level" + level.ToString());
			}

			public void Choice()
			{
				List<FloatMenuOption> options = new List<FloatMenuOption>();
				foreach (AbilityDef Psycast in AbilityLibrary.Psycasts[Level])
				{
					if (!Parent.abilities.AllAbilitiesForReading.Exists(x => x.def.defName == Psycast.defName))
					{
						FloatMenuOption option = new FloatMenuOption(Psycast.label, delegate
						{
							Parent.abilities.GainAbility(Psycast);
							Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast.Remove(Level);
						}, ContentFinder<Texture2D>.Get(Psycast.iconPath), Color.white, MenuOptionPriority.Default, null, null, 30, Rect => Verse.Widgets.InfoCardButton(Rect.ScaledBy(0.7f), Psycast));
						options.Add(option);
					}
				}
				if (ChoiceOfPsycastsMod.Settings.PsycastOptions > 1)
				{
					System.Random rnd = new System.Random(Level + Parent.GetHashCode());
					foreach (var i in Enumerable.Range(0, Math.Max(options.Count() - ChoiceOfPsycastsMod.Settings.PsycastOptions, 0)))
					{
						options.RemoveAt(rnd.Next(options.Count()));
					}
				}
				if (options.Count > 0)
				{
					FloatMenu menu = new FloatMenu(options);
					Find.WindowStack.Add(menu);
				}
				else
				{
					Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast.Remove(Level);
					Log.Error("ChoiceOfPsycasts: No psycasts of level " + Level + " availibile to learn for pawn " + Parent.Name + ".");
				}
			}
		}

		[StaticConstructorOnStartup]
		public static class AbilityLibrary
		{
			public static Dictionary<int, List<AbilityDef>> Psycasts = new Dictionary<int, List<AbilityDef>>();
			public static Dictionary<int, Ability> DummyPsycasts = new Dictionary<int, Ability>();
			static AbilityLibrary()
			{
				foreach (var i in Enumerable.Range(1, 6))
				{
					Psycasts.Add(i, new List<AbilityDef>());
					DummyPsycasts.Add(i, new Ability());
					{
						DummyPsycasts[i].def = new AbilityDef();
						DummyPsycasts[i].def.defName = "DummyPsycast" + i.ToString();
						DummyPsycasts[i].def.level = i;
					}
				}
				List<AbilityDef> Abilities = DefDatabase<AbilityDef>.AllDefsListForReading;
				foreach (AbilityDef Ability in Abilities)
				{
					if (Ability.abilityClass == typeof(Psycast) && Ability.level > 0 && Ability.level < 7)
					{
						Psycasts[Ability.level].Add(Ability);
					}
				}
			}
		}

		public class ChoiceOfPsycastsSettings : ModSettings
		{
			public int PsycastOptions = 1;
			public override void ExposeData()
			{
				Scribe_Values.Look(ref PsycastOptions, "PsycastOptions", defaultValue: 1); ;
				base.ExposeData();
			}
		}

		[StaticConstructorOnStartup]
		public class ChoiceOfPsycastsMod : Verse.Mod
		{
			public static ChoiceOfPsycastsSettings Settings;
			public ChoiceOfPsycastsMod(ModContentPack content) : base(content)
			{
				Settings = GetSettings<ChoiceOfPsycastsSettings>();
			}

			public override void DoSettingsWindowContents(Rect inRect)
			{
				Listing_Standard listingStandard = new Listing_Standard();
				listingStandard.Begin(inRect);
				if (Settings.PsycastOptions == 1)
					listingStandard.Label("Psycast Options: All");
				else if (Settings.PsycastOptions == 0)
					listingStandard.Label("Psycast Options: No free Psycasts");
				else listingStandard.Label("Psycast Options: " + Settings.PsycastOptions.ToString());
				Settings.PsycastOptions = (int)Math.Round(listingStandard.Slider(Settings.PsycastOptions, 0, 5), 0);
				listingStandard.End();
				base.DoSettingsWindowContents(inRect);
			}
			public override string SettingsCategory()
			{
				return "Choice Of Psycasts";
			}
		}
	}
}
