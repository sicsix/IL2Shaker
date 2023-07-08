using System.Diagnostics.CodeAnalysis;
using IL2ShakerDriver;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace IL2ShakerUI.Models;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal class EffectSettings : ReactiveObject, IEffectSettings
{
    [Reactive]
    public bool Enabled { get; set; }

    [Reactive]
    public int Value { get; set; }
}