using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Sandbox.ui
{
	public class HudStuff : Panel
	{
		public Label GravityLabel;
		public Panel GravityBar;
		public HudStuff()
		{
			StyleSheet.Load( "/ui/HudStuff.scss" );

			GravityBar = Add.Panel( "gravitybar" );
			GravityLabel = GravityBar.Add.Label( "Gravity Speed: ", "gravity" );
			GravityBar.Style.Dirty();
		}

		public void Tick( Entity pl )
		{
			if ( pl is SandboxPlayer )
			{
				var player = (SandboxPlayer)pl;

				var gravspeed = player.GravitySpeed;

				GravityLabel.Text = $"Gravity Speed: {gravspeed}";
			}


		}

	}
}
