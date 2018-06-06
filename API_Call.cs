using System;


namespace Capitalism {
	public static partial class CapitalismAPI {
		internal static object Call( string call_type, params object[] args ) {
			switch( call_type ) {
			case "GetModSettings":
				return CapitalismAPI.GetModSettings();
			default:
				throw new Exception( "No such api call " + call_type );
			}
		}
	}
}
