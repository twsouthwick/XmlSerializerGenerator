internal static class SR
{
    public static string GetString(string name) => name;
    
    public static string GetString(string name, params object?[] args) => string.Format(System.Globalization.CultureInfo.InvariantCulture, name, args);
    
    public static string Format(string name, params object?[] args) => string.Format(System.Globalization.CultureInfo.InvariantCulture, name, args);
    
    public const string Arg_ReflectionOnlyCA = nameof(Arg_ReflectionOnlyCA);
    public const string Xml_InvalidNameChars = nameof(Xml_InvalidNameChars);
    public const string XmlAnonymousInclude = nameof(XmlAnonymousInclude);
    public const string XmlAnyElementDuplicate = nameof(XmlAnyElementDuplicate);
    public const string XmlArrayItemAmbiguousTypes = nameof(XmlArrayItemAmbiguousTypes);
    public const string XmlCannotReconcileAccessor = nameof(XmlCannotReconcileAccessor);
    public const string XmlCannotReconcileAccessorDefault = nameof(XmlCannotReconcileAccessorDefault);
    public const string XmlCannotReconcileAttributeAccessor = nameof(XmlCannotReconcileAttributeAccessor);
    public const string XmlChoiceIdDuplicate = nameof(XmlChoiceIdDuplicate);
    public const string XmlChoiceIdentifierAmbiguous = nameof(XmlChoiceIdentifierAmbiguous);
    public const string XmlChoiceIdentifierArrayType = nameof(XmlChoiceIdentifierArrayType);
    public const string XmlChoiceIdentifierMemberMissing = nameof(XmlChoiceIdentifierMemberMissing);
    public const string XmlChoiceIdentifierMissing = nameof(XmlChoiceIdentifierMissing);
    public const string XmlChoiceIdentifierType = nameof(XmlChoiceIdentifierType);
    public const string XmlChoiceIdentifierTypeEnum = nameof(XmlChoiceIdentifierTypeEnum);
    public const string XmlChoiceMissingAnyValue = nameof(XmlChoiceMissingAnyValue);
    public const string XmlChoiceMissingValue = nameof(XmlChoiceMissingValue);
    public const string XmlCircularDerivation = nameof(XmlCircularDerivation);
    public const string XmlConstructorInaccessible = nameof(XmlConstructorInaccessible);
    public const string XmlDataTypeMismatch = nameof(XmlDataTypeMismatch);
    public const string XmlDuplicateAttributeName = nameof(XmlDuplicateAttributeName);
    public const string XmlDuplicateElementName = nameof(XmlDuplicateElementName);
    public const string XmlFieldReflectionError = nameof(XmlFieldReflectionError);
    public const string XmlGetSchemaEmptyTypeName = nameof(XmlGetSchemaEmptyTypeName);
    public const string XmlGetSchemaInclude = nameof(XmlGetSchemaInclude);
    public const string XmlGetSchemaMethodMissing = nameof(XmlGetSchemaMethodMissing);
    public const string XmlGetSchemaMethodName = nameof(XmlGetSchemaMethodName);
    public const string XmlGetSchemaMethodReturnType = nameof(XmlGetSchemaMethodReturnType);
    public const string XmlGetSchemaTypeMissing = nameof(XmlGetSchemaTypeMissing);
    public const string XmlHiddenMember = nameof(XmlHiddenMember);
    public const string XmlIllegalAnyElement = nameof(XmlIllegalAnyElement);
    public const string XmlIllegalArrayArrayAttribute = nameof(XmlIllegalArrayArrayAttribute);
    public const string XmlIllegalArrayTextAttribute = nameof(XmlIllegalArrayTextAttribute);
    public const string XmlIllegalAttribute = nameof(XmlIllegalAttribute);
    public const string XmlIllegalAttributeFlagsArray = nameof(XmlIllegalAttributeFlagsArray);
    public const string XmlIllegalAttributesArrayAttribute = nameof(XmlIllegalAttributesArrayAttribute);
    public const string XmlIllegalAttrOrText = nameof(XmlIllegalAttrOrText);
    public const string XmlIllegalAttrOrTextInterface = nameof(XmlIllegalAttrOrTextInterface);
    public const string XmlIllegalDefault = nameof(XmlIllegalDefault);
    public const string XmlIllegalElementsArrayAttribute = nameof(XmlIllegalElementsArrayAttribute);
    public const string XmlIllegalMultipleText = nameof(XmlIllegalMultipleText);
    public const string XmlIllegalMultipleTextMembers = nameof(XmlIllegalMultipleTextMembers);
    public const string XmlIllegalSimpleContentExtension = nameof(XmlIllegalSimpleContentExtension);
    public const string XmlIllegalType = nameof(XmlIllegalType);
    public const string XmlIllegalTypeContext = nameof(XmlIllegalTypeContext);
    public const string XmlIllegalTypedTextAttribute = nameof(XmlIllegalTypedTextAttribute);
    public const string XmlInternalError = nameof(XmlInternalError);
    public const string XmlInternalErrorDetails = nameof(XmlInternalErrorDetails);
    public const string XmlInternalErrorMethod = nameof(XmlInternalErrorMethod);
    public const string XmlInvalidArrayTypeSyntax = nameof(XmlInvalidArrayTypeSyntax);
    public const string XmlInvalidAttributeType = nameof(XmlInvalidAttributeType);
    public const string XmlInvalidAttributeUse = nameof(XmlInvalidAttributeUse);
    public const string XmlInvalidConstantAttribute = nameof(XmlInvalidConstantAttribute);
    public const string XmlInvalidDataTypeUsage = nameof(XmlInvalidDataTypeUsage);
    public const string XmlInvalidDefaultValue = nameof(XmlInvalidDefaultValue);
    public const string XmlInvalidFormUnqualified = nameof(XmlInvalidFormUnqualified);
    public const string XmlInvalidIdentifier = nameof(XmlInvalidIdentifier);
    public const string XmlInvalidIsNullable = nameof(XmlInvalidIsNullable);
    public const string XmlInvalidNotNullable = nameof(XmlInvalidNotNullable);
    public const string XmlInvalidReturnPosition = nameof(XmlInvalidReturnPosition);
    public const string XmlInvalidSpecifiedType = nameof(XmlInvalidSpecifiedType);
    public const string XmlInvalidTypeAttributes = nameof(XmlInvalidTypeAttributes);
    public const string XmlInvalidXmlOverride = nameof(XmlInvalidXmlOverride);
    public const string XmlInvalidXsdDataType = nameof(XmlInvalidXsdDataType);
    public const string XmlMelformMapping = nameof(XmlMelformMapping);
    public const string XmlMethodTypeNameConflict = nameof(XmlMethodTypeNameConflict);
    public const string XmlMissingMethodEnum = nameof(XmlMissingMethodEnum);
    public const string XmlMissingSchema = nameof(XmlMissingSchema);
    public const string XmlMultipleXmlns = nameof(XmlMultipleXmlns);
    public const string XmlMultipleXmlnsMembers = nameof(XmlMultipleXmlnsMembers);
    public const string XmlNoAddMethod = nameof(XmlNoAddMethod);
    public const string XmlNoDefaultAccessors = nameof(XmlNoDefaultAccessors);
    public const string XmlNoSerializableMembers = nameof(XmlNoSerializableMembers);
    public const string XmlPregenOrphanType = nameof(XmlPregenOrphanType);
    public const string XmlPregenTypeDynamic = nameof(XmlPregenTypeDynamic);
    public const string XmlPropertyReflectionError = nameof(XmlPropertyReflectionError);
    public const string XmlReadOnlyPropertyError = nameof(XmlReadOnlyPropertyError);
    public const string XmlReflectionError = nameof(XmlReflectionError);
    public const string XmlRpcLitArrayElement = nameof(XmlRpcLitArrayElement);
    public const string XmlRpcLitAttributeAttributes = nameof(XmlRpcLitAttributeAttributes);
    public const string XmlRpcLitAttributes = nameof(XmlRpcLitAttributes);
    public const string XmlRpcLitElementNamespace = nameof(XmlRpcLitElementNamespace);
    public const string XmlRpcLitElementNullable = nameof(XmlRpcLitElementNullable);
    public const string XmlRpcLitElements = nameof(XmlRpcLitElements);
    public const string XmlRpcLitXmlns = nameof(XmlRpcLitXmlns);
    public const string XmlSequenceHierarchy = nameof(XmlSequenceHierarchy);
    public const string XmlSequenceInconsistent = nameof(XmlSequenceInconsistent);
    public const string XmlSequenceMatch = nameof(XmlSequenceMatch);
    public const string XmlSequenceMembers = nameof(XmlSequenceMembers);
    public const string XmlSequenceUnique = nameof(XmlSequenceUnique);
    public const string XmlSerializableAttributes = nameof(XmlSerializableAttributes);
    public const string XmlSerializableNameMissing1 = nameof(XmlSerializableNameMissing1);
    public const string XmlSerializableRootDupName = nameof(XmlSerializableRootDupName);
    public const string XmlSerializableSchemaError = nameof(XmlSerializableSchemaError);
    public const string XmlSerializerUnsupportedMember = nameof(XmlSerializerUnsupportedMember);
    public const string XmlSerializerUnsupportedType = nameof(XmlSerializerUnsupportedType);
    public const string XmlSoleXmlnsAttribute = nameof(XmlSoleXmlnsAttribute);
    public const string XmlTypeInaccessible = nameof(XmlTypeInaccessible);
    public const string XmlTypeReflectionError = nameof(XmlTypeReflectionError);
    public const string XmlTypesDuplicate = nameof(XmlTypesDuplicate);
    public const string XmlTypeStatic = nameof(XmlTypeStatic);
    public const string XmlUdeclaredXsdType = nameof(XmlUdeclaredXsdType);
    public const string XmlUnsupportedDefaultType = nameof(XmlUnsupportedDefaultType);
    public const string XmlUnsupportedIDictionary = nameof(XmlUnsupportedIDictionary);
    public const string XmlUnsupportedIDictionaryDetails = nameof(XmlUnsupportedIDictionaryDetails);
    public const string XmlUnsupportedInheritance = nameof(XmlUnsupportedInheritance);
    public const string XmlUnsupportedInterface = nameof(XmlUnsupportedInterface);
    public const string XmlUnsupportedInterfaceDetails = nameof(XmlUnsupportedInterfaceDetails);
    public const string XmlUnsupportedOpenGenericType = nameof(XmlUnsupportedOpenGenericType);
    public const string XmlUnsupportedRank = nameof(XmlUnsupportedRank);
    public const string XmlUnsupportedTypeKind = nameof(XmlUnsupportedTypeKind);
    public const string XmlXmlnsInvalidType = nameof(XmlXmlnsInvalidType);
}
