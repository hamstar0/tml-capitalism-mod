using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;


namespace Capitalism {
	class CapitalismItemInfo : GlobalItem {
		public override bool InstancePerEntity { get { return true; } }
		//public override bool CloneNewInstances { get { return true; } }


		public double MarkupPercentPlus;


		public override GlobalItem Clone( Item item, Item item_clone ) {
			var clone = (CapitalismItemInfo)base.Clone( item, item_clone );
			clone.MarkupPercentPlus = this.MarkupPercentPlus;
			return clone;
		}


		public override void ModifyTooltips( Item item, List<TooltipLine> tooltips ) {
			if( this.MarkupPercentPlus >= 0.001d ) {
				double amt = this.MarkupPercentPlus * 100d;
				string tip = "Has +" + amt.ToString("N0") + "% markup";
				tooltips.Add( new TooltipLine( this.mod, "CapitalismMarkup", tip ) );
			}
		}
	}
}
