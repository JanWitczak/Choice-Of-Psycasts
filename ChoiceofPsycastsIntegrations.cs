using System;
using Verse;
using System.Reflection;

namespace ChoiceOfPsycasts
{
	[StaticConstructorOnStartup]
	public class UpgradablePsycastsFrameworkIntegration
	{
		static public Type PUPExtension;
		static public FieldInfo PUPField;
		static UpgradablePsycastsFrameworkIntegration()
		{
			PUPExtension = Type.GetType("PsycastUpgradeFramework.PsycastExtension, PsycastUpgradeFramework", false);
			if (PUPExtension != null) PUPField = PUPExtension.GetField("UpgradeOnly");
		}
	}
}
