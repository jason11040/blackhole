using Sandbox;
using Sandbox.ui;
using Sandbox.UI;

[Library]
public partial class SandboxHud : HudEntity<RootPanel>
{
	public static HudStuff hudInfo;
	public SandboxHud()
	{
		if ( !IsClient )
			return;

		RootPanel.StyleSheet.Load( "/ui/SandboxHud.scss" );

		RootPanel.AddChild<NameTags>();
		RootPanel.AddChild<CrosshairCanvas>();
		RootPanel.AddChild<ChatBox>();
		RootPanel.AddChild<VoiceList>();
		RootPanel.AddChild<KillFeed>();
		RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
		RootPanel.AddChild<Health>();
		hudInfo = RootPanel.AddChild<HudStuff>();
		RootPanel.AddChild<InventoryBar>();
	}
	[ClientRpc]
	public static void UpdateGravityUI( int gravspeed, int dist )
	{
		hudInfo.UpdateGravity( gravspeed, dist );
	}
}
