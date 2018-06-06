using HamstarHelpers.Utilities.Network;


namespace Capitalism.NetProtocol {
	class WorldDataProtocol : PacketProtocol {
		public string OldID;


		public override void SetServerDefaults() {
			var myworld = CapitalismMod.Instance.GetModWorld<CapitalismWorld>();

			this.OldID = myworld.ID;
		}

		protected override void ReceiveWithClient() {
			if( !string.IsNullOrEmpty(this.OldID) ) {
				var myworld = CapitalismMod.Instance.GetModWorld<CapitalismWorld>();
				myworld._ID = this.OldID;
			}
		}
	}
}
