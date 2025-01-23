internal static class SR
{
    public static string GetString(string name) => name;
    
    public static string GetString(string name, params object?[] args) => string.Format(System.Globalization.CultureInfo.InvariantCulture, name, args);
    
    public static string Format(string name, params object?[] args) => string.Format(System.Globalization.CultureInfo.InvariantCulture, name, args);
    
    public const string Xml_InvalidNameChars = nameof(Xml_InvalidNameChars);
    public const string XmlChoiceMissingAnyValue = nameof(XmlChoiceMissingAnyValue);
    public const string XmlChoiceMissingValue = nameof(XmlChoiceMissingValue);
    public const string XmlCircularDerivation = nameof(XmlCircularDerivation);
    public const string XmlConstructorInaccessible = nameof(XmlConstructorInaccessible);
    public const string XmlGetSchemaEmptyTypeName = nameof(XmlGetSchemaEmptyTypeName);
    public const string XmlGetSchemaInclude = nameof(XmlGetSchemaInclude);
    public const string XmlGetSchemaMethodReturnType = nameof(XmlGetSchemaMethodReturnType);
    public const string XmlGetSchemaTypeMissing = nameof(XmlGetSchemaTypeMissing);
    public const string XmlHiddenMember = nameof(XmlHiddenMember);
    public const string XmlIllegalSimpleContentExtension = nameof(XmlIllegalSimpleContentExtension);
    public const string XmlIllegalTypedTextAttribute = nameof(XmlIllegalTypedTextAttribute);
    public const string XmlInternalError = nameof(XmlInternalError);
    public const string XmlInternalErrorDetails = nameof(XmlInternalErrorDetails);
    public const string XmlInternalErrorMethod = nameof(XmlInternalErrorMethod);
    public const string XmlInvalidArrayTypeSyntax = nameof(XmlInvalidArrayTypeSyntax);
    public const string XmlInvalidIdentifier = nameof(XmlInvalidIdentifier);
    public const string XmlInvalidXmlOverride = nameof(XmlInvalidXmlOverride);
    public const string XmlMelformMapping = nameof(XmlMelformMapping);
    public const string XmlMissingSchema = nameof(XmlMissingSchema);
    public const string XmlNoAddMethod = nameof(XmlNoAddMethod);
    public const string XmlNoDefaultAccessors = nameof(XmlNoDefaultAccessors);
    public const string XmlPregenOrphanType = nameof(XmlPregenOrphanType);
    public const string XmlPregenTypeDynamic = nameof(XmlPregenTypeDynamic);
    public const string XmlReadOnlyPropertyError = nameof(XmlReadOnlyPropertyError);
    public const string XmlSerializableNameMissing1 = nameof(XmlSerializableNameMissing1);
    public const string XmlSerializableRootDupName = nameof(XmlSerializableRootDupName);
    public const string XmlSerializableSchemaError = nameof(XmlSerializableSchemaError);
    public const string XmlSerializerUnsupportedType = nameof(XmlSerializerUnsupportedType);
    public const string XmlTypeInaccessible = nameof(XmlTypeInaccessible);
    public const string XmlTypeStatic = nameof(XmlTypeStatic);
    public const string XmlUnsupportedDefaultType = nameof(XmlUnsupportedDefaultType);
    public const string XmlUnsupportedIDictionary = nameof(XmlUnsupportedIDictionary);
    public const string XmlUnsupportedIDictionaryDetails = nameof(XmlUnsupportedIDictionaryDetails);
    public const string XmlUnsupportedInterface = nameof(XmlUnsupportedInterface);
    public const string XmlUnsupportedInterfaceDetails = nameof(XmlUnsupportedInterfaceDetails);
    public const string XmlUnsupportedOpenGenericType = nameof(XmlUnsupportedOpenGenericType);
    public const string XmlUnsupportedRank = nameof(XmlUnsupportedRank);
}
