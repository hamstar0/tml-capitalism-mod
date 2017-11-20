using System;
using Terraria;
using Terraria.ModLoader;


namespace Capitalism {
	class CapitalismNPC : GlobalNPC {
		public override void SetupShop( int npc_type, Chest shop, ref int next_slot ) {
			var mymod = (CapitalismMod)this.mod;
			if( !mymod.Config.Data.Enabled ) { return; }

			try {
				Player player = Main.player[Main.myPlayer];
				var modplayer = player.GetModPlayer<CapitalismPlayer>( this.mod );

				modplayer.UpdateGivenShop( npc_type, shop, ref next_slot );
			} catch( Exception e ) {
				ErrorLogger.Log( e.ToString() );
				throw e;
			}
		}

		
		public override bool CheckDead( NPC npc ) {
			bool check = base.CheckDead( npc );
			var mymod = (CapitalismMod)this.mod;
			if( !mymod.Config.Data.Enabled ) { return check; }

			try {
				Player player = Main.player[Main.myPlayer];
				if( player != null ) { return check; }
				var modplayer = player.GetModPlayer<CapitalismPlayer>( this.mod );
				if( modplayer != null ) { return check; }

				modplayer.InfuriateVendor( npc.type );
			} catch( Exception e ) {
				ErrorLogger.Log( e.ToString() );
				throw e;
			}

			return check;
		}
	}
}
