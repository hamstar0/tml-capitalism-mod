using Terraria.ModLoader;
using Terraria;
using Terraria.ModLoader.IO;
using System;
using Capitalism.Logic;


namespace Capitalism {
	class CapitalismPlayer : ModPlayer {
		private CapitalismLogic Logic;


		////////////////

		public override void Initialize() {
			if( this.Logic == null ) {
				this.Logic = new CapitalismLogic();
			}
		}

		public override void clientClone( ModPlayer clone ) {
			base.clientClone( clone );
			var myclone = (CapitalismPlayer)clone;
			myclone.Logic = this.Logic;
		}

		public override void OnEnterWorld( Player player ) {
			if( Main.netMode != 2 ) {   // Not server
				if( player.whoAmI == this.player.whoAmI ) { // Current player
					var mymod = (CapitalismMod)this.mod;
					if( !mymod.Config.LoadFile() ) {
						mymod.Config.SaveFile();
					}
					
					if( Main.netMode == 1 ) {
						CapitalismNetProtocol.SendSettingsRequestFromClient( mymod, player );
					}
				}
			}
		}

		////////////////

		public override void Load( TagCompound tags ) {
			try {
				var mymod = (CapitalismMod)this.mod;
				int total_worlds = tags.GetInt( "vendor_world_count" );

				for( int i=0; i< total_worlds; i++ ) {
					string id = tags.GetString( "vendor_world_id_" + i );
					this.Logic.LoadVendorsForCurrentPlayer( mymod, tags, id );
				}
			} catch( Exception e ) {
				ErrorLogger.Log( e.ToString() );
				throw e;
			}
		}

		public override TagCompound Save() {
			int world_count = this.Logic.VendorWorlds.Count;
			var tags = new TagCompound { { "vendor_world_count", world_count } };
				
			int i = 0;
			foreach( string id in this.Logic.VendorWorlds.Keys ) {
				tags.Set( "vendor_world_id_"+i, id );
				i++;
			}

			this.Logic.SaveVendorsForCurrentPlayer( tags );
			return tags;
		}
		
		////////////////

		public override void PreUpdate() {
			this.Logic.Update( (CapitalismMod)this.mod, this.player );
		}


		////////////////

		public void UpdateGivenShop( int npc_type, Chest shop, ref int nextSlot ) {
			if( this.Logic != null ) {
				this.Logic.UpdateGivenShop( (CapitalismMod)this.mod, this.player, npc_type, shop, ref nextSlot );
			}
		}

		public bool InfuriateVendor( int npc_type ) {
			if( this.Logic != null ) {
				return this.Logic.InfuriateVendor( (CapitalismMod)this.mod, npc_type );
			}
			return false;
		}
	}
}
