using UnityEngine;

namespace THEBADDEST.GameDebugSystem
{
	[CreateAssetMenu(fileName = "GameplayCheatWrapper", menuName = "Cheats/Gameplay Wrapper", order = 10)]
	public class GameplayDebugWrapper : DebugWrapperBase
	{
		public override void RegisterCheats(DebugUIBuilder builder)
		{
			// Example button
			builder.AddButton("Give 100 Gold", () =>
			{
				Debug.Log("[Cheats] Granted 100 gold.");
				// GameSystems.Inventory.AddGold(100);
			});

			// // Example slider
			builder.AddSlider("Player Speed", 1f, 10f, 3f, (v) =>
			{
				Debug.Log($"[Cheats] Player speed -> {v:0.##}");
				// PlayerController.Instance.SetSpeed(v);
			});
			
			// Example input
			builder.AddInputField("Teleport To", "x,y,z", (pos) =>
			{
				Debug.Log($"[Cheats] Teleport to -> {pos}");
				// Parse and teleport: var p = Vector3Parser.Parse(pos); Player.Teleport(p);
			});
			builder.AddNumberField("Speed",0,100,1,1, x =>
			{
				Debug.Log($"[Cheats] Teleport to -> {x}");
			});
			builder.AddToggle("Bool",true, b => { Debug.Log($"Bool Value : {b}");});
		}
	}
}


