namespace THEBADDEST.GameDebugSystem
{
	public interface IDebugWrapper
	{
		string CategoryName { get; }
		void RegisterCheats(DebugUIBuilder builder);
	}


}


