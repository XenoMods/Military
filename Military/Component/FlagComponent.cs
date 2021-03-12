using System.Collections.Generic;
using XenoCore.Override.Map.Components;

namespace Military.Component {
	public class FlagComponent : PseudoComponent {
		public bool For2;
		public bool For3;
		public bool For4;
	}
	
	public class FlagComponentBuilder : PseudoComponentBuilder {
		public override string TypeId => "flag";
		
		public override PseudoComponent Build(Dictionary<string, string> Options) {
			return new FlagComponent {
				For2 = ToBool(Options["for2"]),
				For3 = ToBool(Options["for3"]),
				For4 = ToBool(Options["for4"]),
			};
		}
	}
}