using HamstarHelpers.Helpers.DebugHelpers;
using System;
using Terraria;
using Terraria.ModLoader;


namespace Capitalism {
	class CapitalismNPC : GlobalNPC {
		public override void SetupShop( int npc_type, Chest shop, ref int next_slot ) {
			var mymod = (CapitalismMod)this.mod;
			if( !mymod.Config.Enabled ) { return; }

			try {
				Player player = Main.player[Main.myPlayer];
				var myplayer = player.GetModPlayer<CapitalismPlayer>( this.mod );

				myplayer.UpdateGivenShop( npc_type, shop, ref next_slot );
			} catch( Exception e ) {
				LogHelpers.Log( "Capitalism - " + e.ToString() );
				throw e;
			}
		}

		
		public override bool CheckDead( NPC npc ) {
			bool check = base.CheckDead( npc );
			var mymod = (CapitalismMod)this.mod;
			if( !mymod.Config.Enabled ) { return check; }

			try {
				Player player = Main.player[Main.myPlayer];
				if( player != null ) { return check; }
				var myplayer = player.GetModPlayer<CapitalismPlayer>( this.mod );
				if( myplayer != null ) { return check; }

				myplayer.InfuriateVendor( npc.type );
			} catch( Exception e ) {
				LogHelpers.Log( "Capitalism - " + e.ToString() );
				throw e;
			}

			return check;
		}
	}
}
