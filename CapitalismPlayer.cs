using Terraria.ModLoader;
using Terraria;
using Terraria.ModLoader.IO;
using Capitalism.Logic;
using Capitalism.NetProtocol;
using HamstarHelpers.Services.Promises;
using HamstarHelpers.Components.Network;


namespace Capitalism {
	class CapitalismPlayer : ModPlayer {
		private CapitalismLogic Logic;


		////////////////

		public override bool CloneNewInstances { get { return false; } }

		public override void Initialize() {
			this.Logic = new CapitalismLogic();
		}

		public override void clientClone( ModPlayer clone ) {
			base.clientClone( clone );
			var myclone = (CapitalismPlayer)clone;
			myclone.Logic = this.Logic;
		}


		////////////////

		public override void SyncPlayer( int to_who, int from_who, bool new_player ) {
			var mymod = (CapitalismMod)this.mod;

			if( Main.netMode == 2 ) {
				if( to_who == -1 && from_who == this.player.whoAmI ) {
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
					ErrorLogger.Log( "Capitalism config " + CapitalismConfigData.ConfigVersion.ToString() + " created (ModPlayer.OnEnterWorld())." );
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
				PacketProtocol.QuickRequestToServer<ModSettingsProtocol>();
				PacketProtocol.QuickRequestToServer<WorldDataProtocol>();
			} );
		}

		private void OnServerConnect() {
		}


		////////////////

		public override void Load( TagCompound tags ) {
			if( !tags.ContainsKey( "vendor_world_count" ) ) { return; }

			var mymod = (CapitalismMod)this.mod;
			int total_worlds = tags.GetInt( "vendor_world_count" );

			for( int i=0; i< total_worlds; i++ ) {
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

			this.Logic.Update( (CapitalismMod)this.mod, this.player );
		}


		////////////////

		public void UpdateGivenShop( int npc_type, Chest shop, ref int next_slot ) {
			if( this.Logic != null ) {
				this.Logic.UpdateGivenShop( (CapitalismMod)this.mod, this.player, npc_type, shop, ref next_slot );
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
