using HamstarHelpers.Classes.UI.ModConfig;
using HamstarHelpers.Helpers.User;
using System;
using System.ComponentModel;
using Terraria;
using Terraria.ModLoader.Config;


namespace Capitalism {
	public class CapitalismConfig : ModConfig {
		public override ConfigScope Mode => ConfigScope.ServerSide;



		////////////////

		[DefaultValue( true )]
		public bool Enabled { get; set; } = true;


		[Header( "Formula for computing markup:\n"+
			"  b=base value\n  t=total purchases\n  a=exponent factor (default 0.8)\n  c=divsor factor (default 50)\n \n"+
			"b + ( (b *t)^0.8 ) / 50" )]
		[Tooltip( "A factor for computing markup." )]
		[Range( -20f, 20f )]
		[DefaultValue( 0.8f )]
		[CustomModConfigItem( typeof(ConfigFloatInput) )]
		public float MarkupExponent { get; set; } = 0.8f;

		[Tooltip( "A factor for computing markup." )]
		[Range( Single.Epsilon, 1000f )]
		[DefaultValue( 50f )]
		[CustomModConfigItem( typeof(ConfigFloatInput) )]
		public float MarkupDivisor { get; set; } = 50f;

		[Tooltip("% markup if a Tax Collector NPC has moved in.")]
		[Range( 0f, 10f )]
		[DefaultValue( 1.02f )]
		[CustomModConfigItem( typeof(ConfigFloatInput) )]
		public float TaxMarkupPercent { get; set; } = 1.02f;

		[Tooltip("% markup after an NPC has died.")]
		[Range( 0f, 10f )]
		[DefaultValue( 1.5f )]
		[CustomModConfigItem( typeof(ConfigFloatInput) )]
		public float InfuriationMarkupPercent { get; set; } = 1.5f;


		[Tooltip("% that markup prices 'decay' to, twice per day.")]
		[Range( 0f, 10f )]
		[DefaultValue( 0.95f )]
		[CustomModConfigItem( typeof(ConfigFloatInput) )]
		public float BiDailyDecayMarkdownPercent { get; set; } = 0.95f;


		[Tooltip("% scale during a blood moon from female NPCs.")]
		[Range( 0f, 10f )]
		[DefaultValue( 1.1f )]
		[CustomModConfigItem( typeof(ConfigFloatInput) )]
		public float FemaleBloodMoonSellPricePercent { get; set; } = 1.1f;

		[Tooltip( "% scale from a 'lovestruck' NPC." )]
		[Range( 0f, 10f )]
		[DefaultValue( 0.9f )]
		[CustomModConfigItem( typeof(ConfigFloatInput) )]
		public float LovestruckSellPricePercent { get; set; } = 0.9f;

		[Tooltip( "% scale from a 'stinky' NPC." )]
		[Range( 0f, 10f )]
		[DefaultValue( 1.1f )]
		[CustomModConfigItem( typeof(ConfigFloatInput) )]
		public float StinkySellPricePercent { get; set; } = 1.1f;



		////////////////

		public override bool AcceptClientChanges( ModConfig pendingConfig, int whoAmI, ref string message ) {
			if( !UserHelpers.HasBasicServerPrivilege(Main.player[whoAmI]) ) {
				message = "Insufficient privilege.";
				return false;
			}
			return true;
		}
	}
}
