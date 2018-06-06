using HamstarHelpers.WorldHelpers;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace Capitalism {
	class CapitalismWorld : ModWorld {
		internal string _ID = "";
		public string ID {
			get {
				if( string.IsNullOrEmpty(this._ID) ) {
					return WorldHelpers.GetUniqueIdWithSeed();
				} else {
					return this._ID;
				}
			}
		}


		////////////////

		public override void Load( TagCompound tags ) {
			if( tags.ContainsKey("world_id") ) {
				this._ID = tags.GetString( "world_id" );
			}
		}

		public override TagCompound Save() {
			var tags = new TagCompound();

			if( !string.IsNullOrEmpty(this._ID) ) {
				tags[ "world_id" ] = this._ID;
			}

			return tags;
		}
	}
}
