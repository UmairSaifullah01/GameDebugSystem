using System;
using UnityEngine;

namespace THEBADDEST.GameDebugSystem
{
	public abstract class DebugWrapperBase : ScriptableObject, IDebugWrapper
	{
		[SerializeField] private string _categoryName = "Cheats";

		public virtual string CategoryName => string.IsNullOrWhiteSpace(_categoryName) ? name : _categoryName;

		public abstract void RegisterCheats(DebugUIBuilder builder);
	}
}


