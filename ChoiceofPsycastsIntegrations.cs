using System;
using Verse;
using System.Reflection;

namespace ChoiceOfPsycasts
{
	class UpgradablePsycastsFrameworkIntegration
	{
		public Type UPFExtension;
		public FieldInfo UPFUgradeOnlyField;
		public UpgradablePsycastsFrameworkIntegration()
		{
			UPFExtension = Type.GetType("PsycastUpgradeFramework.PsycastExtension, PsycastUpgradeFramework");
			UPFUgradeOnlyField = UPFExtension.GetField("upgradeOnly");
		}
	}
}
