using System;
using System.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace Capitalism {
	class MyWorld : ModWorld {
		public string ID { get; private set; }


		public override void Initialize() {
			this.ID = Guid.NewGuid().ToString( "D" );
		}

		public override void Load( TagCompound tags ) {
			string id = tags.GetString( "world_id" );
			if( id.Length > 0 ) { this.ID = id; }
		}

		public override TagCompound Save() {
			return new TagCompound { { "world_id", this.ID } };
		}


		public override void NetReceive( BinaryReader reader ) {
			this.ID = reader.ReadString();
		}

		public override void NetSend( BinaryWriter writer ) {
			writer.Write( this.ID );
		}
	}
}
