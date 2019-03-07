using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;


namespace Capitalism {
	class CapitalismItemInfo : GlobalItem {
		public double MarkupPercentPlus;


		////////////////

		public override bool InstancePerEntity => true;
		//public override bool CloneNewInstances { get { return true; } }


		////////////////

		public override GlobalItem Clone( Item item, Item itemClone ) {
			var clone = (CapitalismItemInfo)base.Clone( item, itemClone );
			clone.MarkupPercentPlus = this.MarkupPercentPlus;
			return clone;
		}

		////////////////

		public override void ModifyTooltips( Item item, List<TooltipLine> tooltips ) {
			if( this.MarkupPercentPlus >= 0.001d ) {
				double amt = this.MarkupPercentPlus * 100d;
				string tip = "Has +" + amt.ToString("N0") + "% markup";
				tooltips.Add( new TooltipLine( this.mod, "CapitalismMarkup", tip ) );
			}
		}
	}
}
