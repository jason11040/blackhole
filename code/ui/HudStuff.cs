using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Sandbox.ui
{
	public class HudStuff : Panel
	{
		public Panel StatsBG;
		public Panel StatsBG2;
		public Label StatsLabel;
		public Label GravityLabel;
		public Panel GravityBar;
		public Label DistLabel;
		public Panel DistBar;
		public HudStuff()
		{
			StyleSheet.Load( "/ui/HudStuff.scss" );
			StatsBG = Add.Panel( "StatsBG" );
			StatsBG2 = StatsBG.Add.Panel( "StatsBG2" );
			DistBar = StatsBG.Add.Panel( "Distbar" );
			DistLabel = DistBar.Add.Label( "Distance: ", "Dist" );
			GravityBar = StatsBG.Add.Panel( "gravitybar" );
			GravityLabel = GravityBar.Add.Label( "Gravity Speed: ", "gravity" );
			StatsLabel = StatsBG.Add.Label( "BlackHole Settings", "StatsLabel" );
			GravityBar.Style.Dirty();
		}

		public void UpdateGravity(int gravspeed, int Dist )
		{
			GravityLabel.Text = $"Gravity Speed: {gravspeed}";
			DistLabel.Text = $"Distance: {Dist}";
		}

	}
}
