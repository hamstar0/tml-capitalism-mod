using Terraria.ModLoader;
using Terraria;
using Terraria.ModLoader.IO;
using Capitalism.Logic;
using Capitalism.NetProtocol;
using HamstarHelpers.Services.Promises;
using HamstarHelpers.Components.Network;
using HamstarHelpers.Helpers.DebugHelpers;


namespace Capitalism {
	class CapitalismPlayer : ModPlayer {
		private CapitalismLogic Logic;


		////////////////

		public override bool CloneNewInstances => false;

		public override void Initialize() {
			this.Logic = new CapitalismLogic();
		}

		public override void clientClone( ModPlayer clone ) {
			base.clientClone( clone );
			var myclone = (CapitalismPlayer)clone;
			myclone.Logic = this.Logic;
		}


		////////////////

		public override void SyncPlayer( int toWho, int fromWho, bool newPlayer ) {
			var mymod = (CapitalismMod)this.mod;

			if( Main.netMode == 2 ) {
				if( toWho == -1 && fromWho == this.player.whoAmI ) {
					this.OnServerConnect();
				}
			}
		}

		public override void OnEnterWorld( Player player ) {
			if( player.whoAmI != Main.myPlayer ) { return; }
			if( this.player.whoAmI != Main.myPlayer ) { return; }

			var mymod = (CapitalismMod)this.mod;

			if( Main.netMode == 0 ) {
				if( !mymod.ConfigJson.LoadFile() ) {
					mymod.ConfigJson.SaveFile();
					LogHelpers.Alert( "Capitalism config " + mymod.Version.ToString() + " created." );
				}
			}

			if( Main.netMode == 0 ) {
				this.OnSingleConnect();
			}
			if( Main.netMode == 1 ) {
				this.OnClientConnect();
			}
		}

		////////////////

		private void OnSingleConnect() {
		}

		private void OnClientConnect() {
			Promises.AddWorldLoadOncePromise( () => {
				PacketProtocolRequestToServer.QuickRequest<ModSettingsProtocol>( -1 );
				PacketProtocolRequestToServer.QuickRequest<WorldDataProtocol>( -1 );
			} );
		}

		private void OnServerConnect() {
		}


		////////////////

		public override void Load( TagCompound tags ) {
			if( !tags.ContainsKey( "vendor_world_count" ) ) { return; }

			var mymod = (CapitalismMod)this.mod;
			int totalWorlds = tags.GetInt( "vendor_world_count" );

			for( int i=0; i< totalWorlds; i++ ) {
				string id = tags.GetString( "vendor_world_id_" + i );
				this.Logic.LoadVendorsForCurrentPlayer( mymod, tags, id );
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
			var mymod = (CapitalismMod)this.mod;
			if( !mymod.Config.Enabled ) { return; }

			this.Logic.Update( this.player );
		}


		////////////////

		public void UpdateGivenShop( int npcType, Chest shop, ref int nextSlot ) {
			if( this.Logic != null ) {
				this.Logic.UpdateGivenShop( this.player, npcType, shop, ref nextSlot );
			}
		}

		public bool InfuriateVendor( int npcType ) {
			if( this.Logic != null ) {
				return this.Logic.InfuriateVendor( npcType );
			}
			return false;
		}
	}
}
