using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Xml.Serialization;

internal sealed class ModelScope
{
    private readonly TypeScope _typeScope;
    private readonly Dictionary<Type, TypeModel> _models = new Dictionary<Type, TypeModel>();
    private readonly Dictionary<Type, TypeModel> _arrayModels = new Dictionary<Type, TypeModel>();

    internal ModelScope(TypeScope typeScope)
    {
        _typeScope = typeScope;
    }

    internal TypeScope TypeScope
    {
        get { return _typeScope; }
    }

    [RequiresUnreferencedCode("calls GetTypeModel")]
    internal TypeModel GetTypeModel(Type type)
    {
        return GetTypeModel(type, true);
    }

    [RequiresUnreferencedCode("calls GetTypeDesc")]
    internal TypeModel GetTypeModel(Type type, bool directReference)
    {
        TypeModel? model;
        if (_models.TryGetValue(type, out model))
            return model;
        TypeDesc typeDesc = _typeScope.GetTypeDesc(type, null, directReference);

        switch (typeDesc.Kind)
        {
            case TypeKind.Enum:
                model = new EnumModel(type, typeDesc, this);
                break;
            case TypeKind.Primitive:
                model = new PrimitiveModel(type, typeDesc, this);
                break;
            case TypeKind.Array:
            case TypeKind.Collection:
            case TypeKind.Enumerable:
                model = new ArrayModel(type, typeDesc, this);
                break;
            case TypeKind.Root:
            case TypeKind.Class:
            case TypeKind.Struct:
                model = new StructModel(type, typeDesc, this);
                break;
            default:
                if (!typeDesc.IsSpecial) throw new NotSupportedException(SR.Format(SR.XmlUnsupportedTypeKind, type.FullName));
                model = new SpecialModel(type, typeDesc, this);
                break;
        }

        _models.Add(type, model);
        return model;
    }

    [RequiresUnreferencedCode("calls GetArrayTypeDesc")]
    internal ArrayModel GetArrayModel(Type type)
    {
        TypeModel? model;
        if (!_arrayModels.TryGetValue(type, out model))
        {
            model = GetTypeModel(type);
            if (!(model is ArrayModel))
            {
                TypeDesc typeDesc = _typeScope.GetArrayTypeDesc(type);
                model = new ArrayModel(type, typeDesc, this);
            }
            _arrayModels.Add(type, model);
        }
        return (ArrayModel)model;
    }
}
