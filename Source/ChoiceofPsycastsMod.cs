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
		public List<LevelRange> CanLearnPsycastCustom = null;
        //static Regex regex = new Regex("^ChoiceOfPsycasts:<([1-6]),([1-6])>$");

        static int maxValue = (int)HediffDefOf.PsychicAmplifier.maxSeverity;
        static string numberPattern = string.Join("|", Enumerable.Range(1, maxValue));
        static string pattern = $@"^ChoiceOfPsycasts:<({numberPattern}),({numberPattern})>$";
        Regex regex = new Regex(pattern);
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
					foreach (int i in CanLearnPsycast)
					{
						if (AbilityLibrary.ProperLevel(i)) yield return new LearnPsycasts(i, Parent);
					}
				}
			}
			if (CanLearnPsycastCustom != null)
			{
				foreach (LevelRange i in CanLearnPsycastCustom)
				{
					if (AbilityLibrary.ProperLevelRange(i)) yield return new LearnPsycasts(i, Parent);
				}
			}
		}
		public override void PostExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				if (!CanLearnPsycast.NullOrEmpty())
				{
					CanLearnPsycast.RemoveAll(x => !AbilityLibrary.ProperLevel(x));
					if (CanLearnPsycast.Count > 0) Scribe_Collections.Look(ref CanLearnPsycast, "CanLearnPsycast", LookMode.Value);
				}
				if (!CanLearnPsycastCustom.NullOrEmpty())
				{
					CanLearnPsycastCustom.RemoveAll(x => !AbilityLibrary.ProperLevelRange(x));
					if (CanLearnPsycastCustom.Count > 0) Scribe_Collections.Look(ref CanLearnPsycastCustom, "CanLearnPsycastCustom", LookMode.Deep);
				}
			}
			else
			{
				Scribe_Collections.Look(ref CanLearnPsycast, "CanLearnPsycast", LookMode.Value);
				Scribe_Collections.Look(ref CanLearnPsycastCustom, "CanLearnPsycastCustom", LookMode.Deep);
			}
		}
		public override void ReceiveCompSignal(string signal)
		{
			Match m = regex.Match(signal);
			if (m.Success)
			{
				int min = int.Parse(m.Groups[1].Value);
				int max = int.Parse(m.Groups[2].Value);
				if (Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycastCustom == null) Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycastCustom = new List<LevelRange>();
				if (min <= max) Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycastCustom.Add(new LevelRange(min, max));
				else Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycastCustom.Add(new LevelRange(max, min));
			}
		}
	}
	class LearnPsycasts : Command_Action
	{
		private int Level;
		private LevelRange Range;
		Pawn Parent;
		public LearnPsycasts(int level, Pawn pawn)
		{
			action = Choice;
			Level = level;
			Parent = pawn;
			defaultLabel = $"{"LearnAPsycast".Translate()}";
			defaultDesc = $"{"LearnAPsycastDesc".Translate()}";
			icon = AbilityLibrary.IconLevel[level].Texture;
		}
		public LearnPsycasts(LevelRange range, Pawn pawn)
		{
			action = ChoiceCustom;
			Range = range;
			Parent = pawn;
			defaultLabel = $"{"LearnAPsycast".Translate()}";
			defaultDesc = $"{"LearnAPsycastDesc".Translate()}";
			if (Range.low != Range.high) icon = AbilityLibrary.IconMisc.Texture;
			else icon = AbilityLibrary.IconLevel[Range.low].Texture;
		}
		private void Choice()
		{
			List<FloatMenuOption> options = new List<FloatMenuOption>();
			foreach ((AbilityDef, CachedTexture) Psycast in AbilityLibrary.Psycasts[Level])
			{
				if (!Parent.abilities.AllAbilitiesForReading.Exists(x => x.def.defName == Psycast.Item1.defName))
				{
					FloatMenuOption option = new FloatMenuOption(Psycast.Item1.label, delegate
					{
						Parent.abilities.GainAbility(Psycast.Item1);
						Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast.Remove(Level);
					}, Psycast.Item2.Texture, Color.white, MenuOptionPriority.Default, null, null, 30, Rect => Verse.Widgets.InfoCardButton(Rect.ScaledBy(0.7f), Psycast.Item1));
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
				options.Add(new FloatMenuOption("No Available Psycasts", delegate { Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycast.Remove(Level); }));
				FloatMenu menu = new FloatMenu(options);
				Find.WindowStack.Add(menu);
			}
		}
		private void ChoiceCustom()
		{
			List<FloatMenuOption> options = new List<FloatMenuOption>();
			for (int i = Range.low; i <= Range.high; i++)
			{
				foreach ((AbilityDef, CachedTexture) Psycast in AbilityLibrary.Psycasts[i])
				{
					if (!Parent.abilities.AllAbilitiesForReading.Exists(x => x.def.defName == Psycast.Item1.defName))
					{
						FloatMenuOption option = new FloatMenuOption(Psycast.Item1.label, delegate
						{
							Parent.abilities.GainAbility(Psycast.Item1);
							Parent.GetComp<ChoiceOfPsycastsComp>().CanLearnPsycastCustom.Remove(Range);
						}, Psycast.Item2.Texture, Color.white, MenuOptionPriority.Default, null, null, 30, Rect => Verse.Widgets.InfoCardButton(Rect.ScaledBy(0.7f), Psycast.Item1));
						options.Add(option);
					}
				}
			}
			if (ChoiceOfPsycastsMod.Settings.PsycastOptions > 1)
			{
				System.Random rnd = new System.Random(Range.low + Range.high + Parent.abilities.abilities.Count + Parent.GetHashCode());
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
		public static Dictionary<int, List<(AbilityDef, CachedTexture)>> Psycasts = new Dictionary<int, List<(AbilityDef, CachedTexture)>>();
		public static Dictionary<int, Ability> DummyPsycasts = new Dictionary<int, Ability>();
		public static Dictionary<int, CachedTexture> IconLevel = new Dictionary<int, CachedTexture>();
		public static CachedTexture IconMisc = new CachedTexture("Misc");

		static AbilityLibrary()
		{
			int maxLevel = ModLister.HasActiveModWithName("Cooler Psycasts") ? 10 : 6;
            foreach (var i in Enumerable.Range(1, maxLevel))
			{

				Log.Message("DummyPsycast" + i.ToString());

				Psycasts.Add(i, new List<(AbilityDef, CachedTexture)>());
				IconLevel.Add(i, new CachedTexture("Level" + i.ToString()));
				DummyPsycasts.Add(i, new Ability());
				{
					DummyPsycasts[i].def = new AbilityDef
					{
						defName = "DummyPsycast" + i.ToString(),
						level = i,
						abilityClass = typeof(Psycast)
					};
				}
			}
			List<AbilityDef> Abilities = DefDatabase<AbilityDef>.AllDefsListForReading;
			foreach (AbilityDef Ability in Abilities)
			{
				if (Ability.abilityClass == typeof(Psycast) && Ability.level > 0 && Ability.level <= (int)HediffDefOf.PsychicAmplifier.maxSeverity)
				{
					Psycasts[Ability.level].Add((Ability, new CachedTexture(Ability.iconPath)));
				}
			}
		}
		public static bool ProperLevel(int i)
		{
			if (i > 0 && i <= (int)HediffDefOf.PsychicAmplifier.maxSeverity) return true;
			else return false;
		}
		public static bool ProperLevelRange(LevelRange i)
		{
			if (i.low > 0 && i.low <= (int)HediffDefOf.PsychicAmplifier.maxSeverity && i.high >= i.low && i.high <= (int)HediffDefOf.PsychicAmplifier.maxSeverity) return true;
			else return false;
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
	public struct LevelRange : IExposable
	{
		public int low;
		public int high;

		public LevelRange(int a, int b)
		{
			low = a;
			high = b;
		}
		public void ExposeData()
		{
			Scribe_Values.Look(ref low, "low");
			Scribe_Values.Look(ref high, "high");
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