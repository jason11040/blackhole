using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Sandbox.ui
{
	public class HudStuff : Panel
	{
		public Label GravityLabel;
		public Panel GravityBar;
		public Label DistLabel;
		public Panel DistBar;
		public HudStuff()
		{
			StyleSheet.Load( "/ui/HudStuff.scss" );

			DistBar = Add.Panel( "Distbar" );
			DistLabel = DistBar.Add.Label( "Distance: ", "Dist" );
			GravityBar = Add.Panel( "gravitybar" );
			GravityLabel = GravityBar.Add.Label( "Gravity Speed: ", "gravity" );
			GravityBar.Style.Dirty();
		}

		public void UpdateGravity(int gravspeed, int Dist )
		{
			GravityLabel.Text = $"Gravity Speed: {gravspeed}";
			DistLabel.Text = $"Distance: {Dist}";
		}

	}
}
