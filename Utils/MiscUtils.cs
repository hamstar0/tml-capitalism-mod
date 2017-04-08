using System.Collections.Generic;
using System.Linq;


namespace Utils {
	public static class MiscUtils {
		public static string DictToString( IDictionary<object, object> dict ) {
			return string.Join( ";", dict.Select( x => x.Key + "=" + x.Value ).ToArray() );
		}
	}
}
