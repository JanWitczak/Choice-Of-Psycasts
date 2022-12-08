using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using RimWorld;

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
		public List<int> CanLearnPsycast = null;
		public List<Tuple<int, int>> CanLearnPsycastCustom = null;
		public Pawn Parent
		{
			get { return (Pawn)this.parent; }
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (ChoiceOfPsycastsMod.Settings.PsycastOptions > 0)
			{
				if (CanLearnPsycast != null)
				{
					foreach (var i in CanLearnPsycast)
					{
						yield return new LearnPsycasts(i, Parent);
					}
				}
			}
			if (CanLearnPsycastCustom != null)
			{
				foreach (var i in CanLearnPsycastCustom)
				{
					yield return new LearnPsycasts(i, Parent);
				}
			}
		}
		public override void PostExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (!CanLearnPsycast.NullOrEmpty()) Scribe_Collections.Look(ref CanLearnPsycast, false, "CanLearnPsycast", LookMode.Value);
				if (!CanLearnPsycastCustom.NullOrEmpty()) Scribe_Collections.Look(ref CanLearnPsycastCustom, false, "CanLearnPsycastCustom", LookMode.Value);
			}
			else
			{
				Scribe_Collections.Look(ref CanLearnPsycast, false, "CanLearnPsycast", LookMode.Value);
				Scribe_Collections.Look(ref CanLearnPsycastCustom, false, "CanLearnPsycastCustom", LookMode.Value);
			}
		}
		public override void ReceiveCompSignal(string signal)
		{
			Regex reg = new Regex("^ChoiceOfPsycasts:<([1-6]),([1-6])>$");
			Match m = reg.Match(signal);
			if (m.Success)
			{
				int min = int.Parse(m.Groups[1].Value);
				int max = int.Parse(m.Groups[2].Value);
				if (Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycastCustom == null) Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycastCustom = new List<Tuple<int, int>>();
				if (min < max) Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycastCustom.Add(new Tuple<int, int>(min, max));
				else Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycastCustom.Add(new Tuple<int, int>(max, min));
			}
		}
	}
	class LearnPsycasts : Command_Action
	{
		private int Level;
		private Tuple<int, int> Range = null;
		Pawn Parent;
		public LearnPsycasts(int level, Pawn pawn)
		{
			action = Choice;
			Level = level;
			Parent = pawn;
			defaultLabel = $"{"LearnAPsycast".Translate()}";
			defaultDesc = $"{"LearnAPsycastDesc".Translate()}";
			icon = ContentFinder<Texture2D>.Get("Level" + level.ToString());
		}
		public LearnPsycasts(Tuple<int, int> range, Pawn pawn)
		{
			action = ChoiceCustom;
			Range = range;
			Parent = pawn;
			defaultLabel = $"{"LearnAPsycast".Translate()}";
			defaultDesc = $"{"LearnAPsycastDesc".Translate()}";
			if (Range.Item1 != Range.Item2) icon = ContentFinder<Texture2D>.Get("Misc");
			else icon = ContentFinder<Texture2D>.Get("Level" + Range.Item1.ToString());
		}
		private void Choice()
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
				options.Add(new FloatMenuOption("No Availible Psycasts", delegate { Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast.Remove(Level); }));
				FloatMenu menu = new FloatMenu(options);
				Find.WindowStack.Add(menu);
			}
		}
		private void ChoiceCustom()
		{
			List<FloatMenuOption> options = new List<FloatMenuOption>();
			foreach (var i in Enumerable.Range(Range.Item1, Range.Item2 - Range.Item1 + 1))
			{
				foreach (AbilityDef Psycast in AbilityLibrary.Psycasts[i])
				{
					if (!Parent.abilities.AllAbilitiesForReading.Exists(x => x.def.defName == Psycast.defName))
					{
						FloatMenuOption option = new FloatMenuOption(Psycast.label, delegate
						{
							Parent.abilities.GainAbility(Psycast);
							Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycastCustom.Remove(Range);
						}, ContentFinder<Texture2D>.Get(Psycast.iconPath), Color.white, MenuOptionPriority.Default, null, null, 30, Rect => Verse.Widgets.InfoCardButton(Rect.ScaledBy(0.7f), Psycast));
						options.Add(option);
					}
				}
			}
			if (ChoiceOfPsycastsMod.Settings.PsycastOptions > 1)
			{
				System.Random rnd = new System.Random(Range.Item1 + Range.Item2 + Parent.abilities.abilities.Count + Parent.GetHashCode());
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
				options.Add(new FloatMenuOption("No Availible Psycasts", delegate { Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycastCustom.Remove(Range); }));
				FloatMenu menu = new FloatMenu(options);
				Find.WindowStack.Add(menu);
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
	public class ChoiceOfPsycastsMod : Verse.Mod
	{
		public static RimWorld.ChoiceOfPsycasts.ChoiceOfPsycastsSettings Settings;
		public ChoiceOfPsycastsMod(ModContentPack content) : base(content)
		{
			Settings = GetSettings<RimWorld.ChoiceOfPsycasts.ChoiceOfPsycastsSettings>();
			if (Settings.PsycastOptions == 0)
			{ 
				Settings.PsycastOptions = 1;
				Settings.PsycastPicks = 0;
			}
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard settingsMenu = new Listing_Standard();
			settingsMenu.Begin(inRect);
			if (Settings.PsycastOptions == 1) settingsMenu.Label("Psycast Options: All");
			else settingsMenu.Label("Psycast Options: " + Settings.PsycastOptions.ToString());
			Settings.PsycastOptions = (int)settingsMenu.Slider(Settings.PsycastOptions, 1, 5);

			settingsMenu.Label("Psycast Picks Of New Level: " + Settings.PsycastPicks.ToString());
			Settings.PsycastPicks = (int)settingsMenu.Slider(Settings.PsycastPicks, 0, 3);
			
			settingsMenu.Label("Psycast Picks Of Previous Level: " + Settings.PsycastPicksPrev.ToString());
			Settings.PsycastPicksPrev = (int)settingsMenu.Slider(Settings.PsycastPicksPrev, 0, 3);
			settingsMenu.End();
			base.DoSettingsWindowContents(inRect);
		}
		public override string SettingsCategory()
		{
			return "Choice Of Psycasts";
		}
	}
}

namespace RimWorld
{
	namespace ChoiceOfPsycasts
	{
		public class ChoiceOfPsycastsSettings : ModSettings
		{
			public int PsycastOptions = 1;
			public int PsycastPicks = 1;
			public int PsycastPicksPrev = 0;
			public override void ExposeData()
			{
				Scribe_Values.Look(ref PsycastOptions, "PsycastOptions", defaultValue: 1);
				Scribe_Values.Look(ref PsycastPicks, "PsycastPicks", defaultValue: 1);
				Scribe_Values.Look(ref PsycastPicksPrev, "PsycastPicksPrev", defaultValue: 0);
				base.ExposeData();
			}
		}
	}
}