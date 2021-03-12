using System.Collections.Generic;
using XenoCore.Override.Map.Components;

namespace Military.Component {
	public class PointComponent : PseudoComponent {
	}
	
	public class PointComponentBuilder : PseudoComponentBuilder {
		public override string TypeId => "point";
		
		public override PseudoComponent Build(Dictionary<string, string> Options) {
			return new PointComponent();
		}
	}
}