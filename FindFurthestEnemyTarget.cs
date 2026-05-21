using System;
using System.Runtime.InteropServices;
using Landfall.TABS.AI.Components.Tags;
using Unity.Entities;

namespace HiddenUnits;

[Serializable]
[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct FindFurthestEnemyTarget : ITargetingComponent, IComponentData
{
}