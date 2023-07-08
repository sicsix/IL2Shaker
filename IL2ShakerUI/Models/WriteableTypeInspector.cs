using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace IL2ShakerUI.Models;

internal class WriteableTypeInspector : TypeInspectorSkeleton
{
    private readonly ITypeInspector _inspector;

    public WriteableTypeInspector(ITypeInspector inspector)
    {
        _inspector = inspector;
    }

    public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
    {
        // Strips out the reactive properties from the base class since they can't be written to
        var props = _inspector.GetProperties(type, container);
        return props.Where(o => o.CanWrite);
    }
}