using System;
using System.Collections.Generic;
using System.Linq;
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
				if (CanLearnPsycast != null)
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
					FloatMenuOption option = new FloatMenuOption(Psycast.label, delegate
					{
						Parent.abilities.GainAbility(Psycast);
						Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast.Remove(Level);
					}, ContentFinder<Texture2D>.Get(Psycast.iconPath), Color.white, MenuOptionPriority.Default, null, null, 30, Rect => Verse.Widgets.InfoCardButton(Rect.ScaledBy(0.7f), Psycast));
					options.Add(option);
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
			}
		}

		[StaticConstructorOnStartup]
		public static class AbilityLibrary
		{
			public static Dictionary<int, List<AbilityDef>> Psycasts = new Dictionary<int, List<AbilityDef>>();
			static AbilityLibrary()
			{
				foreach (var i in Enumerable.Range(1, 6))
				{
					Psycasts.Add(i, new List<AbilityDef>());
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
				else listingStandard.Label("Psycast Options: " + Settings.PsycastOptions.ToString());
				Settings.PsycastOptions = (int)Math.Round(listingStandard.Slider(Settings.PsycastOptions, 1, 5), 0);
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
