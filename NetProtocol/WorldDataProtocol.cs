using HamstarHelpers.Components.Network;
using HamstarHelpers.Components.Network.Data;


namespace Capitalism.NetProtocol {
	class WorldDataProtocol : PacketProtocol {
		public string OldID;


		////////////////

		private WorldDataProtocol( PacketProtocolDataConstructorLock ctor_lock ) { }

		////////////////

		protected override void SetServerDefaults( int to_who ) {
			var myworld = CapitalismMod.Instance.GetModWorld<CapitalismWorld>();

			this.OldID = myworld.ID;
		}

		////////////////

		protected override void ReceiveWithClient() {
			if( !string.IsNullOrEmpty(this.OldID) ) {
				var myworld = CapitalismMod.Instance.GetModWorld<CapitalismWorld>();
				myworld._ID = this.OldID;
			}
		}
	}
}
