﻿using HamstarHelpers.Classes.Errors;
using System;
using Terraria;
using Terraria.ModLoader;


namespace Capitalism {
	class CapitalismNPC : GlobalNPC {
		public override void SetupShop( int npcType, Chest shop, ref int nextSlot ) {
			var mymod = (CapitalismMod)this.mod;
			if( !mymod.Config.Enabled ) { return; }

			try {
				Player player = Main.player[Main.myPlayer];
				var myplayer = player.GetModPlayer<CapitalismPlayer>();

				myplayer.UpdateGivenShop( npcType, shop, ref nextSlot );
			} catch( Exception e ) {
				throw new ModHelpersException( "", e );
			}
		}

		
		public override bool CheckDead( NPC npc ) {
			bool check = base.CheckDead( npc );
			var mymod = (CapitalismMod)this.mod;
			if( !mymod.Config.Enabled ) { return check; }

			try {
				Player player = Main.player[Main.myPlayer];
				if( player != null ) { return check; }
				var myplayer = ModContent.GetInstance<CapitalismPlayer>();
				if( myplayer != null ) { return check; }

				myplayer.InfuriateVendor( npc.type );
			} catch( Exception e ) {
				throw new ModHelpersException( "", e );
			}

			return check;
		}
	}
}
